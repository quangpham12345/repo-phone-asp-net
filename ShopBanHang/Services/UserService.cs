using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class UserService : IUserService
    {
        private readonly ShopContext _db;

        public UserService(ShopContext db) => _db = db;

        public async Task<PagedResult<User>> LayDanhSachAsync(string? tuKhoa, int? roleId,
                                                              bool? isActive, int page, int pageSize)
        {
            // Giữ ở dạng IQueryable để điều kiện dồn hết vào một câu SQL,
            // chỉ trang hiện tại mới được kéo về bộ nhớ.
            IQueryable<User> q = _db.Users.Include(u => u.Role).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tk = tuKhoa.Trim();
                q = q.Where(u => u.FullName.Contains(tk) || u.Email.Contains(tk));
            }
            if (roleId.HasValue) q = q.Where(u => u.RoleId == roleId.Value);
            if (isActive.HasValue) q = q.Where(u => u.IsActive == isActive.Value);

            q = q.OrderByDescending(u => u.Id);

            return await PagedResult<User>.TaoAsync(q, page, pageSize);
        }

        public async Task<User?> LayTheoIdAsync(int id)
            => await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id);

        public async Task<User?> LayTheoEmailAsync(string email)
            => await _db.Users.Include(u => u.Role)
                              .FirstOrDefaultAsync(u => u.Email == email);

        public async Task<List<Role>> LayVaiTroAsync()
            => await _db.Roles.OrderBy(r => r.Id).ToListAsync();

        public async Task<User?> XacThucAsync(string email, string matKhau)
        {
            var u = await LayTheoEmailAsync(email.Trim());
            if (u == null) return null;
            if (!u.IsActive) return null;                            // tài khoản bị tắt thì không cho vào
            return PasswordHelper.Verify(matKhau, u.PasswordHash) ? u : null;
        }

        public async Task<(bool ok, string? loi)> DangKyAsync(RegisterViewModel vm)
        {
            string email = vm.Email.Trim();
            if (await _db.Users.AnyAsync(u => u.Email == email))
                return (false, "Email này đã được dùng cho một tài khoản khác.");

            var vaiTroKhach = await _db.Roles.FirstOrDefaultAsync(r => r.Name == Role.Customer);
            if (vaiTroKhach == null)
                return (false, "Chưa có vai trò Customer trong hệ thống.");

            _db.Users.Add(new User
            {
                FullName = vm.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHelper.Hash(vm.Password),
                Phone = vm.Phone?.Trim(),
                Address = vm.Address?.Trim(),
                RoleId = vaiTroKhach.Id,       // khách tự đăng ký không bao giờ thành Admin
                IsActive = true
            });
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> LuuAsync(UserFormViewModel vm)
        {
            string email = vm.Email.Trim();

            bool trungEmail = await _db.Users.AnyAsync(u => u.Email == email && u.Id != vm.Id);
            if (trungEmail) return (false, "Email này đã được dùng cho một tài khoản khác.");

            if (!await _db.Roles.AnyAsync(r => r.Id == vm.RoleId))
                return (false, "Vai trò không hợp lệ.");

            if (vm.LaThemMoi)
            {
                if (string.IsNullOrWhiteSpace(vm.Password))
                    return (false, "Thêm tài khoản mới thì mật khẩu bắt buộc nhập.");

                _db.Users.Add(new User
                {
                    FullName = vm.FullName.Trim(),
                    Email = email,
                    PasswordHash = PasswordHelper.Hash(vm.Password),
                    Phone = vm.Phone?.Trim(),
                    Address = vm.Address?.Trim(),
                    RoleId = vm.RoleId,
                    IsActive = vm.IsActive
                });
            }
            else
            {
                var u = await _db.Users.FindAsync(vm.Id);
                if (u == null) return (false, "Không tìm thấy tài khoản cần sửa.");

                u.FullName = vm.FullName.Trim();
                u.Email = email;
                u.Phone = vm.Phone?.Trim();
                u.Address = vm.Address?.Trim();
                u.RoleId = vm.RoleId;
                u.IsActive = vm.IsActive;

                // Bỏ trống ô mật khẩu khi sửa nghĩa là giữ nguyên mật khẩu cũ
                if (!string.IsNullOrWhiteSpace(vm.Password))
                    u.PasswordHash = PasswordHelper.Hash(vm.Password);
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> CapNhatThongTinAsync(ProfileViewModel vm)
        {
            var u = await _db.Users.FindAsync(vm.Id);
            if (u == null) return (false, "Không tìm thấy tài khoản.");

            // Email và vai trò không cho tự sửa ở trang tài khoản của khách
            u.FullName = vm.FullName.Trim();
            u.Phone = vm.Phone?.Trim();
            u.Address = vm.Address?.Trim();

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DoiMatKhauAsync(int userId, ChangePasswordViewModel vm)
        {
            var u = await _db.Users.FindAsync(userId);
            if (u == null) return (false, "Không tìm thấy tài khoản.");

            if (!PasswordHelper.Verify(vm.CurrentPassword, u.PasswordHash))
                return (false, "Mật khẩu hiện tại không đúng.");

            u.PasswordHash = PasswordHelper.Hash(vm.NewPassword);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DatLaiMatKhauAsync(int userId, string matKhauMoi)
        {
            if (string.IsNullOrWhiteSpace(matKhauMoi) || matKhauMoi.Length < 6)
                return (false, "Mật khẩu mới phải có ít nhất 6 ký tự.");

            var u = await _db.Users.FindAsync(userId);
            if (u == null) return (false, "Không tìm thấy tài khoản.");

            u.PasswordHash = PasswordHelper.Hash(matKhauMoi);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int userId, int idNguoiDangThaoTac)
        {
            var u = await _db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == userId);
            if (u == null) return (false, "Không tìm thấy tài khoản.");

            if (u.Id == idNguoiDangThaoTac)
                return (false, "Không thể tự tắt tài khoản đang đăng nhập.");

            // Phải còn ít nhất một Admin đang hoạt động, nếu không sẽ không ai vào được trang quản trị
            if (u.IsActive && u.Role?.Name == Role.Admin)
            {
                int soAdminConBat = await _db.Users
                    .CountAsync(x => x.IsActive && x.Role!.Name == Role.Admin);
                if (soAdminConBat <= 1)
                    return (false, "Đây là Admin đang hoạt động cuối cùng, không thể tắt.");
            }

            u.IsActive = !u.IsActive;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<int> DemTheoVaiTroAsync(string tenVaiTro)
            => await _db.Users.CountAsync(u => u.Role!.Name == tenVaiTro);
    }
}
