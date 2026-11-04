using ShopBanHang.Models.Entities;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>
    /// Nghiệp vụ biến thể sản phẩm (dung lượng × màu). Giá và tồn kho chỉ nằm ở biến thể;
    /// giá và tồn kho hiển thị của cả sản phẩm do Product.DisplayPrice, TotalStock tính ra khi đọc.
    /// </summary>
    public interface IProductVariantService
    {
        /// <summary>Các biến thể của một sản phẩm, kể cả đã tắt, dùng cho khu quản trị.</summary>
        Task<List<ProductVariant>> LayTheoSanPhamAsync(int productId);

        Task<ProductVariant?> LayTheoIdAsync(int id);
        Task<int> DemAsync(int productId);

        Task<(bool ok, string? loi)> LuuAsync(ProductVariant bienThe);
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id);
        Task<(bool ok, string? loi)> XoaAsync(int id);

        /// <summary>
        /// Gỡ ảnh riêng của các biến thể đang trỏ tới những ảnh vừa bị xoá khỏi sản phẩm,
        /// để biến thể quay về dùng ảnh chính thay vì hiện ảnh hỏng.
        /// </summary>
        Task GoAnhAsync(int productId, IEnumerable<string> cacAnh);
    }
}