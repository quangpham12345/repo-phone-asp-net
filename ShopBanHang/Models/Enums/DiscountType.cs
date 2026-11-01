using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Enums
{
    /// <summary>Cách tính tiền giảm của một mã giảm giá.</summary>
    public enum DiscountType
    {
        [Display(Name = "Giảm theo %")]
        PhanTram = 1,

        [Display(Name = "Giảm số tiền cố định")]
        SoTien = 2
    }
}