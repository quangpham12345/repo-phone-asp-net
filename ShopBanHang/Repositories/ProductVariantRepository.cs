using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Models.Entities;
using ShopBanHang.Repositories.Interfaces;

namespace ShopBanHang.Repositories
{
    public class ProductVariantRepository : IProductVariantRepository
    {
        private readonly ShopContext _db;

        public ProductVariantRepository(ShopContext db) => _db = db;

        public async Task<ProductVariant?> LayKemSanPhamAsync(int id)
            => await _db.ProductVariants
                .Include(v => v.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == id);

        public async Task<List<ProductVariant>> LayNhieuKemSanPhamAsync(IEnumerable<int> ids)
        {
            var ds = ids.Distinct().ToList();
            if (ds.Count == 0) return new List<ProductVariant>();

            return await _db.ProductVariants
                .Include(v => v.Product)
                .AsNoTracking()
                .Where(v => ds.Contains(v.Id))
                .ToListAsync();
        }

        public async Task<bool> TruTonKhoAsync(int variantId, int soLuong)
        {
            if (soLuong <= 0) return false;

            // UPDATE ProductVariants SET Stock = Stock - @n WHERE Id = @id AND IsActive = 1 AND Stock >= @n
            // Đọc tồn kho rồi mới trừ bằng C# thì hai người bấm đặt cùng lúc đều thấy "còn 1"
            // và cùng trừ, tồn kho thành -1. Gộp kiểm tra và trừ vào một câu lệnh thì không bị.
            int dong = await _db.ProductVariants
                .Where(v => v.Id == variantId && v.IsActive && v.Stock >= soLuong)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.Stock, v => v.Stock - soLuong));

            return dong > 0;
        }

        public async Task HoanTonKhoAsync(int variantId, int soLuong)
        {
            if (soLuong <= 0) return;

            await _db.ProductVariants
                .Where(v => v.Id == variantId)
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.Stock, v => v.Stock + soLuong));
        }

        public async Task CongDaBanAsync(int productId, int soLuong)
        {
            if (soLuong <= 0) return;

            await _db.Products
                .Where(p => p.Id == productId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.SoldCount, p => p.SoldCount + soLuong));
        }
    }
}
