using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Repositories.Interfaces;

namespace ShopBanHang.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ShopContext _db;

        public OrderRepository(ShopContext db) => _db = db;

        public async Task<PagedResult<Order>> TimKiemAsync(OrderFilter loc, int page, int pageSize)
        {
            IQueryable<Order> q = _db.Orders.Include(o => o.User).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(loc.TuKhoa))
            {
                string tk = loc.TuKhoa.Trim();
                // Tra theo mã đơn, tên người nhận hoặc số điện thoại
                q = q.Where(o => o.OrderCode.Contains(tk)
                              || o.CustomerName.Contains(tk)
                              || o.Phone.Contains(tk));
            }

            if (loc.TrangThai.HasValue) q = q.Where(o => o.Status == loc.TrangThai.Value);
            if (loc.PhuongThuc.HasValue) q = q.Where(o => o.PaymentMethod == loc.PhuongThuc.Value);
            if (loc.DaThanhToan.HasValue) q = q.Where(o => o.IsPaid == loc.DaThanhToan.Value);

            if (loc.TuNgay.HasValue) q = q.Where(o => o.CreatedAt >= loc.TuNgay.Value.Date);
            // Lấy hết ngày cuối: nhỏ hơn 0 giờ của ngày hôm sau
            if (loc.DenNgay.HasValue) q = q.Where(o => o.CreatedAt < loc.DenNgay.Value.Date.AddDays(1));

            q = q.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);

            return await PagedResult<Order>.TaoAsync(q, page, pageSize);
        }

        public async Task<PagedResult<Order>> LayCuaUserAsync(int userId, OrderStatus? trangThai, int page, int pageSize)
        {
            IQueryable<Order> q = _db.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.ProductVariant)
                .AsNoTracking()
                .AsSplitQuery()
                .Where(o => o.UserId == userId);

            if (trangThai.HasValue) q = q.Where(o => o.Status == trangThai.Value);

            q = q.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id);

            return await PagedResult<Order>.TaoAsync(q, page, pageSize);
        }

        public async Task<Order?> LayChiTietAsync(int id)
            => await QueryChiTiet().FirstOrDefaultAsync(o => o.Id == id);

        public async Task<Order?> LayChiTietTheoMaAsync(string orderCode, int userId)
            => await QueryChiTiet().FirstOrDefaultAsync(o => o.OrderCode == orderCode && o.UserId == userId);

        public async Task<Order?> LayChiTietTheoMaAsync(string orderCode)
            => await QueryChiTiet().FirstOrDefaultAsync(o => o.OrderCode == orderCode);

        public async Task<Order?> LayDeSuaAsync(int id)
            => await _db.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

        public async Task<string?> LayMaLonNhatAsync(string tienTo)
            => await _db.Orders
                .Where(o => o.OrderCode.StartsWith(tienTo))
                .OrderByDescending(o => o.OrderCode)
                .Select(o => o.OrderCode)
                .FirstOrDefaultAsync();

        public async Task<int> DemTheoTrangThaiAsync(OrderStatus trangThai)
            => await _db.Orders.CountAsync(o => o.Status == trangThai);

        public async Task<Dictionary<OrderStatus, int>> DemCuaUserTheoTrangThaiAsync(int userId)
            => await _db.Orders
                .Where(o => o.UserId == userId)
                .GroupBy(o => o.Status)
                .Select(g => new { g.Key, SoLuong = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.SoLuong);

        public async Task<int> DemAsync() => await _db.Orders.CountAsync();

        public void Them(Order don) => _db.Orders.Add(don);

        private IQueryable<Order> QueryChiTiet()
            => _db.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.ProductVariant)
                .AsNoTracking()
                .AsSplitQuery();
    }
}
