using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    /// <summary>
    /// Một slide của slideshow trên trang chủ.
    /// Admin thêm, sửa, bật tắt và sắp thứ tự ở khu quản trị.
    /// </summary>
    public class Banner
    {
        public int Id { get; set; }

        /// <summary>Tiêu đề, đồng thời dùng làm chữ mô tả ảnh (alt).</summary>
        [Required(ErrorMessage = "Tiêu đề bắt buộc nhập")]
        [StringLength(150)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        /// <summary>Ảnh tải lên đặt ở /uploads/banners/, hoặc đường dẫn ảnh có sẵn.</summary>
        [Required(ErrorMessage = "Ảnh bắt buộc có")]
        [StringLength(500)]
        [Display(Name = "Ảnh")]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>Bấm vào banner thì đi đâu. Để trống thì banner không bấm được.</summary>
        [StringLength(500)]
        [Display(Name = "Liên kết")]
        public string? LinkUrl { get; set; }

        [Display(Name = "Thứ tự")]
        public int SortOrder { get; set; }

        /// <summary>Tắt thì ẩn khỏi trang chủ nhưng vẫn còn trong danh sách quản trị.</summary>
        [Display(Name = "Đang hiện")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
