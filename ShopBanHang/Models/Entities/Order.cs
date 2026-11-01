using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShopBanHang.Models.Enums;

namespace ShopBanHang.Models.Entities
{
    public class Order
    {
        public int Id { get; set; }

        /// <summary>Mã đơn khách dùng để tra cứu, ví dụ DH20261024-0007.</summary>
        [Required]
        [StringLength(20)]
        [Display(Name = "Mã đơn")]
        public string OrderCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên người nhận bắt buộc nhập")]
        [StringLength(100)]
        [Display(Name = "Người nhận")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại bắt buộc nhập")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng bắt buộc nhập")]
        [StringLength(250)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Tổng tiền")]
        public decimal Total { get; set; }

        [Display(Name = "Trạng thái")]
        public OrderStatus Status { get; set; } = OrderStatus.ChoXacNhan;

        [Display(Name = "Thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        [Display(Name = "Ngày đặt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Nullable có chủ ý: khách không đăng nhập vẫn đặt được hàng.
        /// Vì cột này trỏ tới Users nên tài khoản chỉ được tắt, không xoá cứng.
        /// </summary>
        [Display(Name = "Tài khoản")]
        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
