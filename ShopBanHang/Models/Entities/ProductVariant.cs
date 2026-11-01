using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopBanHang.Models.Entities
{
    /// <summary>
    /// Một cấu hình bán được của sản phẩm: dung lượng × màu, mỗi cấu hình có giá,
    /// tồn kho và (tuỳ chọn) ảnh riêng. Một sản phẩm có thể không có biến thể nào,
    /// khi đó giá và tồn kho vẫn nhập thẳng trên Product như trước.
    /// </summary>
    public class ProductVariant
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập dung lượng")]
        [StringLength(30, ErrorMessage = "Dung lượng tối đa 30 ký tự")]
        [Display(Name = "Dung lượng")]
        public string Storage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập màu sắc")]
        [StringLength(40, ErrorMessage = "Màu sắc tối đa 40 ký tự")]
        [Display(Name = "Màu sắc")]
        public string Color { get; set; } = string.Empty;

        /// <summary>Giá bán của riêng biến thể này. decimal(18,0) vì VNĐ không có phần xu.</summary>
        [Range(1, 999999999, ErrorMessage = "Giá bán phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        /// <summary>Giá gốc để gạch ngang khi giảm giá. Null là không giảm.</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Giá gốc")]
        public decimal? OldPrice { get; set; }

        [Range(0, 99999, ErrorMessage = "Tồn kho không được âm")]
        [Display(Name = "Tồn kho")]
        public int Stock { get; set; }

        /// <summary>Ảnh riêng của biến thể (thường là ảnh theo màu). Null là dùng ảnh chính của sản phẩm.</summary>
        [StringLength(250)]
        [Display(Name = "Ảnh")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Đang bán")]
        public bool IsActive { get; set; } = true;
    }
}