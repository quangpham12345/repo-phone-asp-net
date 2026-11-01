using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    /// <summary>Tin tức, bài viết trên trang bán hàng.</summary>
    public class News
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tiêu đề bắt buộc nhập")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(220)]
        [Display(Name = "Đường dẫn")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả ngắn")]
        public string? Summary { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Nội dung")]
        public string? Content { get; set; }

        [StringLength(250)]
        [Display(Name = "Ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Đang hiện")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày đăng")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
