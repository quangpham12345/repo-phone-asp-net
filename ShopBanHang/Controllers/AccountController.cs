using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers
{
    /// <summary>
    /// Đăng nhập, đăng ký, đăng xuất, quên mật khẩu và trang tài khoản.
    ///
    /// Dùng SignInManager/UserManager của Identity nhưng vẫn giữ controller
    /// của mình, không scaffold Identity UI. Nhờ vậy chỗ gộp giỏ hàng và gộp
    /// chat của khách lẻ sau khi đăng nhập vẫn nằm ngay trong file này.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IAddressService _addressService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public AccountController(IUserService userService,
                                 IAddressService addressService,
                                 UserManager<User> userManager,
                                 SignInManager<User> signInManager)
        {
            _userService = userService;
            _addressService = addressService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ===== Đăng nhập =====

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectTheoVaiTro();
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email.Trim());

            // Cờ IsActive là của shop, SignInManager không biết nên phải tự kiểm.
            // Không nói rõ sai email hay sai mật khẩu, tránh để người ngoài dò email có tồn tại.
            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(string.Empty,
                    "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị tắt.");
                return View(vm);
            }

            var kq = await _signInManager.PasswordSignInAsync(
                user, vm.Password, vm.RememberMe, lockoutOnFailure: true);

            if (kq.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty,
                    "Tài khoản tạm bị khoá do nhập sai mật khẩu nhiều lần. Thử lại sau ít phút.");
                return View(vm);
            }
            if (!kq.Succeeded)
            {
                ModelState.AddModelError(string.Empty,
                    "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị tắt.");
                return View(vm);
            }

            await GopDuLieuKhachLe(user);

            // Quay lại đúng trang người dùng định vào trước khi bị chặn.
            // Chỉ nhận đường dẫn nội bộ, chặn chuyển hướng ra ngoài tên miền.
            if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                return Redirect(vm.ReturnUrl);

            return RedirectTheoVaiTro(await _userService.LayTenVaiTroAsync(user));
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
            var user = await _userManager.FindByEmailAsync(vm.Email.Trim());
            if (user != null)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                await GopDuLieuKhachLe(user);
            }

            TempData["ThanhCong"] = "Đăng ký thành công. Chào mừng bạn đến với ShopBanHang.";
            return RedirectToAction("Index", "Home");
        }

        // ===== Đăng xuất =====

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied() => View();

        // ===== Quên mật khẩu =====

        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email.Trim());

            // Luôn báo cùng một câu dù email có tồn tại hay không, để người ngoài
            // không dùng trang này mà dò xem email nào đã đăng ký.
            if (user == null || !user.IsActive)
            {
                TempData["ThanhCong"] = "Nếu email tồn tại trong hệ thống, hướng dẫn đặt lại mật khẩu đã được gửi.";
                return RedirectToAction(nameof(Login));
            }

            string token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Hệ thống thật sẽ gửi liên kết này qua email. Bài tập lớn chưa cấu hình
            // máy chủ thư nên hiện thẳng liên kết ra màn hình để còn thử được.
            vm.LienKetDatLai = Url.Action(nameof(ResetPassword), "Account",
                new { email = user.Email, token }, Request.Scheme);

            return View(vm);
        }

        [HttpGet]
        public IActionResult ResetPassword(string? email, string? token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                TempData["Loi"] = "Liên kết đặt lại mật khẩu không hợp lệ.";
                return RedirectToAction(nameof(Login));
            }

            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email.Trim());
            if (user == null)
            {
                // Không lộ chuyện email không tồn tại
                TempData["ThanhCong"] = "Đặt lại mật khẩu thành công. Mời bạn đăng nhập.";
                return RedirectToAction(nameof(Login));
            }

            var kq = await _userManager.ResetPasswordAsync(user, vm.Token, vm.Password);
            if (!kq.Succeeded)
            {
                ModelState.AddModelError(string.Empty,
                    "Liên kết đã hết hạn hoặc không hợp lệ. Hãy yêu cầu lại.");
                return View(vm);
            }

            // Đặt lại mật khẩu xong thì bỏ luôn trạng thái khoá tạm nếu có
            await _userManager.ResetAccessFailedCountAsync(user);
            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["ThanhCong"] = "Đặt lại mật khẩu thành công. Mời bạn đăng nhập.";
            return RedirectToAction(nameof(Login));
        }

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
                Email = user.Email ?? "",
                FullName = user.FullName,
                Phone = user.PhoneNumber,
                Address = user.Address,
                RoleName = await _userService.LayTenVaiTroAsync(user),
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

        // ===== Sổ địa chỉ giao hàng =====

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Addresses()
        {
            ViewData["Title"] = "Sổ địa chỉ";
            return View(await _addressService.LayCuaUserAsync(LayIdDangNhap()));
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddressCreate()
        {
            ViewData["Title"] = "Thêm địa chỉ";
            return View("AddressForm", new Address());
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> AddressEdit(int id)
        {
            var dc = await _addressService.LayTheoIdAsync(id, LayIdDangNhap());
            if (dc == null)
            {
                TempData["Loi"] = "Không tìm thấy địa chỉ.";
                return RedirectToAction(nameof(Addresses));
            }

            ViewData["Title"] = "Sửa địa chỉ";
            return View("AddressForm", dc);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressSave(Address dc)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = dc.Id == 0 ? "Thêm địa chỉ" : "Sửa địa chỉ";
                return View("AddressForm", dc);
            }

            var (ok, loi) = await _addressService.LuuAsync(dc, LayIdDangNhap());
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, loi!);
                ViewData["Title"] = dc.Id == 0 ? "Thêm địa chỉ" : "Sửa địa chỉ";
                return View("AddressForm", dc);
            }

            TempData["ThanhCong"] = dc.Id == 0 ? "Đã thêm địa chỉ." : "Đã cập nhật địa chỉ.";
            return RedirectToAction(nameof(Addresses));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressSetDefault(int id)
        {
            var (ok, loi) = await _addressService.DatMacDinhAsync(id, LayIdDangNhap());
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã đặt làm địa chỉ mặc định.";
            return RedirectToAction(nameof(Addresses));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddressDelete(int id)
        {
            var (ok, loi) = await _addressService.XoaAsync(id, LayIdDangNhap());
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã xoá địa chỉ.";
            return RedirectToAction(nameof(Addresses));
        }

        // ===== Hàm dùng chung =====

        /// <summary>
        /// Gộp dữ liệu khách lẻ vào tài khoản vừa đăng nhập.
        ///
        /// Đây là chỗ duy nhất làm việc đó, gọi từ cả Login và Register.
        /// Giỏ hàng nằm trong Session, đoạn chat neo bằng cookie - ASP.NET Core
        /// không xoay session id khi đăng nhập nên cả hai đều còn nguyên ở đây.
        /// </summary>
        private Task GopDuLieuKhachLe(User user)
        {
            // GIAI ĐOẠN SAU: gộp giỏ hàng trong Session vào giỏ của tài khoản
            // GIAI ĐOẠN SAU: gán UserId cho ChatConversation đang neo bằng cookie
            return Task.CompletedTask;
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
