using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class ProductVariantService : IProductVariantService
    {
        private readonly ShopContext _db;

        public ProductVariantService(ShopContext db) => _db = db;

        public async Task<List<ProductVariant>> LayTheoSanPhamAsync(int productId)
            => await _db.ProductVariants
                .Where(v => v.ProductId == productId)
                .OrderBy(v => v.Storage)
                .ThenBy(v => v.Color)
                .AsNoTracking()
                .ToListAsync();

        public async Task<ProductVariant?> LayTheoIdAsync(int id)
            => await _db.ProductVariants.FindAsync(id);

        public async Task<int> DemAsync(int productId)
            => await _db.ProductVariants.CountAsync(v => v.ProductId == productId);

        public async Task<(bool ok, string? loi)> LuuAsync(ProductVariant bienThe)
        {
            string dungLuong = (bienThe.Storage ?? string.Empty).Trim();
            string mauSac = (bienThe.Color ?? string.Empty).Trim();
            string? anh = string.IsNullOrWhiteSpace(bienThe.ImageUrl) ? null : bienThe.ImageUrl.Trim();

            if (dungLuong.Length == 0) return (false, "Vui lòng nhập dung lượng.");
            if (mauSac.Length == 0) return (false, "Vui lòng nhập màu sắc.");
            if (bienThe.Price <= 0) return (false, "Giá bán phải lớn hơn 0.");
            if (bienThe.Stock < 0) return (false, "Tồn kho không được âm.");

            // Cùng luật với giá gốc của sản phẩm: có giá gốc thì phải lớn hơn giá bán
            decimal? giaGoc = bienThe.OldPrice is > 0 ? bienThe.OldPrice : null;
            if (giaGoc.HasValue && giaGoc.Value <= bienThe.Price)
                return (false, "Giá gốc phải lớn hơn giá bán, hoặc để trống nếu không giảm giá.");

            // Sửa thì lấy bản ghi đang có trước, vì ProductId lấy từ DB chứ không tin form
            ProductVariant? hienCo = null;
            int productId = bienThe.ProductId;

            if (bienThe.Id != 0)
            {
                hienCo = await _db.ProductVariants.FindAsync(bienThe.Id);
                if (hienCo == null) return (false, "Không tìm thấy biến thể cần sửa.");
                productId = hienCo.ProductId;
            }

            if (!await _db.Products.AnyAsync(p => p.Id == productId))
                return (false, "Không tìm thấy sản phẩm.");

            bool trung = await _db.ProductVariants.AnyAsync(v =>
                v.ProductId == productId && v.Storage == dungLuong && v.Color == mauSac && v.Id != bienThe.Id);
            if (trung)
                return (false, $"Sản phẩm đã có biến thể {dungLuong} - {mauSac}.");

            if (hienCo == null)
            {
                _db.ProductVariants.Add(new ProductVariant
                {
                    ProductId = productId,
                    Storage = dungLuong,
                    Color = mauSac,
                    Price = bienThe.Price,
                    OldPrice = giaGoc,
                    Stock = bienThe.Stock,
                    ImageUrl = anh,
                    IsActive = bienThe.IsActive
                });
            }
            else
            {
                hienCo.Storage = dungLuong;
                hienCo.Color = mauSac;
                hienCo.Price = bienThe.Price;
                hienCo.OldPrice = giaGoc;
                hienCo.Stock = bienThe.Stock;
                hienCo.ImageUrl = anh;
                hienCo.IsActive = bienThe.IsActive;
            }

            return await LuuVaDongBoAsync(productId);
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id)
        {
            var v = await _db.ProductVariants.FindAsync(id);
            if (v == null) return (false, "Không tìm thấy biến thể.");

            v.IsActive = !v.IsActive;
            return await LuuVaDongBoAsync(v.ProductId);
        }

        public async Task<(bool ok, string? loi)> XoaAsync(int id)
        {
            var v = await _db.ProductVariants.FindAsync(id);
            if (v == null) return (false, "Không tìm thấy biến thể.");

            // Khi OrderDetails có VariantId (phần của đơn hàng), chặn xoá biến thể đã nằm trong đơn
            // và báo "hãy tắt biến thể thay vì xoá", giống cách sản phẩm và tài khoản đang làm.

            int productId = v.ProductId;
            _db.ProductVariants.Remove(v);
            return await LuuVaDongBoAsync(productId);
        }

        public async Task DongBoSanPhamAsync(int productId)
        {
            var bienThe = await _db.ProductVariants
                .AsNoTracking()
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            var tong = TongHopBienThe.Tinh(bienThe);
            if (tong == null) return;   // chưa có biến thể: giữ giá và tồn kho nhập tay

            var sp = await _db.Products.FindAsync(productId);
            if (sp == null) return;

            sp.Price = tong.Gia;
            sp.OldPrice = tong.GiaGoc;
            sp.Stock = tong.TonKho;
            await _db.SaveChangesAsync();
        }

        public async Task GoAnhAsync(int productId, IEnumerable<string> cacAnh)
        {
            var ds = cacAnh
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList();
            if (ds.Count == 0) return;

            var bienThe = await _db.ProductVariants
                .Where(v => v.ProductId == productId && v.ImageUrl != null && ds.Contains(v.ImageUrl))
                .ToListAsync();
            if (bienThe.Count == 0) return;

            foreach (var v in bienThe) v.ImageUrl = null;
            await _db.SaveChangesAsync();
        }

        /// <summary>Lưu thay đổi biến thể và đồng bộ giá, tồn kho của sản phẩm trong cùng một transaction.</summary>
        private async Task<(bool ok, string? loi)> LuuVaDongBoAsync(int productId)
        {
            await using var tx = await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            await DongBoSanPhamAsync(productId);
            await tx.CommitAsync();
            return (true, null);
        }
    }
}