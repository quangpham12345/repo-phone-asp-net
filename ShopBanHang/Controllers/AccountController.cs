using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers
{
    /// <summary>
    /// Đăng nhập, đăng ký, đăng xuất và trang tài khoản của người dùng.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService) => _userService = userService;

        // ===== Đăng nhập =====

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Đã đăng nhập rồi thì không cần vào lại trang này
            if (User.Identity?.IsAuthenticated == true) return RedirectTheoVaiTro();

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userService.XacThucAsync(vm.Email, vm.Password);
            if (user == null)
            {
                // Không nói rõ sai email hay sai mật khẩu, tránh để người ngoài dò email có tồn tại
                ModelState.AddModelError(string.Empty,
                    "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị tắt.");
                return View(vm);
            }

            await DangNhapBangCookie(user, vm.RememberMe);

            // Quay lại đúng trang người dùng định vào trước khi bị chặn.
            // Chỉ nhận đường dẫn nội bộ, chặn chuyển hướng ra ngoài tên miền.
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectTheoVaiTro(user.Role?.Name);
        }

        // ===== Đăng ký =====

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectTheoVaiTro();
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var (ok, loi) = await _userService.DangKyAsync(vm);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, loi!);
                return View(vm);
            }

            // Đăng ký xong cho vào luôn, khỏi bắt đăng nhập lại
            var user = await _userService.LayTheoEmailAsync(vm.Email.Trim());
            if (user != null) await DangNhapBangCookie(user, false);

            TempData["ThanhCong"] = "Đăng ký thành công. Chào mừng bạn đến với ShopBanHang.";
            return RedirectToAction("Index", "Home");
        }

        // ===== Đăng xuất =====

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // ===== Tài khoản của tôi =====

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userService.LayTheoIdAsync(LayIdDangNhap());
            if (user == null) return RedirectToAction(nameof(Login));

            return View(new ProfileViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Address = user.Address,
                RoleName = user.Role?.Name,
                CreatedAt = user.CreatedAt
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel vm)
        {
            // Chỉ cho sửa chính mình, không tin Id gửi lên từ form
            vm.Id = LayIdDangNhap();

            if (!ModelState.IsValid) return View(vm);

            var (ok, loi) = await _userService.CapNhatThongTinAsync(vm);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, loi!);
                return View(vm);
            }

            TempData["ThanhCong"] = "Đã cập nhật thông tin tài khoản.";
            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var (ok, loi) = await _userService.DoiMatKhauAsync(LayIdDangNhap(), vm);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, loi!);
                return View(vm);
            }

            TempData["ThanhCong"] = "Đã đổi mật khẩu.";
            return RedirectToAction(nameof(Profile));
        }

        // ===== Hàm dùng chung =====

        /// <summary>
        /// Phát cookie đăng nhập kèm các claim. Vai trò đi vào ClaimTypes.Role
        /// nên [Authorize(Roles = "Admin")] và User.IsInRole dùng được ngay.
        /// </summary>
        private async Task DangNhapBangCookie(User user, bool ghiNho)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role?.Name ?? Role.Customer)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    // Ghi nhớ thì cookie sống qua lần tắt trình duyệt, không thì hết phiên là mất
                    IsPersistent = ghiNho,
                    ExpiresUtc = ghiNho ? DateTimeOffset.UtcNow.AddDays(14) : null
                });
        }

        /// <summary>Rẽ nhánh sau đăng nhập: Admin vào khu quản trị, khách về trang bán hàng.</summary>
        private IActionResult RedirectTheoVaiTro(string? tenVaiTro = null)
        {
            tenVaiTro ??= User.FindFirstValue(ClaimTypes.Role);

            return tenVaiTro == Role.Admin
                ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
                : RedirectToAction("Index", "Home");
        }

        private int LayIdDangNhap()
            => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;
    }
}
