using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>Bước đặt hàng: chọn địa chỉ, áp mã giảm giá, tạo đơn, trừ tồn kho.</summary>
    public interface ICheckoutService
    {
        /// <summary>Dữ liệu cho màn đặt hàng, điền sẵn địa chỉ mặc định và email của khách.</summary>
        Task<CheckoutViewModel> ChuanBiAsync(int userId, CheckoutForm? form = null);

        /// <summary>Kiểm tra mã với tiền hàng hiện tại trong giỏ (nút "Áp dụng").</summary>
        Task<ApMaResult> ApMaAsync(int userId, string? code);

        /// <summary>
        /// Tạo đơn từ toàn bộ giỏ trong một transaction: trừ tồn kho từng biến thể,
        /// cộng lượt dùng mã, lưu đơn, làm trống giỏ. Lỗi ở bất kỳ bước nào thì huỷ hết.
        /// </summary>
        Task<DatHangResult> DatHangAsync(int userId, CheckoutForm form);
    }
}
