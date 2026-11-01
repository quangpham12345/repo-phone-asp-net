using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>
    /// Nghiệp vụ tài khoản. Cả MVC controller và API controller đều gọi vào đây,
    /// nên luật chỉ viết một lần.
    /// </summary>
    public interface IUserService
    {
        Task<PagedResult<User>> LayDanhSachAsync(string? tuKhoa, int? roleId, bool? isActive,
                                                 int page, int pageSize);

        Task<User?> LayTheoIdAsync(int id);
        Task<User?> LayTheoEmailAsync(string email);
        Task<List<Role>> LayVaiTroAsync();

        /// <summary>Kiểm email và mật khẩu. Trả về null nếu sai, hoặc tài khoản đang bị tắt.</summary>
        Task<User?> XacThucAsync(string email, string matKhau);

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
