using ShopBanHang.Models.Entities;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>
    /// Nghiệp vụ biến thể sản phẩm (dung lượng × màu). Mỗi lần thêm, sửa, tắt, xoá biến thể,
    /// service tự đồng bộ Product.Price, OldPrice và Stock từ các biến thể.
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
        /// Tính lại giá và tồn kho của sản phẩm từ các biến thể. Sản phẩm chưa có biến thể
        /// thì không làm gì, giá và tồn kho nhập tay được giữ nguyên.
        /// </summary>
        Task DongBoSanPhamAsync(int productId);

        /// <summary>
        /// Gỡ ảnh riêng của các biến thể đang trỏ tới những ảnh vừa bị xoá khỏi sản phẩm,
        /// để biến thể quay về dùng ảnh chính thay vì hiện ảnh hỏng.
        /// </summary>
        Task GoAnhAsync(int productId, IEnumerable<string> cacAnh);
    }
}