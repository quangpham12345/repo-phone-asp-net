using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopBanHang.Models.Entities
{
    /// <summary>Dòng hàng trong đơn. Bảng nối Order với Product, có thêm số lượng và đơn giá.</summary>
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        /// <summary>
        /// Biến thể khách đã mua. Null ở các đơn cũ tạo trước khi có giỏ hàng theo biến thể.
        /// Dòng có biến thể nghĩa là lúc đặt đã trừ tồn kho của biến thể đó,
        /// nên huỷ đơn thì hoàn lại đúng chỗ này.
        /// </summary>
        public int? ProductVariantId { get; set; }
        public virtual ProductVariant? ProductVariant { get; set; }

        /// <summary>Chép tên sản phẩm lúc mua, để hoá đơn không đổi khi admin sửa tên sản phẩm.</summary>
        [StringLength(200)]
        [Display(Name = "Sản phẩm")]
        public string? ProductName { get; set; }

        /// <summary>Chép cấu hình lúc mua, ví dụ "256GB · Titan Đen".</summary>
        [StringLength(80)]
        [Display(Name = "Phân loại")]
        public string? VariantName { get; set; }

        [Range(1, 999, ErrorMessage = "Số lượng phải từ 1 trở lên")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; }

        /// <summary>
        /// Đơn giá tại thời điểm mua, KHÔNG join sang Products lấy giá hiện tại.
        /// Shop đổi giá thì hoá đơn cũ phải giữ nguyên giá lúc khách đặt.
        /// </summary>
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Đơn giá")]
        public decimal UnitPrice { get; set; }

        /// <summary>Thành tiền của dòng, tính sẵn nên không map xuống cột trong CSDL.</summary>
        [NotMapped]
        public decimal LineTotal => Quantity * UnitPrice;
    }
}
