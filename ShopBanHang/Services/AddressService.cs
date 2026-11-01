using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Models.Entities;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class AddressService : IAddressService
    {
        private readonly ShopContext _db;

        public AddressService(ShopContext db) => _db = db;

        public async Task<List<Address>> LayCuaUserAsync(int userId)
            => await _db.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)     // mặc định lên đầu
                .ThenByDescending(a => a.Id)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Address?> LayMacDinhAsync(int userId)
            => await _db.Addresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .AsNoTracking()
                .FirstOrDefaultAsync();

        /// <summary>Chỉ trả về nếu đúng là địa chỉ của người đang hỏi, tránh xem trộm của người khác.</summary>
        public async Task<Address?> LayTheoIdAsync(int id, int userId)
            => await _db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

        public async Task<(bool ok, string? loi)> LuuAsync(Address dc, int userId)
        {
            // Không tin UserId gửi lên từ form, luôn gán theo người đang đăng nhập
            dc.UserId = userId;

            bool laDiaChiDauTien = !await _db.Addresses.AnyAsync(a => a.UserId == userId);

            if (dc.Id == 0)
            {
                dc.CreatedAt = DateTime.Now;

                // Địa chỉ đầu tiên luôn là mặc định, nếu không khách sẽ không có
                // địa chỉ nào được chọn sẵn lúc đặt hàng
                if (laDiaChiDauTien) dc.IsDefault = true;

                _db.Addresses.Add(dc);
            }
            else
            {
                var cu = await LayTheoIdAsync(dc.Id, userId);
                if (cu == null) return (false, "Không tìm thấy địa chỉ cần sửa.");

                cu.ReceiverName = dc.ReceiverName.Trim();
                cu.Phone = dc.Phone.Trim();
                cu.Detail = dc.Detail.Trim();
                cu.IsDefault = dc.IsDefault || laDiaChiDauTien;
                dc = cu;
            }

            await _db.SaveChangesAsync();

            // Mỗi người chỉ một địa chỉ mặc định, nên gỡ cờ ở những cái còn lại
            if (dc.IsDefault) await GoCoMacDinhKhac(userId, dc.Id);

            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DatMacDinhAsync(int id, int userId)
        {
            var dc = await LayTheoIdAsync(id, userId);
            if (dc == null) return (false, "Không tìm thấy địa chỉ.");

            dc.IsDefault = true;
            await _db.SaveChangesAsync();
            await GoCoMacDinhKhac(userId, id);

            return (true, null);
        }

        public async Task<(bool ok, string? loi)> XoaAsync(int id, int userId)
        {
            var dc = await LayTheoIdAsync(id, userId);
            if (dc == null) return (false, "Không tìm thấy địa chỉ.");

            bool laMacDinh = dc.IsDefault;
            _db.Addresses.Remove(dc);
            await _db.SaveChangesAsync();

            // Xoá mất địa chỉ mặc định thì đẩy cái mới nhất còn lại lên thay,
            // để khách không rơi vào cảnh không có địa chỉ nào được chọn sẵn
            if (laMacDinh)
            {
                var conLai = await _db.Addresses
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync();

                if (conLai != null)
                {
                    conLai.IsDefault = true;
                    await _db.SaveChangesAsync();
                }
            }

            return (true, null);
        }

        private async Task GoCoMacDinhKhac(int userId, int idGiuLai)
        {
            var khac = await _db.Addresses
                .Where(a => a.UserId == userId && a.Id != idGiuLai && a.IsDefault)
                .ToListAsync();

            if (khac.Count == 0) return;

            foreach (var a in khac) a.IsDefault = false;
            await _db.SaveChangesAsync();
        }
    }
}
