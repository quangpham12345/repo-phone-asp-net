using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class NewsService : INewsService
    {
        private readonly ShopContext _db;

        public NewsService(ShopContext db) => _db = db;

        public async Task<PagedResult<News>> LayDanhSachAsync(string? tuKhoa, bool? active,
                                                              int page, int pageSize)
        {
            IQueryable<News> q = _db.News.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim();
                q = q.Where(n => n.Title.Contains(tk));
            }
            if (active.HasValue) q = q.Where(n => n.IsActive == active.Value);

            q = q.OrderByDescending(n => n.CreatedAt);

            return await PagedResult<News>.TaoAsync(q, page, pageSize);
        }

        public async Task<List<News>> LayMoiNhatAsync(int soLuong)
            => await _db.News
                .Where(n => n.IsActive)
                .OrderByDescending(n => n.CreatedAt)
                .Take(soLuong)
                .AsNoTracking()
                .ToListAsync();

        public async Task<News?> LayTheoIdAsync(int id)
            => await _db.News.FindAsync(id);

        public async Task<News?> LayTheoSlugAsync(string slug)
            => await _db.News.FirstOrDefaultAsync(n => n.Slug == slug && n.IsActive);

        public async Task<(bool ok, string? loi)> LuuAsync(News n)
        {
            string slug = SlugHelper.Tao(n.Title);
            if (string.IsNullOrEmpty(slug))
                return (false, "Tiêu đề không hợp lệ.");

            if (await _db.News.AnyAsync(x => x.Slug == slug && x.Id != n.Id))
                return (false, "Đã có bài viết khác cùng tiêu đề.");

            if (n.Id == 0)
            {
                n.Slug = slug;
                n.Title = n.Title.Trim();
                n.CreatedAt = DateTime.Now;
                _db.News.Add(n);
            }
            else
            {
                var cu = await _db.News.FindAsync(n.Id);
                if (cu == null) return (false, "Không tìm thấy bài viết cần sửa.");

                cu.Title = n.Title.Trim();
                cu.Slug = slug;
                cu.Summary = n.Summary?.Trim();
                cu.Content = n.Content?.Trim();
                cu.IsActive = n.IsActive;

                if (!string.IsNullOrWhiteSpace(n.ImageUrl)) cu.ImageUrl = n.ImageUrl.Trim();
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id)
        {
            var n = await _db.News.FindAsync(id);
            if (n == null) return (false, "Không tìm thấy bài viết.");

            n.IsActive = !n.IsActive;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> XoaAsync(int id)
        {
            var n = await _db.News.FindAsync(id);
            if (n == null) return (false, "Không tìm thấy bài viết.");

            // Tin tức không bị bảng nào tham chiếu nên xoá hẳn được
            _db.News.Remove(n);
            await _db.SaveChangesAsync();
            return (true, null);
        }
    }
}
