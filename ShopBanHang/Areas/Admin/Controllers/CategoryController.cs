using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Entities;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>Thêm, sửa, xoá, bật tắt danh mục sản phẩm.</summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _service;

        public CategoryController(ICategoryService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            DatTieuDe("Danh mục");
            return View(await _service.LayTatCaAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            DatTieuDe("Thêm danh mục");
            return View("Form", new Category { IsActive = true });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var c = await _service.LayTheoIdAsync(id);
            if (c == null)
            {
                TempData["Loi"] = "Không tìm thấy danh mục.";
                return RedirectToAction(nameof(Index));
            }

            DatTieuDe("Sửa danh mục");
            return View("Form", c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Category c)
        {
            // Slug do service sinh từ tên nên không bắt người dùng nhập
            ModelState.Remove(nameof(Category.Slug));

            if (!ModelState.IsValid)
            {
                DatTieuDe(c.Id == 0 ? "Thêm danh mục" : "Sửa danh mục");
                return View("Form", c);
            }

            var (ok, loi) = await _service.LuuAsync(c);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, loi!);
                DatTieuDe(c.Id == 0 ? "Thêm danh mục" : "Sửa danh mục");
                return View("Form", c);
            }

            TempData["ThanhCong"] = c.Id == 0 ? "Đã thêm danh mục." : "Đã cập nhật danh mục.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var (ok, loi) = await _service.DoiTrangThaiAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã đổi trạng thái danh mục.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (ok, loi) = await _service.XoaAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã xoá danh mục.";
            return RedirectToAction(nameof(Index));
        }

        private void DatTieuDe(string tieuDe)
        {
            ViewData["Title"] = tieuDe;
            ViewData["ActiveMenu"] = "danh-muc";
        }
    }
}
