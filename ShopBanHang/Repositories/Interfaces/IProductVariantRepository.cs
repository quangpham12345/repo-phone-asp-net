using ShopBanHang.Models.Entities;

namespace ShopBanHang.Repositories.Interfaces
{
    /// <summary>Các thao tác kho mà giỏ hàng và đơn hàng cần: đọc biến thể, trừ/hoàn tồn kho, cộng số đã bán.</summary>
    public interface IProductVariantRepository
    {
        /// <summary>Biến thể kèm sản phẩm cha, không theo dõi.</summary>
        Task<ProductVariant?> LayKemSanPhamAsync(int id);

        /// <summary>Nhiều biến thể kèm sản phẩm cha, không theo dõi (dùng cho giỏ của khách chưa đăng nhập).</summary>
        Task<List<ProductVariant>> LayNhieuKemSanPhamAsync(IEnumerable<int> ids);

        /// <summary>
        /// Trừ tồn kho bằng một câu UPDATE có điều kiện Stock &gt;= soLuong.
        /// Hai người cùng mua chiếc cuối thì chỉ một người trừ được, người kia nhận false.
        /// </summary>
        Task<bool> TruTonKhoAsync(int variantId, int soLuong);

        /// <summary>Cộng trả tồn kho khi huỷ đơn.</summary>
        Task HoanTonKhoAsync(int variantId, int soLuong);

        /// <summary>Cộng SoldCount của sản phẩm khi đơn hoàn thành.</summary>
        Task CongDaBanAsync(int productId, int soLuong);
    }
}
