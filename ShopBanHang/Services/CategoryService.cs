using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ShopContext _db;

        public CategoryService(ShopContext db) => _db = db;

        public async Task<List<Category>> LayTatCaAsync()
            => await _db.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Id)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Category?> LayTheoIdAsync(int id)
            => await _db.Categories.FindAsync(id);

        public async Task<(bool ok, string? loi)> LuuAsync(Category c)
        {
            string slug = SlugHelper.Tao(c.Name);
            if (string.IsNullOrEmpty(slug))
                return (false, "Tên danh mục không hợp lệ.");

            if (await _db.Categories.AnyAsync(x => x.Slug == slug && x.Id != c.Id))
                return (false, "Đã có danh mục khác cùng tên.");

            if (c.Id == 0)
            {
                c.Slug = slug;
                c.Name = c.Name.Trim();
                _db.Categories.Add(c);
            }
            else
            {
                var cu = await _db.Categories.FindAsync(c.Id);
                if (cu == null) return (false, "Không tìm thấy danh mục cần sửa.");

                cu.Name = c.Name.Trim();
                cu.Slug = slug;
                cu.DisplayOrder = c.DisplayOrder;
                cu.IsActive = c.IsActive;
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c == null) return (false, "Không tìm thấy danh mục.");

            c.IsActive = !c.IsActive;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> XoaAsync(int id)
        {
            var c = await _db.Categories.FindAsync(id);
            if (c == null) return (false, "Không tìm thấy danh mục.");

            // Khoá ngoại Products -> Categories đặt Restrict, xoá thẳng sẽ ném lỗi SQL.
            // Kiểm trước ở đây để trả về câu tiếng Việt thay vì màn hình lỗi.
            int soSanPham = await _db.Products.CountAsync(p => p.CategoryId == id);
            if (soSanPham > 0)
                return (false, $"Danh mục đang có {soSanPham} sản phẩm, hãy chuyển chúng sang danh mục khác trước. "
                             + "Muốn tạm ẩn thì bấm nút bật tắt.");

            _db.Categories.Remove(c);
            await _db.SaveChangesAsync();
            return (true, null);
        }
    }

    public class BrandService : IBrandService
    {
        private readonly ShopContext _db;

        public BrandService(ShopContext db) => _db = db;

        public async Task<List<Brand>> LayTatCaAsync()
            => await _db.Brands
                .Include(b => b.Products)
                .OrderBy(b => b.Name)
                .AsNoTracking()
                .ToListAsync();

        public async Task<Brand?> LayTheoIdAsync(int id)
            => await _db.Brands.FindAsync(id);

        public async Task<(bool ok, string? loi)> LuuAsync(Brand b)
        {
            string slug = SlugHelper.Tao(b.Name);
            if (string.IsNullOrEmpty(slug))
                return (false, "Tên thương hiệu không hợp lệ.");

            if (await _db.Brands.AnyAsync(x => x.Slug == slug && x.Id != b.Id))
                return (false, "Đã có thương hiệu khác cùng tên.");

            if (b.Id == 0)
            {
                b.Slug = slug;
                b.Name = b.Name.Trim();
                _db.Brands.Add(b);
            }
            else
            {
                var cu = await _db.Brands.FindAsync(b.Id);
                if (cu == null) return (false, "Không tìm thấy thương hiệu cần sửa.");

                cu.Name = b.Name.Trim();
                cu.Slug = slug;
                cu.IsActive = b.IsActive;

                // form luôn điền sẵn logo hiện tại, nên ô trống nghĩa là admin muốn bỏ logo
                cu.LogoUrl = string.IsNullOrWhiteSpace(b.LogoUrl) ? null : b.LogoUrl.Trim();
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id)
        {
            var b = await _db.Brands.FindAsync(id);
            if (b == null) return (false, "Không tìm thấy thương hiệu.");

            b.IsActive = !b.IsActive;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> XoaAsync(int id)
        {
            var b = await _db.Brands.FindAsync(id);
            if (b == null) return (false, "Không tìm thấy thương hiệu.");

            int soSanPham = await _db.Products.CountAsync(p => p.BrandId == id);
            if (soSanPham > 0)
                return (false, $"Thương hiệu đang có {soSanPham} sản phẩm, không xoá được. "
                             + "Muốn tạm ẩn thì bấm nút bật tắt.");

            _db.Brands.Remove(b);
            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}
