using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    /// <summary>Danh mục sản phẩm: Điện thoại, Tablet, Phụ kiện...</summary>
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục bắt buộc nhập")]
        [StringLength(100)]
        [Display(Name = "Tên danh mục")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Chuỗi không dấu dùng trên URL, sinh từ Name bằng SlugHelper.</summary>
        [Required]
        [StringLength(120)]
        [Display(Name = "Đường dẫn")]
        public string Slug { get; set; } = string.Empty;

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; }

        [Display(Name = "Đang hiện")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
