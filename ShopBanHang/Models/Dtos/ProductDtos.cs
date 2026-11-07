using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Dtos
{
    /// <summary>Một dòng sản phẩm trong danh sách trả ra API (GET /api/products).</summary>
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }

        /// <summary>Giá thấp nhất trong các biến thể đang bán. null khi chưa có biến thể.</summary>
        public decimal? Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int TotalStock { get; set; }
        public int SoldCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>Chi tiết một sản phẩm (GET /api/products/{id}), kèm thông số và biến thể.</summary>
    public class ProductDetailDto : ProductDto
    {
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public string? Description { get; set; }
        public List<string> Gallery { get; set; } = new();
        public string? Ram { get; set; }
        public string? Os { get; set; }
        public string? Screen { get; set; }
        public string? Chip { get; set; }
        public string? Battery { get; set; }
        public string? Camera { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<VariantDto> Variants { get; set; } = new();
    }

    public class VariantDto
    {
        public int Id { get; set; }
        public string Storage { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public int Stock { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Dữ liệu client gửi lên khi thêm (POST) hoặc sửa (PUT) sản phẩm.
    /// Có annotation nên [ApiController] tự trả 400 kèm danh sách lỗi khi dữ liệu sai,
    /// controller không phải tự kiểm ModelState.
    /// Giá, tồn kho, dung lượng, màu nằm ở biến thể, không nằm ở đây (giống form admin).
    /// </summary>
    public class ProductWriteDto
    {
        [Required(ErrorMessage = "Tên sản phẩm bắt buộc nhập")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Tên sản phẩm từ 2 đến 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "categoryId không hợp lệ")]
        public int CategoryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "brandId không hợp lệ")]
        public int BrandId { get; set; }

        [StringLength(250)]
        public string? ImageUrl { get; set; }

        /// <summary>Ảnh phụ, nhiều ảnh cách nhau bằng dấu ;</summary>
        [StringLength(1000)]
        public string? GalleryUrls { get; set; }

        public string? Description { get; set; }

        [StringLength(30)] public string? Ram { get; set; }
        [StringLength(50)] public string? Os { get; set; }
        [StringLength(100)] public string? Screen { get; set; }
        [StringLength(100)] public string? Chip { get; set; }
        [StringLength(50)] public string? Battery { get; set; }
        [StringLength(200)] public string? Camera { get; set; }

        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>Danh mục hoặc thương hiệu, dạng gọn cho API.</summary>
    public class LookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }
}
