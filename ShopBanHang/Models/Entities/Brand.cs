using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopBanHang.Models.Entities
{
    /// <summary>Thương hiệu: Apple, Samsung, Xiaomi...</summary>
    public class Brand
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên thương hiệu bắt buộc nhập")]
        [StringLength(100)]
        [Display(Name = "Tên thương hiệu")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        [Display(Name = "Đường dẫn")]
        public string Slug { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        [Display(Name = "Logo")]
        public string? LogoUrl { get; set; }

        [Display(Name = "Đang hiện")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
