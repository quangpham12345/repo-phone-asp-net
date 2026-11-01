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

            if (loc.GiaTu.HasValue) q = q.Where(p => p.Price >= loc.GiaTu.Value);
            if (loc.GiaDen.HasValue) q = q.Where(p => p.Price <= loc.GiaDen.Value);

            if (!string.IsNullOrWhiteSpace(loc.Ram))
                q = q.Where(p => p.Ram == loc.Ram);
            if (!string.IsNullOrWhiteSpace(loc.Storage))
                q = q.Where(p => p.Storage == loc.Storage);
            if (!string.IsNullOrWhiteSpace(loc.Os))
                q = q.Where(p => p.Os == loc.Os);
            q = loc.SapXep switch
            {
                ProductSort.GiaTang => q.OrderBy(p => p.Price),
                ProductSort.GiaGiam => q.OrderByDescending(p => p.Price),
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
            if (vm.OldPrice.HasValue && vm.OldPrice.Value > 0 && vm.OldPrice.Value <= vm.Price)
                return (false, "Giá gốc phải lớn hơn giá bán, hoặc để trống nếu không giảm giá.");

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
                _db.Products.Add(new Product
                {
                    Name = vm.Name.Trim(),
                    Slug = slug,
                    Price = vm.Price,
                    OldPrice = vm.OldPrice,
                    Stock = vm.Stock,
                    ImageUrl = anh.AnhChinh,
                    Description = vm.Description?.Trim(),
                    CategoryId = vm.CategoryId,
                    BrandId = vm.BrandId,
                    GalleryUrls = anh.Gallery,
                    Ram = vm.Ram?.Trim(),
                    Storage = vm.Storage?.Trim(),
                    Os = vm.Os?.Trim(),
                    Color = vm.Color?.Trim(),
                    Screen = vm.Screen?.Trim(),
                    Chip = vm.Chip?.Trim(),
                    Battery = vm.Battery?.Trim(),
                    Camera = vm.Camera?.Trim(),
                    IsFeatured = vm.IsFeatured,
                    IsActive = vm.IsActive,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                var p = await _db.Products.FindAsync(vm.Id);
                if (p == null) return (false, "Không tìm thấy sản phẩm cần sửa.");

                p.Name = vm.Name.Trim();
                p.Slug = slug;
                p.Price = vm.Price;
                p.OldPrice = vm.OldPrice;
                p.Stock = vm.Stock;
                p.Description = vm.Description?.Trim();
                p.CategoryId = vm.CategoryId;
                p.BrandId = vm.BrandId;
                p.GalleryUrls = anh.Gallery;
                p.Ram = vm.Ram?.Trim();
                p.Storage = vm.Storage?.Trim();
                p.Os = vm.Os?.Trim();
                p.Color = vm.Color?.Trim();
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

            // Sản phẩm không có biến thể thì giá và tồn kho nhập tay trên Product
            var khongBienThe = await _db.Products.AsNoTracking()
                .Where(p => p.IsActive && p.Stock <= nguong && !p.Variants.Any())
                .Select(p => new CanhBaoTonKhoItem
                {
                    SanPhamId = p.Id,
                    TenSanPham = p.Name,
                    Anh = p.ImageUrl,
                    BienThe = null,
                    Ton = p.Stock
                })
                .ToListAsync();

            return bienThe.Concat(khongBienThe)
                .OrderBy(x => x.Ton)
                .ThenBy(x => x.TenSanPham)
                .ToList();
        }

        public async Task<int> DemCanhBaoTonKhoAsync()
        {
            int nguong = NguongTonKho.Nguong;
            int a = await _db.ProductVariants
                .CountAsync(v => v.IsActive && v.Stock <= nguong && v.Product!.IsActive);
            int b = await _db.Products
                .CountAsync(p => p.IsActive && p.Stock <= nguong && !p.Variants.Any());
            return a + b;
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
        /// Nạp kèm Category và Brand vì view nào cũng hiện tên danh mục và thương hiệu.
        /// Không Include thì mỗi dòng lại thêm một truy vấn.
        /// </summary>
        private IQueryable<Product> KemDanhMuc()
            => _db.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
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
                    Price = p.Price
                }).ToListAsync();
        }

        public async Task<(List<string> ram, List<string> storage, List<string> os)> LayGiaTriLocAsync()
        {
            var active = _db.Products.AsNoTracking().Where(p => p.IsActive);
            var ram = await active.Where(p => p.Ram != null && p.Ram != "")
                .Select(p => p.Ram!).Distinct().OrderBy(x => x).ToListAsync();
            var storage = await active.Where(p => p.Storage != null && p.Storage != "")
                .Select(p => p.Storage!).Distinct().OrderBy(x => x).ToListAsync();
            var os = await active.Where(p => p.Os != null && p.Os != "")
                .Select(p => p.Os!).Distinct().OrderBy(x => x).ToListAsync();
            return (ram, storage, os);
        }

        public async Task<List<Product>> LayPhienBanKhacAsync(Product sp, int soLuong)
        {
            // Cùng hãng + danh mục, rồi so tên sau khi bỏ dung lượng ở cuối
            var candidates = await KemDanhMuc()
                .Where(p => p.IsActive
                            && p.BrandId == sp.BrandId
                            && p.CategoryId == sp.CategoryId
                            && p.Id != sp.Id)
                .ToListAsync();

            static string TenDongMay(string name)
            {
                return System.Text.RegularExpressions.Regex
                    .Replace(name ?? "", @"\s+\d+\s*(GB|TB)\s*$", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                    .Trim();
            }

            string dong = TenDongMay(sp.Name);

            return candidates
                .Where(p => string.Equals(TenDongMay(p.Name), dong, StringComparison.OrdinalIgnoreCase))
                .OrderBy(p => p.Storage)
                .ThenBy(p => p.Color)
                .Take(soLuong)
                .ToList();
        }

        public async Task<List<Product>> LayTheoIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids.Distinct().Take(3).ToList();
            if (idList.Count == 0) return new List<Product>();
            return await KemDanhMuc().Where(p => p.IsActive && idList.Contains(p.Id)).ToListAsync();
        }
    }
}
