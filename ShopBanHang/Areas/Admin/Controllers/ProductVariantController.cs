using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Quản lý biến thể (dung lượng × màu) của một sản phẩm: danh sách, thêm, sửa, bật tắt, xoá.
    /// Mọi trang đều đi kèm productId; sau khi lưu quay về danh sách biến thể của đúng sản phẩm đó.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class ProductVariantController : Controller
    {
        private readonly IProductVariantService _variants;
        private readonly IProductService _products;

        public ProductVariantController(IProductVariantService variants, IProductService products)
        {
            _variants = variants;
            _products = products;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int productId)
        {
            var sp = await _products.LayTheoIdAsync(productId);
            if (sp == null) return KhongThaySanPham();

            DatTieuDe("Biến thể sản phẩm");

            return View(new ProductVariantListViewModel
            {
                SanPham = sp,
                CacBienThe = await _variants.LayTheoSanPhamAsync(productId)
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create(int productId)
        {
            var sp = await _products.LayTheoIdAsync(productId);
            if (sp == null) return KhongThaySanPham();

            var vm = new ProductVariantFormViewModel
            {
                ProductId = productId,
                TenSanPham = sp.Name,
                CacAnh = sp.TatCaAnh
            };

            // Đã có biến thể thì điền sẵn giá đang hiển thị của sản phẩm, vì thêm màu mới thường cùng giá
            if (sp.HasVariants)
            {
                vm.Price = sp.DisplayPrice;
                vm.OldPrice = sp.DisplayOldPrice;
            }

            DatTieuDe("Thêm biến thể");
            return View("Form", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var v = await _variants.LayTheoIdAsync(id);
            if (v == null)
            {
                TempData["Loi"] = "Không tìm thấy biến thể.";
                return RedirectToAction("Index", "Product");
            }

            var sp = await _products.LayTheoIdAsync(v.ProductId);
            if (sp == null) return KhongThaySanPham();

            DatTieuDe("Sửa biến thể");
            return View("Form", new ProductVariantFormViewModel
            {
                Id = v.Id,
                ProductId = v.ProductId,
                Storage = v.Storage,
                Color = v.Color,
                Price = v.Price,
                OldPrice = v.OldPrice,
                Stock = v.Stock,
                ImageUrl = v.ImageUrl,
                IsActive = v.IsActive,
                TenSanPham = sp.Name,
                CacAnh = sp.TatCaAnh
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ProductVariantFormViewModel vm)
        {
            var sp = await _products.LayTheoIdAsync(vm.ProductId);
            if (sp == null) return KhongThaySanPham();

            // Ảnh riêng của biến thể phải là một trong các ảnh của sản phẩm
            if (!string.IsNullOrWhiteSpace(vm.ImageUrl)
                && !sp.TatCaAnh.Contains(vm.ImageUrl.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(vm.ImageUrl), "Ảnh này không thuộc sản phẩm.");
            }

            if (!ModelState.IsValid) return VeLaiForm(vm, sp);

            var (ok, loi) = await _variants.LuuAsync(new ProductVariant
            {
                Id = vm.Id,
                ProductId = vm.ProductId,
                Storage = vm.Storage,
                Color = vm.Color,
                Price = vm.Price,
                OldPrice = vm.OldPrice,
                Stock = vm.Stock,
                ImageUrl = vm.ImageUrl,
                IsActive = vm.IsActive
            });

            if (!ok)
            {
                // Giữ nguyên những gì vừa gõ, không bắt nhập lại
                ModelState.AddModelError(string.Empty, loi!);
                return VeLaiForm(vm, sp);
            }

            TempData["ThanhCong"] = vm.LaThemMoi ? "Đã thêm biến thể." : "Đã cập nhật biến thể.";
            return RedirectToAction(nameof(Index), new { productId = vm.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id, int productId)
        {
            var (ok, loi) = await _variants.DoiTrangThaiAsync(id);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã đổi trạng thái biến thể.";

            return RedirectToAction(nameof(Index), new { productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int productId)
        {
            var (ok, loi) = await _variants.XoaAsync(id);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã xoá biến thể.";

            return RedirectToAction(nameof(Index), new { productId });
        }

        private IActionResult VeLaiForm(ProductVariantFormViewModel vm, Product sp)
        {
            vm.TenSanPham = sp.Name;
            vm.CacAnh = sp.TatCaAnh;
            DatTieuDe(vm.LaThemMoi ? "Thêm biến thể" : "Sửa biến thể");
            return View("Form", vm);
        }

        private IActionResult KhongThaySanPham()
        {
            TempData["Loi"] = "Không tìm thấy sản phẩm.";
            return RedirectToAction("Index", "Product");
        }

        private void DatTieuDe(string tieuDe)
        {
            ViewData["Title"] = tieuDe;
            ViewData["ActiveMenu"] = "san-pham";
        }
    }
}