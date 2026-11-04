using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Models.Entities;
using ShopBanHang.Repositories.Interfaces;

namespace ShopBanHang.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ShopContext _db;

        public CartRepository(ShopContext db) => _db = db;

        public async Task<List<CartItem>> LayCuaUserAsync(int userId)
            => await _db.CartItems
                .Include(c => c.ProductVariant)
                    .ThenInclude(v => v!.Product)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

        public async Task<CartItem?> LayDongAsync(int userId, int variantId)
            => await _db.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductVariantId == variantId);

        public async Task<CartItem?> LayTheoIdAsync(int id, int userId)
            => await _db.CartItems
                .Include(c => c.ProductVariant)
                    .ThenInclude(v => v!.Product)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        public async Task<int> DemSoLuongAsync(int userId)
            => await _db.CartItems.Where(c => c.UserId == userId).SumAsync(c => (int?)c.Quantity) ?? 0;

        public void Them(CartItem item) => _db.CartItems.Add(item);

        public void Xoa(CartItem item) => _db.CartItems.Remove(item);

        public void XoaNhieu(IEnumerable<CartItem> items) => _db.CartItems.RemoveRange(items);
    }
}
