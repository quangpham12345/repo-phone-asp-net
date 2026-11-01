using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ShopContext _db;

        public ReviewService(ShopContext db) => _db = db;

        public async Task<List<Review>> LayCuaSanPhamAsync(int productId)
            => await _db.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

        public async Task<(double trungBinh, int soLuong)> ThongKeAsync(int productId)
        {
            var diem = await _db.Reviews
                .Where(r => r.ProductId == productId)
                .Select(r => r.Rating)
                .ToListAsync();

            return diem.Count == 0 ? (0, 0) : (Math.Round(diem.Average(), 1), diem.Count);
        }

        /// <summary>
        /// Khách được coi là đã mua khi có dòng hàng của sản phẩm nằm trong một
        /// đơn đã Hoàn thành của chính khách đó. Đơn đang giao hay đã huỷ không tính.
        /// </summary>
        public async Task<bool> DaMuaAsync(int productId, int userId)
            => await _db.OrderDetails
                .AnyAsync(d => d.ProductId == productId
                            && d.Order!.UserId == userId
                            && d.Order.Status == OrderStatus.HoanThanh);

        public async Task<Review?> LayCuaToiAsync(int productId, int userId)
            => await _db.Reviews.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

        public async Task<(bool ok, string? loi)> LuuAsync(int productId, int userId, int soSao, string? nhanXet)
        {
            if (soSao is < 1 or > 5) return (false, "Số sao phải từ 1 đến 5.");

            if (!await _db.Products.AnyAsync(p => p.Id == productId))
                return (false, "Không tìm thấy sản phẩm.");

            // Chốt chặn thật nằm ở đây, không chỉ ở chỗ ẩn hiện form trên giao diện
            if (!await DaMuaAsync(productId, userId))
                return (false, "Chỉ khách đã mua và nhận hàng thành công mới được đánh giá sản phẩm này.");

            var cu = await LayCuaToiAsync(productId, userId);
            if (cu == null)
            {
                _db.Reviews.Add(new Review
                {
                    ProductId = productId,
                    UserId = userId,
                    Rating = soSao,
                    Comment = nhanXet?.Trim(),
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                // Đánh giá lại thì sửa bản cũ, vì mỗi người chỉ có một đánh giá cho một sản phẩm
                cu.Rating = soSao;
                cu.Comment = nhanXet?.Trim();
                cu.CreatedAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> XoaCuaToiAsync(int productId, int userId)
        {
            var r = await LayCuaToiAsync(productId, userId);
            if (r == null) return (false, "Bạn chưa đánh giá sản phẩm này.");

            _db.Reviews.Remove(r);
            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}
