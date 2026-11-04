using ShopBanHang.Models.Entities;

namespace ShopBanHang.Repositories.Interfaces
{
    /// <summary>Truy cập bảng CartItems.</summary>
    public interface ICartRepository
    {
        /// <summary>Giỏ của một người, kèm biến thể và sản phẩm của từng dòng (có theo dõi để còn sửa, xoá).</summary>
        Task<List<CartItem>> LayCuaUserAsync(int userId);

        /// <summary>Dòng giỏ của đúng biến thể này, null nếu chưa có.</summary>
        Task<CartItem?> LayDongAsync(int userId, int variantId);

        /// <summary>Chỉ trả về nếu dòng giỏ thuộc về chính người đang hỏi.</summary>
        Task<CartItem?> LayTheoIdAsync(int id, int userId);

        /// <summary>Tổng số lượng sản phẩm trong giỏ, dùng cho badge trên header.</summary>
        Task<int> DemSoLuongAsync(int userId);

        void Them(CartItem item);
        void Xoa(CartItem item);
        void XoaNhieu(IEnumerable<CartItem> items);
    }
}
