using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Enums
{
    /// <summary>Hình thức thanh toán khách chọn khi đặt hàng.</summary>
    public enum PaymentMethod
    {
        [Display(Name = "Thanh toán khi nhận hàng")]
        COD = 0,

        [Display(Name = "Chuyển khoản ngân hàng")]
        ChuyenKhoan = 1
    }
}
