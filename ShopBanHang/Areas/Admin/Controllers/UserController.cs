using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Quản lý tài khoản: danh sách có phân trang và lọc, thêm, sửa,
    /// bật tắt hoạt động, đặt lại mật khẩu.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class UserController : Controller
    {
        private const int CoTrang = 10;

        private readonly IUserService _userService;

        public UserController(IUserService userService) => _userService = userService;

        [HttpGet]
        public async Task<IActionResult> Index(string? tuKhoa, int? roleId, bool? isActive, int page = 1)
        {
            ViewData["Title"] = "Tài khoản";
            ViewData["ActiveMenu"] = "tai-khoan";

            var vm = new UserListViewModel
            {
                TuKhoa = tuKhoa,
                RoleId = roleId,
                IsActive = isActive,
                CacVaiTro = await _userService.LayVaiTroAsync(),
                KetQua = await _userService.LayDanhSachAsync(tuKhoa, roleId, isActive, page, CoTrang)
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Thêm tài khoản";
            ViewData["ActiveMenu"] = "tai-khoan";

            return View("Form", new UserFormViewModel
            {
                CacVaiTro = await _userService.LayVaiTroAsync()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var u = await _userService.LayTheoIdAsync(id);
            if (u == null)
            {
                TempData["Loi"] = "Không tìm thấy tài khoản.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Sửa tài khoản";
            ViewData["ActiveMenu"] = "tai-khoan";

            return View("Form", new UserFormViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                Address = u.Address,
                RoleId = u.RoleId,
                IsActive = u.IsActive,
                CacVaiTro = await _userService.LayVaiTroAsync()
                // Password để trống: bỏ trống khi lưu là giữ mật khẩu cũ
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(UserFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = vm.LaThemMoi ? "Thêm tài khoản" : "Sửa tài khoản";
                ViewData["ActiveMenu"] = "tai-khoan";
                vm.CacVaiTro = await _userService.LayVaiTroAsync();
                return View("Form", vm);
            }

            var (ok, loi) = await _userService.LuuAsync(vm);
            if (!ok)
            {
                // Giữ nguyên những gì vừa gõ, không bắt nhập lại
                ModelState.AddModelError(string.Empty, loi!);
                ViewData["Title"] = vm.LaThemMoi ? "Thêm tài khoản" : "Sửa tài khoản";
                ViewData["ActiveMenu"] = "tai-khoan";
                vm.CacVaiTro = await _userService.LayVaiTroAsync();
                return View("Form", vm);
            }

            TempData["ThanhCong"] = vm.LaThemMoi ? "Đã thêm tài khoản." : "Đã cập nhật tài khoản.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var (ok, loi) = await _userService.DoiTrangThaiAsync(id, LayIdDangNhap());

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã đổi trạng thái tài khoản.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string matKhauMoi)
        {
            var (ok, loi) = await _userService.DatLaiMatKhauAsync(id, matKhauMoi);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã đặt lại mật khẩu.";

            return RedirectToAction(nameof(Edit), new { id });
        }

        private int LayIdDangNhap()
            => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;
    }
}
