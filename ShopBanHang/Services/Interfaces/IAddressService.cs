using ShopBanHang.Models.Entities;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>Sổ địa chỉ giao hàng của khách.</summary>
    public interface IAddressService
    {
        Task<List<Address>> LayCuaUserAsync(int userId);
        Task<Address?> LayMacDinhAsync(int userId);

        /// <summary>Chỉ trả về nếu địa chỉ thuộc về chính người đang hỏi.</summary>
        Task<Address?> LayTheoIdAsync(int id, int userId);

        Task<(bool ok, string? loi)> LuuAsync(Address dc, int userId);
        Task<(bool ok, string? loi)> DatMacDinhAsync(int id, int userId);
        Task<(bool ok, string? loi)> XoaAsync(int id, int userId);
    }

    /// <summary>Đánh giá sản phẩm.</summary>
    public interface IReviewService
    {
        Task<List<Review>> LayCuaSanPhamAsync(int productId);

        /// <summary>Điểm trung bình và số lượt đánh giá của một sản phẩm.</summary>
        Task<(double trungBinh, int soLuong)> ThongKeAsync(int productId);

        /// <summary>Khách đã mua và nhận hàng thành công sản phẩm này chưa.</summary>
        Task<bool> DaMuaAsync(int productId, int userId);

        Task<Review?> LayCuaToiAsync(int productId, int userId);
        Task<(bool ok, string? loi)> LuuAsync(int productId, int userId, int soSao, string? nhanXet);
        Task<(bool ok, string? loi)> XoaCuaToiAsync(int productId, int userId);
    }
}
