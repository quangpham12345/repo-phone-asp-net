using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>
    /// Nghiệp vụ sản phẩm. MVC controller và API controller đều gọi vào đây.
    /// </summary>
    public interface IProductService
    {
        /// <summary>Sản phẩm nổi bật cho trang chủ.</summary>
        Task<List<Product>> LayNoiBatAsync(int soLuong);

        /// <summary>Sản phẩm mới nhất cho trang chủ.</summary>
        Task<List<Product>> LayMoiNhatAsync(int soLuong);

        /// <summary>
        /// Danh sách có lọc và phân trang cho trang danh mục. Các Product trả về đã Include Variants,
        /// nên DisplayPrice, TotalStock... dùng được ngay. Lọc giá và sắp xếp giá tính theo giá thấp nhất
        /// trong các biến thể đang bán; lọc dung lượng khớp khi có biến thể đang bán đúng dung lượng đó.
        /// </summary>
        Task<PagedResult<Product>> LayDanhSachAsync(ProductFilter loc, int page, int pageSize);

        Task<Product?> LayTheoSlugAsync(string slug);
        Task<Product?> LayTheoIdAsync(int id);

        /// <summary>Sản phẩm cùng danh mục, trừ chính nó.</summary>
        Task<List<Product>> LayLienQuanAsync(Product sp, int soLuong);

        Task<List<Category>> LayDanhMucAsync();
        Task<List<Brand>> LayThuongHieuAsync();
        Task<List<Brand>> LayThuongHieuAsync(int soLuong);
        Task<List<Product>> LayBanChayAsync(int soLuong);
        Task<int> DemAsync();
        Task<List<ProductSuggestItem>> GoiYAsync(string tuKhoa, int soLuong);
        Task<(List<string> ram, List<string> storage, List<string> os)> LayGiaTriLocAsync();
        Task<List<Product>> LayTheoIdsAsync(IEnumerable<int> ids);

        /// <summary>
        /// Sản phẩm đang bán theo danh sách id, GIỮ ĐÚNG THỨ TỰ của danh sách
        /// (dùng cho "Sản phẩm bạn đã xem" đọc từ cookie). Id không tồn tại hoặc đã ẩn thì bỏ qua.
        /// </summary>
        Task<List<Product>> LayTheoThuTuIdAsync(IList<int> ids);
        // ===== Phần quản trị =====

        /// <summary>Lưu thêm mới hoặc cập nhật. Slug sinh tự động từ tên.</summary>
        Task<(bool ok, string? loi)> LuuAsync(ProductFormViewModel vm);

        /// <summary>Ẩn hoặc hiện lại sản phẩm. Không xoá cứng vì đơn hàng đang trỏ tới.</summary>
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id);

        /// <summary>Bật tắt cờ nổi bật trên trang chủ.</summary>
        Task<(bool ok, string? loi)> DoiNoiBatAsync(int id);

        /// <summary>Biến thể đang bán (của sản phẩm đang bán) mà tồn kho từ ngưỡng trở xuống, ít nhất lên trước.</summary>
        Task<List<CanhBaoTonKhoItem>> LayCanhBaoTonKhoAsync();

        /// <summary>Chỉ đếm, dùng cho số trên menu bên trái.</summary>
        Task<int> DemCanhBaoTonKhoAsync();
    }
}
