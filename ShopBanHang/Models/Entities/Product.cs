using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopBanHang.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm bắt buộc nhập")]
        [StringLength(200)]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(220)]
        [Display(Name = "Đường dẫn")]
        public string Slug { get; set; } = string.Empty;

        /// <summary>Giá bán. decimal(18,0) vì VNĐ không có phần xu.</summary>        
        [Range(0, 999999999, ErrorMessage = "Giá phải từ 0 trở lên")]
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Giá bán")]
        public decimal Price { get; set; }

        /// <summary>Giá gốc để hiển thị gạch ngang khi giảm giá. Null là không giảm.</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Giá gốc")]
        public decimal? OldPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho không được âm")]
        [Display(Name = "Tồn kho")]
        public int Stock { get; set; }

        /// <summary>Cộng dồn khi đơn chuyển sang Hoàn thành, dùng cho thống kê bán chạy.</summary>        
        [Display(Name = "Đã bán")]
        public int SoldCount { get; set; }

        [StringLength(250)]
        [Display(Name = "Ảnh chính")]
        public string? ImageUrl { get; set; }

        /// <summary>Các ảnh phụ, ngăn cách bằng dấu ; dùng cho gallery trang chi tiết.</summary>
        [StringLength(1000)]
        [Display(Name = "Ảnh gallery")]
        public string? GalleryUrls { get; set; }

        [Display(Name = "Mô tả")]
        [DataType(DataType.MultilineText)]
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

        [Display(Name = "Nổi bật")]
        public bool IsFeatured { get; set; }

        [Display(Name = "Đang bán")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Display(Name = "Thương hiệu")]
        public int BrandId { get; set; }
        public virtual Brand? Brand { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

        [NotMapped]
        public List<string> TatCaAnh
        {
            get
            {
                var list = new List<string>();
                if (!string.IsNullOrWhiteSpace(ImageUrl)) list.Add(ImageUrl);
                if (!string.IsNullOrWhiteSpace(GalleryUrls))
                {
                    foreach (var u in GalleryUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                        if (!list.Contains(u)) list.Add(u);
                }
                return list;
            }
        }
    }
}
