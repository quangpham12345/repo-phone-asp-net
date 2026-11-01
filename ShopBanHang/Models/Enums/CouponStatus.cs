using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Enums
{
    /// <summary>
    /// Trạng thái của mã giảm giá tại một thời điểm. Không lưu trong DB mà tính
    /// từ IsActive, StartDate, EndDate, UsageLimit, UsedCount (xem Coupon.LayTrangThai).
    /// </summary>
    public enum CouponStatus
    {
        [Display(Name = "Đang chạy")]
        DangChay = 1,

        [Display(Name = "Sắp diễn ra")]
        SapDienRa = 2,

        [Display(Name = "Hết hạn")]
        HetHan = 3,

        [Display(Name = "Hết lượt dùng")]
        HetLuot = 4,

        [Display(Name = "Đã tắt")]
        DaTat = 5
    }
}