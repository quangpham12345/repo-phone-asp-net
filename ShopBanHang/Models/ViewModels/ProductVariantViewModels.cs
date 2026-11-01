using System.ComponentModel.DataAnnotations;
using ShopBanHang.Models.Entities;

namespace ShopBanHang.Models.ViewModels
{
    /// <summary>Trang danh sách biến thể của một sản phẩm.</summary>
    public class ProductVariantListViewModel
    {
        public Product SanPham { get; set; } = null!;
        public List<ProductVariant> CacBienThe { get; set; } = new();
    }

    /// <summary>Form thêm và sửa một biến thể. Không bind thẳng entity.</summary>
    public class ProductVariantFormViewModel
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public bool LaThemMoi => Id == 0;

        [Required(ErrorMessage = "Vui lòng nhập dung lượng")]
        [StringLength(30, ErrorMessage = "Dung lượng tối đa 30 ký tự")]
        [Display(Name = "Dung lượng")]
        public string Storage { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập màu sắc")]
        [StringLength(40, ErrorMessage = "Màu sắc tối đa 40 ký tự")]
        [Display(Name = "Màu sắc")]
        public string Color { get; set; } = string.Empty;

        [Range(1, 999999999, ErrorMessage = "Giá bán phải lớn hơn 0")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Range(0, 999999999, ErrorMessage = "Giá gốc phải từ 0 trở lên")]
        [Display(Name = "Giá gốc")]
        public decimal? OldPrice { get; set; }

        [Range(0, 99999, ErrorMessage = "Tồn kho không được âm")]
        [Display(Name = "Tồn kho")]
        public int Stock { get; set; }

        [StringLength(250)]
        [Display(Name = "Ảnh của biến thể")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Đang bán")]
        public bool IsActive { get; set; } = true;

        // ----- Chỉ để vẽ form, không lấy từ form gửi lên -----

        // Nullable để MVC không coi là ô bắt buộc: giá trị này không nằm trong form gửi lên
        public string? TenSanPham { get; set; }

        /// <summary>Các ảnh của sản phẩm để chọn làm ảnh riêng cho biến thể.</summary>
        public List<string> CacAnh { get; set; } = new();
    }
}