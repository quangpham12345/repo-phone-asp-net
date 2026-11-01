using ShopBanHang.Models.Entities;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>Nghiệp vụ banner trang chủ.</summary>
    public interface IBannerService
    {
        /// <summary>Các banner đang bật, dùng cho slideshow trang chủ.</summary>
        Task<List<Banner>> LayDangHienAsync();

        /// <summary>Toàn bộ banner kể cả đã tắt, dùng cho khu quản trị.</summary>
        Task<List<Banner>> LayTatCaAsync();

        Task<Banner?> LayTheoIdAsync(int id);
        Task<(bool ok, string? loi)> LuuAsync(Banner banner);
        Task<(bool ok, string? loi)> XoaAsync(int id);
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id);
    }
}
