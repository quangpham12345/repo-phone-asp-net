using System.ComponentModel.DataAnnotations;
using ShopBanHang.Models.Entities;

namespace ShopBanHang.Models.ViewModels
{
    /// <summary>
    /// Form thêm và sửa sản phẩm.
    ///
    /// Không bind thẳng entity Product vào form: entity có navigation property
    /// và các cột như SoldCount không được để người dùng sửa qua form.
    /// </summary>
    public class ProductFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm bắt buộc nhập")]
        [StringLength(200, MinimumLength = 2)]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [Range(0, 999999999, ErrorMessage = "Giá bán phải từ 0 trở lên")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        [Range(0, 999999999, ErrorMessage = "Giá gốc phải từ 0 trở lên")]
        [Display(Name = "Giá gốc")]
        public decimal? OldPrice { get; set; }

        [Range(0, 99999, ErrorMessage = "Tồn kho không được âm")]
        [Display(Name = "Tồn kho")]
        public int Stock { get; set; }

        [StringLength(250)]
        [Display(Name = "Ảnh chính")]
        public string? ImageUrl { get; set; }

        [StringLength(1000)]
        [Display(Name = "Ảnh gallery (cách nhau bằng ;)")]
        public string? GalleryUrls { get; set; }

        [DataType(DataType.MultilineText)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [StringLength(30)]
        [Display(Name = "RAM")]
        public string? Ram { get; set; }

        [StringLength(30)]
        [Display(Name = "Dung lượng")]
        public string? Storage { get; set; }

        [StringLength(50)]
        [Display(Name = "Hệ điều hành")]
        public string? Os { get; set; }

        [StringLength(40)]
        [Display(Name = "Màu sắc")]
        public string? Color { get; set; }

        [StringLength(100)]
        [Display(Name = "Màn hình")]
        public string? Screen { get; set; }

        [StringLength(100)]
        [Display(Name = "Chip")]
        public string? Chip { get; set; }

        [StringLength(50)]
        [Display(Name = "Pin")]
        public string? Battery { get; set; }

        [StringLength(200)]
        [Display(Name = "Camera")]
        public string? Camera { get; set; }

        [Display(Name = "Danh mục")]
        [Range(1, int.MaxValue, ErrorMessage = "Hãy chọn danh mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Thương hiệu")]
        [Range(1, int.MaxValue, ErrorMessage = "Hãy chọn thương hiệu")]
        public int BrandId { get; set; }

        [Display(Name = "Nổi bật")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Đang bán")]
        public bool IsActive { get; set; } = true;

        /// <summary>Chỉ để hiện trên form, không cho sửa — số này do hệ thống cộng dồn.</summary>
        [Display(Name = "Đã bán")]
        public int SoldCount { get; set; }

        /// <summary>Số biến thể của sản phẩm. Có biến thể thì giá và tồn kho bị khoá trên form.</summary>
        public int SoBienThe { get; set; }

        // ===== Quản lý nhiều ảnh: form gửi lên, service gộp với ImageUrl/GalleryUrls =====

        /// <summary>Các ảnh admin tích chọn xoá.</summary>
        public List<string> AnhXoa { get; set; } = new();

        /// <summary>Ảnh admin chọn làm ảnh đại diện. Rỗng thì giữ ảnh đại diện hiện tại.</summary>
        public string? AnhChinh { get; set; }

        /// <summary>Đường dẫn ảnh gõ tay, nhiều ảnh cách nhau bằng dấu ;.</summary>
        [StringLength(500)]
        [Display(Name = "Thêm ảnh bằng đường dẫn")]
        public string? AnhUrlMoi { get; set; }

        /// <summary>Đường dẫn các ảnh vừa tải lên trong lần lưu này. Controller điền sau khi lưu file.</summary>
        public List<string> AnhTaiLenMoi { get; set; } = new();

        public List<Category> CacDanhMuc { get; set; } = new();
        public List<Brand> CacThuongHieu { get; set; } = new();

        public bool LaThemMoi => Id == 0;
    }
}
