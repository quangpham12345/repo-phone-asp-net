using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShopBanHang.Helpers;

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

        [StringLength(50)]
        [Display(Name = "Hệ điều hành")]
        public string? Os { get; set; }

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

        // ===== Các thuộc tính tính từ biến thể. Chỉ đúng khi query đã Include(p => p.Variants). =====

        /// <summary>Sản phẩm có ít nhất một biến thể (kể cả đã tắt). Chưa có thì chưa có giá, chưa bán được.</summary>
        [NotMapped]
        public bool HasVariants => Variants.Count > 0;

        /// <summary>Giá bán hiển thị: giá thấp nhất trong các biến thể đang bán. Chưa có biến thể thì là 0.</summary>
        [NotMapped]
        public decimal DisplayPrice => TongHopBienThe.Tinh(Variants)?.Gia ?? 0;

        /// <summary>Giá gốc của biến thể đang được lấy làm giá hiển thị. Null là không giảm giá.</summary>
        [NotMapped]
        public decimal? DisplayOldPrice => TongHopBienThe.Tinh(Variants)?.GiaGoc;

        /// <summary>Tổng tồn kho của các biến thể đang bán.</summary>
        [NotMapped]
        public int TotalStock => TongHopBienThe.Tinh(Variants)?.TonKho ?? 0;

        /// <summary>Các biến thể đang bán có giá khác nhau, để hiện chữ "Từ" trước giá.</summary>
        [NotMapped]
        public bool HasPriceRange => Variants.Where(v => v.IsActive).Select(v => v.Price).Distinct().Count() > 1;

        /// <summary>Các dung lượng đang bán, xếp từ nhỏ đến lớn.</summary>
        [NotMapped]
        public List<string> StorageOptions => Variants
            .Where(v => v.IsActive)
            .Select(v => v.Storage)
            .Distinct()
            .OrderBy(TongHopBienThe.GiaTriGB).ThenBy(s => s)
            .ToList();

        /// <summary>Các màu đang bán.</summary>
        [NotMapped]
        public List<string> ColorOptions => Variants
            .Where(v => v.IsActive)
            .Select(v => v.Color)
            .Distinct()
            .OrderBy(c => c)
            .ToList();

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
