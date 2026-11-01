using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>Thêm, sửa, xoá, bật tắt thương hiệu. Logo tải lên được từ máy.</summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class BrandController : Controller
    {
        private readonly IBrandService _service;
        private readonly IWebHostEnvironment _env;

        public BrandController(IBrandService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            DatTieuDe("Thương hiệu");
            return View(await _service.LayTatCaAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            DatTieuDe("Thêm thương hiệu");
            return View("Form", new Brand { IsActive = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var b = await _service.LayTheoIdAsync(id);
            if (b == null)
            {
                TempData["Loi"] = "Không tìm thấy thương hiệu.";
                return RedirectToAction(nameof(Index));
            }

            DatTieuDe("Sửa thương hiệu");
            return View("Form", b);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Brand b, IFormFile? anhTaiLen)
        {
            ModelState.Remove(nameof(Brand.Slug));

            if (!ModelState.IsValid)
            {
                DatTieuDe(b.Id == 0 ? "Thêm thương hiệu" : "Sửa thương hiệu");
                return View("Form", b);
            }

            var (duongDan, loiAnh) = await ImageStorage.LuuAsync(anhTaiLen, _env, "brands");
            if (loiAnh != null) ModelState.AddModelError(nameof(Brand.LogoUrl), loiAnh);
            else if (duongDan != null) b.LogoUrl = duongDan;           

            var (ok, loi) = await _service.LuuAsync(b);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, loi!);
                DatTieuDe(b.Id == 0 ? "Thêm thương hiệu" : "Sửa thương hiệu");
                return View("Form", b);
            }

            TempData["ThanhCong"] = b.Id == 0 ? "Đã thêm thương hiệu." : "Đã cập nhật thương hiệu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var (ok, loi) = await _service.DoiTrangThaiAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã đổi trạng thái thương hiệu.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (ok, loi) = await _service.XoaAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã xoá thương hiệu.";
            return RedirectToAction(nameof(Index));
        }

        private void DatTieuDe(string tieuDe)
        {
            ViewData["Title"] = tieuDe;
            ViewData["ActiveMenu"] = "thuong-hieu";
        }
    }
}
