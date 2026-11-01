using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>Tổng hợp số liệu cho trang tổng quan khu quản trị.</summary>
    public interface IThongKeService
    {
        /// <summary>Lấy trọn bộ số liệu của trang tổng quan trong một lần gọi.</summary>
        Task<DashboardViewModel> LaySoLieuAsync();

        Task<List<DoanhThuNgay>> LayDoanhThu30NgayAsync();
        Task<List<DoanhThuThang>> LayDoanhThu12ThangAsync();
        Task<List<SanPhamBanChay>> LayTopSanPhamAsync(int soLuong);
        Task<List<KhachHangTop>> LayTopKhachHangAsync(int soLuong);
    }
}
