using ShopBanHang.Helpers;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>
    /// Giỏ hàng theo biến thể.
    /// Khách đã đăng nhập: giỏ lưu trong bảng CartItems.
    /// Khách chưa đăng nhập: giỏ lưu trong Session; controller đọc/ghi Session, service chỉ kiểm tra và tính.
    /// </summary>
    public interface ICartService
    {
        // ---------- Khách đã đăng nhập ----------

        /// <summary>Giỏ kèm giá và tồn kho hiện tại của từng biến thể.</summary>
        Task<CartViewModel> LayGioAsync(int userId);

        /// <summary>Thêm biến thể vào giỏ; đã có thì cộng dồn số lượng. Không cho vượt tồn kho.</summary>
        Task<(bool ok, string? loi)> ThemAsync(int userId, int variantId, int soLuong);

        /// <summary>Đặt lại số lượng của một dòng. Số lượng 0 là xoá dòng.</summary>
        Task<(bool ok, string? loi)> CapNhatAsync(int userId, int cartItemId, int soLuong);

        Task<(bool ok, string? loi)> XoaAsync(int userId, int cartItemId);
        Task XoaHetAsync(int userId);

        /// <summary>Tổng số lượng trong giỏ, cho badge trên header.</summary>
        Task<int> DemAsync(int userId);

        // ---------- Khách chưa đăng nhập (giỏ trong Session) ----------

        /// <summary>Ở giỏ khách lẻ, CartItemId của mỗi dòng chính là VariantId.</summary>
        Task<CartViewModel> LayGioKhachAsync(IReadOnlyList<GuestCartLine> gio);

        /// <summary>Kiểm tra rồi thêm vào danh sách (sửa trực tiếp <paramref name="gio"/>).</summary>
        Task<(bool ok, string? loi)> ThemKhachAsync(List<GuestCartLine> gio, int variantId, int soLuong);

        /// <summary>Đặt lại số lượng một dòng trong danh sách; 0 là xoá.</summary>
        Task<(bool ok, string? loi)> CapNhatKhachAsync(List<GuestCartLine> gio, int variantId, int soLuong);

        /// <summary>Gộp giỏ khách lẻ vào giỏ của tài khoản vừa đăng nhập. Dòng nào vượt tồn kho thì cắt bớt.</summary>
        Task GopGioKhachAsync(int userId, IReadOnlyList<GuestCartLine> gio);
    }
}
