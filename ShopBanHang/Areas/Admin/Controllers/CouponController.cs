using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Quản lý mã giảm giá: danh sách có phân trang và lọc theo trạng thái,
    /// thêm, sửa, bật tắt, xoá (chỉ xoá được mã chưa ai dùng).
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class CouponController : Controller
    {
        private const int CoTrang = 10;

        private readonly ICouponService _couponService;

        public CouponController(ICouponService couponService) => _couponService = couponService;

        [HttpGet]
        public async Task<IActionResult> Index(string? tuKhoa, CouponStatus? trangThai, int page = 1)
        {
            ViewData["Title"] = "Mã giảm giá";
            ViewData["ActiveMenu"] = "ma-giam-gia";

            var vm = new CouponListViewModel
            {
                TuKhoa = tuKhoa,
                TrangThai = trangThai,
                KetQua = await _couponService.LayDanhSachAsync(tuKhoa, trangThai, page, CoTrang)
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm mã giảm giá";
            ViewData["ActiveMenu"] = "ma-giam-gia";

            return View("Form", new CouponFormViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _couponService.LayTheoIdAsync(id);
            if (c == null)
            {
                TempData["Loi"] = "Không tìm thấy mã giảm giá.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["Title"] = "Sửa mã giảm giá";
            ViewData["ActiveMenu"] = "ma-giam-gia";

            return View("Form", new CouponFormViewModel
            {
                Id = c.Id,
                Code = c.Code,
                Description = c.Description,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MaxDiscount = c.MaxDiscount,
                MinOrderValue = c.MinOrderValue,
                UsageLimit = c.UsageLimit,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                IsActive = c.IsActive,
                UsedCount = c.UsedCount
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(CouponFormViewModel vm)
        {
            if (!ModelState.IsValid)
                return VeLaiForm(vm);

            var (ok, loi) = await _couponService.LuuAsync(new Coupon
            {
                Id = vm.Id,
                Code = vm.Code,
                Description = vm.Description,
                DiscountType = vm.DiscountType,
                DiscountValue = vm.DiscountValue,
                MaxDiscount = vm.MaxDiscount,
                MinOrderValue = vm.MinOrderValue,
                UsageLimit = vm.UsageLimit,
                StartDate = vm.StartDate,
                EndDate = vm.EndDate,
                IsActive = vm.IsActive
            });

            if (!ok)
            {
                // Giữ nguyên những gì vừa gõ, không bắt nhập lại
                ModelState.AddModelError(string.Empty, loi!);
                return VeLaiForm(vm);
            }

            TempData["ThanhCong"] = vm.LaThemMoi ? "Đã thêm mã giảm giá." : "Đã cập nhật mã giảm giá.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var (ok, loi) = await _couponService.DoiTrangThaiAsync(id);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã đổi trạng thái mã giảm giá.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (ok, loi) = await _couponService.XoaAsync(id);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã xoá mã giảm giá.";

            return RedirectToAction(nameof(Index));
        }

        private IActionResult VeLaiForm(CouponFormViewModel vm)
        {
            ViewData["Title"] = vm.LaThemMoi ? "Thêm mã giảm giá" : "Sửa mã giảm giá";
            ViewData["ActiveMenu"] = "ma-giam-gia";
            return View("Form", vm);
        }
    }
}