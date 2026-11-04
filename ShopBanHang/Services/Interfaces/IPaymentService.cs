using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>Thanh toán: tạo mã QR chuyển khoản và xác nhận đã nhận tiền.</summary>
    public interface IPaymentService
    {
        /// <summary>Đơn này có cần hiện QR không: chuyển khoản, chưa trả tiền, chưa huỷ.</summary>
        bool CanThanhToanQr(Order don);

        /// <summary>Link ảnh VietQR đã điền sẵn số tiền và nội dung là mã đơn.</summary>
        string TaoQrUrl(Order don);

        ThongTinChuyenKhoan LayThongTinChuyenKhoan(Order don);

        /// <summary>Admin xác nhận đã nhận tiền của đơn.</summary>
        Task<(bool ok, string? loi)> XacNhanDaThanhToanAsync(int orderId);
    }
}
