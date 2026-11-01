using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>
    /// Nghiệp vụ tài khoản. Bọc quanh UserManager của Identity để controller
    /// không phải gọi thẳng, và để chỗ nào cũng dùng chung một luật.
    /// </summary>
    public interface IUserService
    {
        Task<UserListViewModel> LayDanhSachAsync(string? tuKhoa, int? roleId, bool? isActive,
                                                 int page, int pageSize);

        Task<User?> LayTheoIdAsync(int id);
        Task<User?> LayTheoEmailAsync(string email);
        Task<List<Role>> LayVaiTroAsync();

        /// <summary>Tên vai trò của một tài khoản. Shop chỉ gán mỗi người một vai.</summary>
        Task<string?> LayTenVaiTroAsync(User user);

        /// <summary>Khách tự đăng ký, luôn nhận vai trò Customer.</summary>
        Task<(bool ok, string? loi)> DangKyAsync(RegisterViewModel vm);

        Task<(bool ok, string? loi)> LuuAsync(UserFormViewModel vm);
        Task<(bool ok, string? loi)> CapNhatThongTinAsync(ProfileViewModel vm);
        Task<(bool ok, string? loi)> DoiMatKhauAsync(int userId, ChangePasswordViewModel vm);
        Task<(bool ok, string? loi)> DatLaiMatKhauAsync(int userId, string matKhauMoi);

        /// <summary>Bật hoặc tắt hoạt động. Không xoá cứng vì đơn hàng đang trỏ tới tài khoản.</summary>
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int userId, int idNguoiDangThaoTac);

        Task<int> DemTheoVaiTroAsync(string tenVaiTro);
    }
}
