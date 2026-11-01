using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Models.Entities;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class BannerService : IBannerService
    {
        private readonly ShopContext _db;

        public BannerService(ShopContext db) => _db = db;

        public async Task<List<Banner>> LayDangHienAsync()
            => await _db.Banners
                .Where(b => b.IsActive)
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.Id)
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<Banner>> LayTatCaAsync()
            => await _db.Banners
                .OrderBy(b => b.SortOrder)
                .ThenBy(b => b.Id)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Banner?> LayTheoIdAsync(int id)
            => await _db.Banners.FindAsync(id);

        public async Task<(bool ok, string? loi)> LuuAsync(Banner banner)
        {
            if (string.IsNullOrWhiteSpace(banner.ImageUrl))
                return (false, "Banner phải có ảnh.");

            if (banner.Id == 0)
            {
                banner.CreatedAt = DateTime.Now;
                _db.Banners.Add(banner);
            }
            else
            {
                var cu = await _db.Banners.FindAsync(banner.Id);
                if (cu == null) return (false, "Không tìm thấy banner cần sửa.");

                cu.Title = banner.Title.Trim();
                cu.ImageUrl = banner.ImageUrl.Trim();
                cu.LinkUrl = banner.LinkUrl?.Trim();
                cu.SortOrder = banner.SortOrder;
                cu.IsActive = banner.IsActive;
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> XoaAsync(int id)
        {
            var b = await _db.Banners.FindAsync(id);
            if (b == null) return (false, "Không tìm thấy banner.");

            // Banner không bị bảng nào tham chiếu nên xoá hẳn được, khác với sản phẩm và tài khoản
            _db.Banners.Remove(b);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id)
        {
            var b = await _db.Banners.FindAsync(id);
            if (b == null) return (false, "Không tìm thấy banner.");

            b.IsActive = !b.IsActive;
            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}
