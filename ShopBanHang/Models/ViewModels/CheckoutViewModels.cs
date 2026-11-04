using System.ComponentModel.DataAnnotations;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;

namespace ShopBanHang.Models.ViewModels
{
    /// <summary>
    /// Dữ liệu khách gửi lên ở bước đặt hàng.
    ///
    /// Địa chỉ có hai cách: chọn một địa chỉ trong sổ (AddressId, chỉ khách đã đăng nhập),
    /// hoặc nhập địa chỉ mới (AddressId = null, bắt buộc điền họ tên, số điện thoại, địa chỉ).
    /// Khách vãng lai không có sổ địa chỉ nên luôn đi nhánh nhập mới.
    /// Email luôn bắt buộc để shop gửi xác nhận đơn và để khách lẻ còn cách liên lạc.
    /// </summary>
    public class CheckoutForm : IValidatableObject
    {
        [Display(Name = "Địa chỉ nhận hàng")]
        public int? AddressId { get; set; }

        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ tên người nhận")]
        public string? ReceiverName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [StringLength(250, ErrorMessage = "Địa chỉ tối đa 250 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? AddressDetail { get; set; }

        [Display(Name = "Lưu địa chỉ này vào sổ địa chỉ")]
        public bool LuuDiaChi { get; set; } = true;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [StringLength(500, ErrorMessage = "Ghi chú tối đa 500 ký tự")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Ghi chú cho shop")]
        public string? Note { get; set; }

        [Display(Name = "Hình thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        [StringLength(30)]
        [Display(Name = "Mã giảm giá")]
        public string? CouponCode { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AddressId.HasValue) yield break;

            if (string.IsNullOrWhiteSpace(ReceiverName) || ReceiverName.Trim().Length < 2)
                yield return new ValidationResult("Vui lòng nhập họ tên người nhận (ít nhất 2 ký tự).", new[] { nameof(ReceiverName) });

            if (string.IsNullOrWhiteSpace(Phone))
                yield return new ValidationResult("Vui lòng nhập số điện thoại.", new[] { nameof(Phone) });

            if (string.IsNullOrWhiteSpace(AddressDetail))
                yield return new ValidationResult("Vui lòng nhập địa chỉ nhận hàng.", new[] { nameof(AddressDetail) });
        }
    }

    /// <summary>Mọi thứ màn đặt hàng cần hiện.</summary>
    public class CheckoutViewModel
    {
        public CartViewModel Gio { get; set; } = new();
        public List<Address> CacDiaChi { get; set; } = new();
        public CheckoutForm Form { get; set; } = new();

        /// <summary>False là khách vãng lai: không có sổ địa chỉ, giỏ lấy từ Session.</summary>
        public bool DaDangNhap { get; set; }

        /// <summary>Số tiền giảm nếu khách đã nhập mã hợp lệ (khi trang được vẽ lại sau lỗi).</summary>
        public decimal TienGiam { get; set; }
        public decimal TongThanhToan => Math.Max(0, Gio.TamTinh - TienGiam);
    }

    /// <summary>Trả về cho JavaScript khi khách bấm "Áp dụng" mã giảm giá.</summary>
    public record ApMaResult(bool Ok, string Message, decimal TienGiam, decimal TongThanhToan,
                             string TienGiamText, string TongThanhToanText);

    /// <summary>Kết quả đặt hàng.</summary>
    public record DatHangResult(bool Ok, string? Loi, Order? Don);
}
