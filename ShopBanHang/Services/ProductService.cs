using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class ProductService : IProductService
    {
        private readonly ShopContext _db;

        public ProductService(ShopContext db) => _db = db;

        public async Task<List<Product>> LayNoiBatAsync(int soLuong)
            => await KemDanhMuc()
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.SoldCount)
                .Take(soLuong)
                .ToListAsync();

        public async Task<List<Product>> LayMoiNhatAsync(int soLuong)
            => await KemDanhMuc()
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedAt)
                .Take(soLuong)
                .ToListAsync();

        public async Task<PagedResult<Product>> LayDanhSachAsync(ProductFilter loc, int page, int pageSize)
        {
            // Dồn mọi điều kiện vào một câu SQL, chỉ trang hiện tại mới kéo về bộ nhớ
            IQueryable<Product> q = KemDanhMuc();

            if (loc.ChiHangDangBan) q = q.Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(loc.TuKhoa))
            {
                string tk = loc.TuKhoa.Trim();
                q = q.Where(p => p.Name.Contains(tk));
            }
            if (!string.IsNullOrWhiteSpace(loc.DanhMuc))
                q = q.Where(p => p.Category!.Slug == loc.DanhMuc);

            if (!string.IsNullOrWhiteSpace(loc.ThuongHieu))
                q = q.Where(p => p.Brand!.Slug == loc.ThuongHieu);

            // Giá của sản phẩm là giá thấp nhất trong các biến thể đang bán (cùng luật với Product.DisplayPrice).
            // Sản phẩm chưa có biến thể đang bán thì MIN là NULL nên tự rơi khỏi mọi khoảng giá.
            if (loc.GiaTu.HasValue)
            {
                decimal tu = loc.GiaTu.Value;
                q = q.Where(p => p.Variants.Where(v => v.IsActive).Min(v => (decimal?)v.Price) >= tu);
            }
            if (loc.GiaDen.HasValue)
            {
                decimal den = loc.GiaDen.Value;
                q = q.Where(p => p.Variants.Where(v => v.IsActive).Min(v => (decimal?)v.Price) <= den);
            }

            if (!string.IsNullOrWhiteSpace(loc.Ram))
                q = q.Where(p => p.Ram == loc.Ram);
            if (!string.IsNullOrWhiteSpace(loc.Storage))
            {
                string dungLuong = loc.Storage;
                q = q.Where(p => p.Variants.Any(v => v.IsActive && v.Storage == dungLuong));
            }
            if (!string.IsNullOrWhiteSpace(loc.Os))
                q = q.Where(p => p.Os == loc.Os);
            q = loc.SapXep switch
            {
                // Sản phẩm chưa có biến thể đang bán (chưa có giá) luôn xuống cuối, dù sắp tăng hay giảm
                ProductSort.GiaTang => q.OrderBy(p => p.Variants.Any(v => v.IsActive) ? 0 : 1)
                                        .ThenBy(p => p.Variants.Where(v => v.IsActive).Min(v => (decimal?)v.Price)),
                ProductSort.GiaGiam => q.OrderBy(p => p.Variants.Any(v => v.IsActive) ? 0 : 1)
                                        .ThenByDescending(p => p.Variants.Where(v => v.IsActive).Min(v => (decimal?)v.Price)),
                ProductSort.BanChay => q.OrderByDescending(p => p.SoldCount),
                _ => q.OrderByDescending(p => p.CreatedAt)
            };

            return await PagedResult<Product>.TaoAsync(q, page, pageSize);
        }

        public async Task<Product?> LayTheoSlugAsync(string slug)
            => await KemDanhMuc().FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

        public async Task<Product?> LayTheoIdAsync(int id)
            => await KemDanhMuc().FirstOrDefaultAsync(p => p.Id == id);

        public async Task<List<Product>> LayLienQuanAsync(Product sp, int soLuong)
            => await KemDanhMuc()
                .Where(p => p.IsActive && p.CategoryId == sp.CategoryId && p.Id != sp.Id)
                .OrderByDescending(p => p.SoldCount)
                .Take(soLuong)
                .ToListAsync();

        public async Task<List<Category>> LayDanhMucAsync()
            => await _db.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .AsNoTracking()
                .ToListAsync();

        public async Task<List<Brand>> LayThuongHieuAsync()
            => await _db.Brands
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .AsNoTracking()
                .ToListAsync();
        public async Task<List<Brand>> LayThuongHieuAsync(int soLuong)
           => await _db.Brands
               .Where(b => b.IsActive)
               .OrderBy(b => b.Name)
               .Take(soLuong)
               .AsNoTracking()
               .ToListAsync();
        public async Task<List<Product>> LayBanChayAsync(int soLuong)
        => await KemDanhMuc()
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.SoldCount)
            .Take(soLuong)
            .ToListAsync();

        public async Task<int> DemAsync() => await _db.Products.CountAsync();

        // ===== Phần quản trị =====

        public async Task<(bool ok, string? loi)> LuuAsync(ProductFormViewModel vm)
        {
            if (!await _db.Categories.AnyAsync(c => c.Id == vm.CategoryId))
                return (false, "Danh mục không hợp lệ.");
            if (!await _db.Brands.AnyAsync(b => b.Id == vm.BrandId))
                return (false, "Thương hiệu không hợp lệ.");

            string slug = SlugHelper.Tao(vm.Name);
            if (await _db.Products.AnyAsync(p => p.Slug == slug && p.Id != vm.Id))
                return (false, "Đã có sản phẩm khác cùng tên, hãy đổi tên cho khác đi.");

            // Gộp ảnh: ảnh hiện có - ảnh bị xoá + ảnh mới, rồi chọn ảnh đại diện và ghép gallery
            var anh = ProductGallery.Gop(vm.ImageUrl, vm.GalleryUrls, vm.AnhXoa, vm.AnhChinh, vm.AnhTaiLenMoi, vm.AnhUrlMoi);
            if (anh.Loi != null) return (false, anh.Loi);

            if (vm.Id == 0)
            {
                var moi = new Product
                {
                    Name = vm.Name.Trim(),
                    Slug = slug,
                    ImageUrl = anh.AnhChinh,
                    Description = vm.Description?.Trim(),
                    CategoryId = vm.CategoryId,
                    BrandId = vm.BrandId,
                    GalleryUrls = anh.Gallery,
                    Ram = vm.Ram?.Trim(),
                    Os = vm.Os?.Trim(),
                    Screen = vm.Screen?.Trim(),
                    Chip = vm.Chip?.Trim(),
                    Battery = vm.Battery?.Trim(),
                    Camera = vm.Camera?.Trim(),
                    IsFeatured = vm.IsFeatured,
                    IsActive = vm.IsActive,
                    CreatedAt = DateTime.Now
                };
                _db.Products.Add(moi);

                await _db.SaveChangesAsync();

                // Trả id mới về cho controller để chuyển thẳng sang trang thêm biến thể
                vm.Id = moi.Id;
                return (true, null);
            }
            else
            {
                var p = await _db.Products.FindAsync(vm.Id);
                if (p == null) return (false, "Không tìm thấy sản phẩm cần sửa.");

                p.Name = vm.Name.Trim();
                p.Slug = slug;
                p.Description = vm.Description?.Trim();
                p.CategoryId = vm.CategoryId;
                p.BrandId = vm.BrandId;
                p.GalleryUrls = anh.Gallery;
                p.Ram = vm.Ram?.Trim();
                p.Os = vm.Os?.Trim();
                p.Screen = vm.Screen?.Trim();
                p.Chip = vm.Chip?.Trim();
                p.Battery = vm.Battery?.Trim();
                p.Camera = vm.Camera?.Trim();
                p.IsFeatured = vm.IsFeatured;
                p.IsActive = vm.IsActive;

                // Form luôn gửi lại ảnh hiện tại qua ô ẩn, nên ảnh chính do bộ gộp ảnh quyết định
                p.ImageUrl = anh.AnhChinh;

                // SoldCount do hệ thống cộng dồn khi đơn hoàn thành, form không được sửa
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return (false, "Không tìm thấy sản phẩm.");

            p.IsActive = !p.IsActive;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<List<CanhBaoTonKhoItem>> LayCanhBaoTonKhoAsync()
        {
            int nguong = NguongTonKho.Nguong;

            // Biến thể đang bán của sản phẩm đang bán
            var bienThe = await _db.ProductVariants.AsNoTracking()
                .Where(v => v.IsActive && v.Stock <= nguong && v.Product!.IsActive)
                .Select(v => new CanhBaoTonKhoItem
                {
                    SanPhamId = v.ProductId,
                    TenSanPham = v.Product!.Name,
                    Anh = v.ImageUrl ?? v.Product.ImageUrl,
                    BienThe = v.Storage + " · " + v.Color,
                    Ton = v.Stock
                })
                .ToListAsync();

            return bienThe
                .OrderBy(x => x.Ton)
                .ThenBy(x => x.TenSanPham)
                .ToList();
        }

        public async Task<int> DemCanhBaoTonKhoAsync()
        {
            int nguong = NguongTonKho.Nguong;
            return await _db.ProductVariants
                .CountAsync(v => v.IsActive && v.Stock <= nguong && v.Product!.IsActive);
        }

        public async Task<(bool ok, string? loi)> DoiNoiBatAsync(int id)
        {
            var p = await _db.Products.FindAsync(id);
            if (p == null) return (false, "Không tìm thấy sản phẩm.");

            p.IsFeatured = !p.IsFeatured;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        /// <summary>
        /// Nạp kèm Category, Brand và Variants vì view nào cũng hiện tên danh mục, thương hiệu và giá
        /// (giá lấy từ biến thể). Không Include thì mỗi dòng lại thêm một truy vấn, và giá sẽ ra 0.
        /// </summary>
        private IQueryable<Product> KemDanhMuc()
            => _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                .AsNoTracking();

        public async Task<List<ProductSuggestItem>> GoiYAsync(string tuKhoa, int soLuong)
        {
            if (string.IsNullOrWhiteSpace(tuKhoa) || tuKhoa.Trim().Length < 2)
                return new List<ProductSuggestItem>();
            string tk = tuKhoa.Trim();
            return await _db.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Name.Contains(tk))
                .OrderByDescending(p => p.SoldCount).Take(soLuong)
                .Select(p => new ProductSuggestItem
                {
                    Name = p.Name,
                    Slug = p.Slug,
                    ImageUrl = p.ImageUrl,
                    Price = p.Variants.Where(v => v.IsActive).Min(v => (decimal?)v.Price) ?? 0
                }).ToListAsync();
        }

        public async Task<(List<string> ram, List<string> storage, List<string> os)> LayGiaTriLocAsync()
        {
            var active = _db.Products.AsNoTracking().Where(p => p.IsActive);
            var ram = await active.Where(p => p.Ram != null && p.Ram != "")
                .Select(p => p.Ram!).Distinct().OrderBy(x => x).ToListAsync();
            // Dung lượng lấy từ biến thể đang bán của sản phẩm đang bán, xếp 128GB, 256GB, 512GB, 1TB
            var storage = await _db.ProductVariants.AsNoTracking()
                .Where(v => v.IsActive && v.Product!.IsActive)
                .Select(v => v.Storage).Distinct().ToListAsync();
            storage = storage.OrderBy(TongHopBienThe.GiaTriGB).ThenBy(x => x).ToList();
            var os = await active.Where(p => p.Os != null && p.Os != "")
                .Select(p => p.Os!).Distinct().OrderBy(x => x).ToListAsync();
            return (ram, storage, os);
        }

        public async Task<List<Product>> LayTheoIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.Distinct().Take(3).ToList();
            if (idList.Count == 0) return new List<Product>();
            return await KemDanhMuc().Where(p => p.IsActive && idList.Contains(p.Id)).ToListAsync();
        }
    }
}
