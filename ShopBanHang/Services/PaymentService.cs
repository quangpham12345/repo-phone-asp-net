using Microsoft.Extensions.Options;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Repositories.Interfaces;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    /// <summary>
    /// Thanh toán chuyển khoản bằng mã VietQR.
    ///
    /// Không gọi cổng thanh toán nào: shop chỉ đưa cho khách mã QR đã điền sẵn
    /// số tiền và nội dung là mã đơn. Khách quét bằng app ngân hàng và chuyển.
    /// Admin xem sao kê, thấy tiền về đúng nội dung thì bấm "Xác nhận đã thanh toán".
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly ThanhToanOptions _opt;
        private readonly IOrderRepository _orders;
        private readonly IUnitOfWork _uow;

        public PaymentService(IOptions<ThanhToanOptions> opt, IOrderRepository orders, IUnitOfWork uow)
        {
            _opt = opt.Value;
            _orders = orders;
            _uow = uow;
        }

        public bool CanThanhToanQr(Order don)
            => don.PaymentMethod == PaymentMethod.ChuyenKhoan
            && !don.IsPaid
            && don.Status != OrderStatus.DaHuy;

        public string TaoQrUrl(Order don)
        {
            // Quick Link của VietQR, không cần khoá API:
            // https://img.vietqr.io/image/<BANK>-<SO_TK>-<TEMPLATE>.png?amount=..&addInfo=..&accountName=..
            string soTien = ((long)don.Total).ToString();
            return $"https://img.vietqr.io/image/{Uri.EscapeDataString(_opt.BankId)}-{Uri.EscapeDataString(_opt.AccountNo)}-{_opt.Template}.png"
                 + $"?amount={soTien}"
                 + $"&addInfo={Uri.EscapeDataString(NoiDung(don))}"
                 + $"&accountName={Uri.EscapeDataString(_opt.AccountName)}";
        }

        public ThongTinChuyenKhoan LayThongTinChuyenKhoan(Order don)
            => new(_opt.BankName, _opt.AccountNo, _opt.AccountName, don.Total, NoiDung(don));

        public async Task<(bool ok, string? loi)> XacNhanDaThanhToanAsync(int orderId)
        {
            var don = await _orders.LayDeSuaAsync(orderId);
            if (don == null) return (false, "Không tìm thấy đơn hàng.");
            if (don.Status == OrderStatus.DaHuy) return (false, "Đơn đã huỷ, không xác nhận thanh toán được.");
            if (don.IsPaid) return (false, "Đơn này đã được xác nhận thanh toán từ trước.");

            don.IsPaid = true;
            don.PaidAt = DateTime.Now;
            await _uow.SaveChangesAsync();
            return (true, null);
        }

        /// <summary>
        /// Nội dung chuyển khoản là mã đơn bỏ dấu gạch (DH202611040001), vì vài ngân hàng
        /// tự xoá ký tự đặc biệt trong nội dung, làm admin khó dò sao kê.
        /// </summary>
        private static string NoiDung(Order don) => don.OrderCode.Replace("-", "");
    }
}
