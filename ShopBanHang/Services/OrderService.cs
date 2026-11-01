using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class OrderService : IOrderService
    {
        private readonly ShopContext _db;

        public OrderService(ShopContext db) => _db = db;

        public async Task<PagedResult<Order>> LayDanhSachAsync(string? tuKhoa, OrderStatus? trangThai,
                                                               int page, int pageSize)
        {
            IQueryable<Order> q = _db.Orders.Include(o => o.User).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim();
                // Tra theo mã đơn, tên người nhận hoặc số điện thoại
                q = q.Where(o => o.OrderCode.Contains(tk)
                              || o.CustomerName.Contains(tk)
                              || o.Phone.Contains(tk));
            }
            if (trangThai.HasValue) q = q.Where(o => o.Status == trangThai.Value);

            q = q.OrderByDescending(o => o.CreatedAt);

            return await PagedResult<Order>.TaoAsync(q, page, pageSize);
        }

        public async Task<Order?> LayChiTietAsync(int id)
            => await _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id, OrderStatus moi)
        {
            // Không AsNoTracking vì còn phải sửa và lưu lại
            var don = await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (don == null) return (false, "Không tìm thấy đơn hàng.");

            // Luật chuyển trạng thái nằm trong OrderStatusFlow, không rải if ở đây
            if (!OrderStatusFlow.ChoPhepChuyen(don.Status, moi))
                return (false, $"Không thể chuyển từ \"{don.Status.Nhan()}\" sang \"{moi.Nhan()}\".");

            don.Status = moi;

            // Chỉ cộng số đã bán khi đơn thật sự hoàn thành.
            // Cộng sớm từ lúc xác nhận thì đơn bị huỷ giữa đường sẽ làm sai thống kê.
            if (moi == OrderStatus.HoanThanh)
            {
                foreach (var dong in don.OrderDetails)
                {
                    var sp = await _db.Products.FindAsync(dong.ProductId);
                    if (sp != null) sp.SoldCount += dong.Quantity;
                }
            }

            // Trừ tồn kho sẽ làm ở bước đặt hàng phía khách (giai đoạn sau),
            // nên ở đây chưa có gì phải hoàn lại khi huỷ đơn.

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<int> DemAsync() => await _db.Orders.CountAsync();

        public async Task<int> DemTheoTrangThaiAsync(OrderStatus trangThai)
            => await _db.Orders.CountAsync(o => o.Status == trangThai);
    }
}
