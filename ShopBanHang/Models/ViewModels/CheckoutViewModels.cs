using System.ComponentModel.DataAnnotations;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;

namespace ShopBanHang.Models.ViewModels
{
    /// <summary>
    /// Dữ liệu khách gửi lên ở bước đặt hàng.
    ///
    /// Địa chỉ có hai cách: chọn một địa chỉ trong sổ (AddressId), hoặc nhập địa chỉ mới
    /// (AddressId = null, điền ba ô bên dưới). Validate() kiểm đúng nhánh khách đã chọn.
    /// </summary>
    public class CheckoutForm : IValidatableObject
    {
        [Display(Name = "Địa chỉ nhận hàng")]
        public int? AddressId { get; set; }

        [StringLength(100, ErrorMessage = "Tên người nhận tối đa 100 ký tự")]
        [Display(Name = "Người nhận")]
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

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        [Display(Name = "Email nhận thông báo")]
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
                yield return new ValidationResult("Vui lòng nhập tên người nhận.", new[] { nameof(ReceiverName) });

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
