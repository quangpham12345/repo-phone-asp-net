using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Quản lý banner trang chủ: thêm, sửa, xoá, bật tắt, sắp thứ tự.
    /// Ảnh có thể tải lên từ máy, hoặc gõ thẳng đường dẫn ảnh có sẵn.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class BannerController : Controller
    {
        private readonly IBannerService _bannerService;
        private readonly IWebHostEnvironment _env;

        public BannerController(IBannerService bannerService, IWebHostEnvironment env)
        {
            _bannerService = bannerService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            DatTieuDe("Banner trang chủ");
            return View(await _bannerService.LayTatCaAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            DatTieuDe("Thêm banner");
            return View("Form", new Banner { IsActive = true, SortOrder = 0 });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var bn = await _bannerService.LayTheoIdAsync(id);
            if (bn == null)
            {
                TempData["Loi"] = "Không tìm thấy banner.";
                return RedirectToAction(nameof(Index));
            }

            DatTieuDe("Sửa banner");
            return View("Form", bn);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Banner banner, IFormFile? anhTaiLen)
        {
            // Ảnh tải lên được ưu tiên hơn đường dẫn gõ tay
            var (duongDan, loiAnh) = await ImageStorage.LuuAsync(anhTaiLen, _env, "banners");
            if (loiAnh != null)
            {
                ModelState.AddModelError(nameof(banner.ImageUrl), loiAnh);
            }
            else if (duongDan != null)
            {
                banner.ImageUrl = duongDan;
                ModelState.Remove(nameof(banner.ImageUrl));   // bỏ lỗi "bắt buộc" của giá trị cũ
            }

            if (!ModelState.IsValid)
            {
                DatTieuDe(banner.Id == 0 ? "Thêm banner" : "Sửa banner");
                return View("Form", banner);
            }

            var (ok, loi) = await _bannerService.LuuAsync(banner);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, loi!);
                DatTieuDe(banner.Id == 0 ? "Thêm banner" : "Sửa banner");
                return View("Form", banner);
            }

            TempData["ThanhCong"] = banner.Id == 0 ? "Đã thêm banner." : "Đã cập nhật banner.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var (ok, loi) = await _bannerService.DoiTrangThaiAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã đổi trạng thái banner.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (ok, loi) = await _bannerService.XoaAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã xoá banner.";
            return RedirectToAction(nameof(Index));
        }

        private void DatTieuDe(string tieuDe)
        {
            ViewData["Title"] = tieuDe;
            ViewData["ActiveMenu"] = "banner";
        }
    }
}
