using ShopBanHang.Helpers;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>
    /// Bước đặt hàng: chọn/nhập địa chỉ, áp mã giảm giá, tạo đơn, trừ tồn kho.
    ///
    /// Chạy được cho cả hai loại khách:
    /// userId &gt; 0 là khách đã đăng nhập, giỏ lấy từ bảng CartItems;
    /// userId = 0 là khách vãng lai, giỏ lấy từ <c>gioKhach</c> (controller đọc từ Session).
    /// </summary>
    public interface ICheckoutService
    {
        /// <summary>Dữ liệu cho màn đặt hàng. Khách đã đăng nhập thì điền sẵn địa chỉ mặc định và email.</summary>
        Task<CheckoutViewModel> ChuanBiAsync(int userId, IReadOnlyList<GuestCartLine> gioKhach, CheckoutForm? form = null);

        /// <summary>Kiểm tra mã với tiền hàng hiện tại trong giỏ (nút "Áp dụng").</summary>
        Task<ApMaResult> ApMaAsync(int userId, IReadOnlyList<GuestCartLine> gioKhach, string? code);

        /// <summary>
        /// Tạo đơn từ toàn bộ giỏ trong một transaction: trừ tồn kho từng biến thể,
        /// cộng lượt dùng mã, lưu đơn, làm trống giỏ trong CSDL (khách đã đăng nhập).
        /// Giỏ Session của khách vãng lai do controller xoá sau khi đặt thành công.
        /// </summary>
        Task<DatHangResult> DatHangAsync(int userId, IReadOnlyList<GuestCartLine> gioKhach, CheckoutForm form);
    }
}
