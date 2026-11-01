using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class UserService : IUserService
    {
        private readonly ShopContext _db;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public UserService(ShopContext db, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<UserListViewModel> LayDanhSachAsync(string? tuKhoa, int? roleId,
                                                              bool? isActive, int page, int pageSize)
        {
            IQueryable<User> q = _db.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim();
                q = q.Where(u => u.FullName.Contains(tk) || u.Email!.Contains(tk));
            }
            if (isActive.HasValue) q = q.Where(u => u.IsActive == isActive.Value);

            // Identity nối người dùng với vai trò qua bảng AspNetUserRoles,
            // nên lọc theo vai phải đi qua bảng nối chứ không còn cột RoleId.
            if (roleId.HasValue)
            {
                var idTrongVai = _db.UserRoles.Where(ur => ur.RoleId == roleId.Value).Select(ur => ur.UserId);
                q = q.Where(u => idTrongVai.Contains(u.Id));
            }

            q = q.OrderByDescending(u => u.Id);

            var ketQua = await PagedResult<User>.TaoAsync(q, page, pageSize);

            // Lấy tên vai trò cho cả trang bằng MỘT truy vấn, thay vì hỏi
            // UserManager từng người một
            var idTrang = ketQua.Items.Select(u => u.Id).ToList();
            var vaiTro = await (from ur in _db.UserRoles
                                join r in _db.Roles on ur.RoleId equals r.Id
                                where idTrang.Contains(ur.UserId)
                                select new { ur.UserId, r.Name })
                               .ToListAsync();

            return new UserListViewModel
            {
                KetQua = ketQua,
                TuKhoa = tuKhoa,
                RoleId = roleId,
                IsActive = isActive,
                CacVaiTro = await LayVaiTroAsync(),
                VaiTroCuaUser = vaiTro
                    .GroupBy(x => x.UserId)
                    .ToDictionary(g => g.Key, g => g.First().Name ?? "")
            };
        }

        public async Task<User?> LayTheoIdAsync(int id) => await _userManager.FindByIdAsync(id.ToString());

        public async Task<User?> LayTheoEmailAsync(string email)
            => await _userManager.FindByEmailAsync(email.Trim());

        public async Task<List<Role>> LayVaiTroAsync()
            => await _roleManager.Roles.OrderBy(r => r.Id).ToListAsync();

        public async Task<string?> LayTenVaiTroAsync(User user)
            => (await _userManager.GetRolesAsync(user)).FirstOrDefault();

        public async Task<(bool ok, string? loi)> DangKyAsync(RegisterViewModel vm)
        {
            var user = new User
            {
                UserName = vm.Email.Trim(),     // Identity bắt buộc có UserName, dùng luôn email
                Email = vm.Email.Trim(),
                FullName = vm.FullName.Trim(),
                PhoneNumber = vm.Phone?.Trim(),
                Address = vm.Address?.Trim(),
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var kq = await _userManager.CreateAsync(user, vm.Password);
            if (!kq.Succeeded) return (false, GomLoi(kq));

            // Khách tự đăng ký không bao giờ thành Admin
            await _userManager.AddToRoleAsync(user, Role.Customer);
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> LuuAsync(UserFormViewModel vm)
        {
            var vaiTro = await _roleManager.FindByIdAsync(vm.RoleId.ToString());
            if (vaiTro == null) return (false, "Vai trò không hợp lệ.");

            if (vm.LaThemMoi)
            {
                if (string.IsNullOrWhiteSpace(vm.Password))
                    return (false, "Thêm tài khoản mới thì mật khẩu bắt buộc nhập.");

                var moi = new User
                {
                    UserName = vm.Email.Trim(),
                    Email = vm.Email.Trim(),
                    FullName = vm.FullName.Trim(),
                    PhoneNumber = vm.Phone?.Trim(),
                    Address = vm.Address?.Trim(),
                    IsActive = vm.IsActive,
                    CreatedAt = DateTime.Now
                };

                var kq = await _userManager.CreateAsync(moi, vm.Password);
                if (!kq.Succeeded) return (false, GomLoi(kq));

                await _userManager.AddToRoleAsync(moi, vaiTro.Name!);
                return (true, null);
            }

            var u = await _userManager.FindByIdAsync(vm.Id.ToString());
            if (u == null) return (false, "Không tìm thấy tài khoản cần sửa.");

            u.FullName = vm.FullName.Trim();
            u.Email = vm.Email.Trim();
            u.UserName = vm.Email.Trim();
            u.PhoneNumber = vm.Phone?.Trim();
            u.Address = vm.Address?.Trim();
            u.IsActive = vm.IsActive;

            var kqSua = await _userManager.UpdateAsync(u);
            if (!kqSua.Succeeded) return (false, GomLoi(kqSua));

            // Bỏ trống ô mật khẩu khi sửa nghĩa là giữ nguyên mật khẩu cũ
            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                string token = await _userManager.GeneratePasswordResetTokenAsync(u);
                var kqMk = await _userManager.ResetPasswordAsync(u, token, vm.Password);
                if (!kqMk.Succeeded) return (false, GomLoi(kqMk));
            }

            // Shop chỉ cho mỗi người một vai, nên đổi vai là gỡ hết vai cũ rồi gán vai mới
            var vaiCu = await _userManager.GetRolesAsync(u);
            if (!vaiCu.Contains(vaiTro.Name!))
            {
                if (vaiCu.Count > 0) await _userManager.RemoveFromRolesAsync(u, vaiCu);
                await _userManager.AddToRoleAsync(u, vaiTro.Name!);
            }

            return (true, null);
        }

        public async Task<(bool ok, string? loi)> CapNhatThongTinAsync(ProfileViewModel vm)
        {
            var u = await _userManager.FindByIdAsync(vm.Id.ToString());
            if (u == null) return (false, "Không tìm thấy tài khoản.");

            // Email và vai trò không cho tự sửa ở trang tài khoản của khách
            u.FullName = vm.FullName.Trim();
            u.PhoneNumber = vm.Phone?.Trim();
            u.Address = vm.Address?.Trim();

            var kq = await _userManager.UpdateAsync(u);
            return kq.Succeeded ? (true, null) : (false, GomLoi(kq));
        }

        public async Task<(bool ok, string? loi)> DoiMatKhauAsync(int userId, ChangePasswordViewModel vm)
        {
            var u = await _userManager.FindByIdAsync(userId.ToString());
            if (u == null) return (false, "Không tìm thấy tài khoản.");

            var kq = await _userManager.ChangePasswordAsync(u, vm.CurrentPassword, vm.NewPassword);
            if (kq.Succeeded) return (true, null);

            // Sai mật khẩu cũ thì Identity trả mã PasswordMismatch, dịch cho dễ hiểu
            if (kq.Errors.Any(e => e.Code == "PasswordMismatch"))
                return (false, "Mật khẩu hiện tại không đúng.");

            return (false, GomLoi(kq));
        }

        public async Task<(bool ok, string? loi)> DatLaiMatKhauAsync(int userId, string matKhauMoi)
        {
            if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Length < 6)
                return (false, "Mật khẩu mới phải có ít nhất 6 ký tự.");

            var u = await _userManager.FindByIdAsync(userId.ToString());
            if (u == null) return (false, "Không tìm thấy tài khoản.");

            string token = await _userManager.GeneratePasswordResetTokenAsync(u);
            var kq = await _userManager.ResetPasswordAsync(u, token, matKhauMoi);
            return kq.Succeeded ? (true, null) : (false, GomLoi(kq));
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int userId, int idNguoiDangThaoTac)
        {
            var u = await _userManager.FindByIdAsync(userId.ToString());
            if (u == null) return (false, "Không tìm thấy tài khoản.");

            if (u.Id == idNguoiDangThaoTac)
                return (false, "Không thể tự tắt tài khoản đang đăng nhập.");

            // Phải còn ít nhất một Admin đang hoạt động, nếu không sẽ không ai vào được trang quản trị
            if (u.IsActive && await _userManager.IsInRoleAsync(u, Role.Admin))
            {
                int soAdminConBat = (await _userManager.GetUsersInRoleAsync(Role.Admin))
                                    .Count(x => x.IsActive);
                if (soAdminConBat <= 1)
                    return (false, "Đây là Admin đang hoạt động cuối cùng, không thể tắt.");
            }

            u.IsActive = !u.IsActive;
            var kq = await _userManager.UpdateAsync(u);
            return kq.Succeeded ? (true, null) : (false, GomLoi(kq));
        }

        public async Task<int> DemTheoVaiTroAsync(string tenVaiTro)
            => (await _userManager.GetUsersInRoleAsync(tenVaiTro)).Count;

        /// <summary>Gộp các lỗi Identity trả về thành một câu đọc được.</summary>
        private static string GomLoi(IdentityResult kq)
            => string.Join(" ", kq.Errors.Select(e => DichLoi(e)));

        private static string DichLoi(IdentityError e) => e.Code switch
        {
            "DuplicateUserName" or "DuplicateEmail" => "Email này đã được dùng cho một tài khoản khác.",
            "PasswordTooShort" => "Mật khẩu quá ngắn.",
            "PasswordRequiresDigit" => "Mật khẩu phải có ít nhất một chữ số.",
            "PasswordRequiresUpper" => "Mật khẩu phải có ít nhất một chữ hoa.",
            "PasswordRequiresLower" => "Mật khẩu phải có ít nhất một chữ thường.",
            "PasswordRequiresNonAlphanumeric" => "Mật khẩu phải có ít nhất một ký tự đặc biệt.",
            "InvalidEmail" => "Email không đúng định dạng.",
            _ => e.Description
        };
    }
}
