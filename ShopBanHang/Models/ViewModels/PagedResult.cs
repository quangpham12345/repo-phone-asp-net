using Microsoft.EntityFrameworkCore;

namespace ShopBanHang.Models.ViewModels
{
    /// <summary>
    /// Một trang dữ liệu kèm thông tin phân trang, dùng chung cho mọi danh sách.
    /// </summary>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);
        public bool CoTrangTruoc => Page > 1;
        public bool CoTrangSau => Page < TotalPages;

        /// <summary>Số thứ tự của dòng đầu tiên trên trang, để cột STT đếm tiếp chứ không về 1.</summary>
        public int SttDau => (Page - 1) * PageSize;

        /// <summary>
        /// Phân trang ngay trên IQueryable: đếm và lấy đúng một trang bằng SQL,
        /// không kéo cả bảng về bộ nhớ.
        /// </summary>
        public static async Task<PagedResult<T>> TaoAsync(IQueryable<T> query, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            int tong = await query.CountAsync();

            // Trang vượt quá số trang có thật thì kéo về trang cuối
            int soTrang = (int)Math.Ceiling(tong / (double)pageSize);
            if (soTrang > 0 && page > soTrang) page = soTrang;

            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = tong
            };
        }
    }
}
