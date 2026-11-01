using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>Thêm, sửa, xoá, bật tắt tin tức.</summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class NewsController : Controller
    {
        private const int CoTrang = 10;

        private readonly INewsService _service;
        private readonly IWebHostEnvironment _env;

        public NewsController(INewsService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? tuKhoa, bool? active, int page = 1)
        {
            DatTieuDe("Tin tức");

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.Active = active;

            return View(await _service.LayDanhSachAsync(tuKhoa, active, page, CoTrang));
        }

        [HttpGet]
        public IActionResult Create()
        {
            DatTieuDe("Thêm bài viết");
            return View("Form", new News { IsActive = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var n = await _service.LayTheoIdAsync(id);
            if (n == null)
            {
                TempData["Loi"] = "Không tìm thấy bài viết.";
                return RedirectToAction(nameof(Index));
            }

            DatTieuDe("Sửa bài viết");
            return View("Form", n);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(News n, IFormFile? anhTaiLen)
        {
            ModelState.Remove(nameof(News.Slug));

            var (duongDan, loiAnh) = await ImageStorage.LuuAsync(anhTaiLen, _env, "news");
            if (loiAnh != null) ModelState.AddModelError(nameof(News.ImageUrl), loiAnh);
            else if (duongDan != null) n.ImageUrl = duongDan;

            if (!ModelState.IsValid)
            {
                DatTieuDe(n.Id == 0 ? "Thêm bài viết" : "Sửa bài viết");
                return View("Form", n);
            }

            var (ok, loi) = await _service.LuuAsync(n);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, loi!);
                DatTieuDe(n.Id == 0 ? "Thêm bài viết" : "Sửa bài viết");
                return View("Form", n);
            }

            TempData["ThanhCong"] = n.Id == 0 ? "Đã thêm bài viết." : "Đã cập nhật bài viết.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var (ok, loi) = await _service.DoiTrangThaiAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã đổi trạng thái bài viết.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (ok, loi) = await _service.XoaAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã xoá bài viết.";
            return RedirectToAction(nameof(Index));
        }

        private void DatTieuDe(string tieuDe)
        {
            ViewData["Title"] = tieuDe;
            ViewData["ActiveMenu"] = "tin-tuc";
        }
    }
}
