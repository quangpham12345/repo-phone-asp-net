using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>Nghiệp vụ danh mục sản phẩm.</summary>
    public interface ICategoryService
    {
        Task<List<Category>> LayTatCaAsync();
        Task<Category?> LayTheoIdAsync(int id);
        Task<(bool ok, string? loi)> LuuAsync(Category c);
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id);

        /// <summary>Chỉ xoá được khi không còn sản phẩm nào thuộc danh mục.</summary>
        Task<(bool ok, string? loi)> XoaAsync(int id);
    }

    /// <summary>Nghiệp vụ thương hiệu.</summary>
    public interface IBrandService
    {
        Task<List<Brand>> LayTatCaAsync();
        Task<Brand?> LayTheoIdAsync(int id);
        Task<(bool ok, string? loi)> LuuAsync(Brand b);
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id);
        Task<(bool ok, string? loi)> XoaAsync(int id);
    }

    /// <summary>Nghiệp vụ tin tức.</summary>
    public interface INewsService
    {
        Task<PagedResult<News>> LayDanhSachAsync(string? tuKhoa, bool? active, int page, int pageSize);

        /// <summary>Tin đang hiện, dùng cho trang bán hàng.</summary>
        Task<List<News>> LayMoiNhatAsync(int soLuong);

        Task<News?> LayTheoIdAsync(int id);
        Task<News?> LayTheoSlugAsync(string slug);
        Task<(bool ok, string? loi)> LuuAsync(News n);
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id);
        Task<(bool ok, string? loi)> XoaAsync(int id);
    }
}
