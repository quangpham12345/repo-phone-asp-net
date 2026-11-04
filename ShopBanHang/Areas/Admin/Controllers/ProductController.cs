using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Quản lý sản phẩm: danh sách có lọc và phân trang, thêm, sửa,
    /// ẩn hiện, bật tắt nổi bật. Ảnh tải lên được từ máy.
    ///
    /// Khác phía khách ở chỗ danh sách này thấy cả hàng đã ẩn.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class ProductController : Controller
    {
        private const int CoTrang = 10;

        private readonly IProductService _service;
        private readonly IProductVariantService _variants;
        private readonly IWebHostEnvironment _env;

        public ProductController(IProductService service, IProductVariantService variants, IWebHostEnvironment env)
        {
            _service = service;
            _variants = variants;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ProductFilter loc, int page = 1)
        {
            DatTieuDe("Sản phẩm");

            loc.ChiHangDangBan = false;   // admin xem được cả hàng đã ẩn

            return View(new ProductListViewModel
            {
                Loc = loc,
                KetQua = await _service.LayDanhSachAsync(loc, page, CoTrang),
                CacDanhMuc = await _service.LayDanhMucAsync(),
                CacThuongHieu = await _service.LayThuongHieuAsync(),
                SanPhamCanhBao = (await _service.LayCanhBaoTonKhoAsync())
                        .Select(x => x.SanPhamId).ToHashSet()
            });
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            DatTieuDe("Thêm sản phẩm");
            return View("Form", await NapDanhMuc(new ProductFormViewModel { IsActive = true }));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _service.LayTheoIdAsync(id);
            if (p == null)
            {
                TempData["Loi"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction(nameof(Index));
            }

            DatTieuDe("Sửa sản phẩm");

            return View("Form", await NapDanhMuc(new ProductFormViewModel
            {
                Id = p.Id,
                Name = p.Name,
                ImageUrl = p.ImageUrl,
                GalleryUrls = p.GalleryUrls,
                Description = p.Description,
                Ram = p.Ram,
                Os = p.Os,
                Screen = p.Screen,
                Chip = p.Chip,
                Battery = p.Battery,
                Camera = p.Camera,
                CategoryId = p.CategoryId,
                BrandId = p.BrandId,
                IsFeatured = p.IsFeatured,
                IsActive = p.IsActive,
                SoldCount = p.SoldCount,
                SoBienThe = await _variants.DemAsync(p.Id)
            }));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ProductFormViewModel vm, List<IFormFile>? anhTaiLen)
        {
            // Kiểm tra form trước, chưa đụng đến đĩa nếu form sai
            if (!ModelState.IsValid) return await VeLaiForm(vm);

            var (cacAnhMoi, loiAnh) = await ImageStorage.LuuNhieuAsync(anhTaiLen, _env, "products");
            if (loiAnh != null)
            {
                ModelState.AddModelError(nameof(vm.ImageUrl), loiAnh);
                return await VeLaiForm(vm);
            }
            vm.AnhTaiLenMoi = cacAnhMoi;

            // Phải nhớ trước khi lưu: lưu thêm mới xong thì service gán vm.Id nên LaThemMoi sẽ thành false
            bool laThemMoi = vm.LaThemMoi;

            var (ok, loi) = await _service.LuuAsync(vm);
            if (!ok)
            {
                // Lưu thất bại thì ảnh vừa tải lên không sản phẩm nào dùng, xoá đi cho sạch
                foreach (var u in cacAnhMoi) ImageStorage.Xoa(u, _env);
                ModelState.AddModelError(string.Empty, loi!);
                return await VeLaiForm(vm);
            }

            // Lưu thành công mới xoá file ảnh bị gỡ; lỡ lưu lỗi thì ảnh cũ vẫn còn nguyên.
            // Chỉ xoá những ảnh thật sự thuộc sản phẩm này.
            var anhCu = ProductGallery.Tach(vm.GalleryUrls);
            if (!string.IsNullOrWhiteSpace(vm.ImageUrl)) anhCu.Add(vm.ImageUrl.Trim());
            var anhBiGo = vm.AnhXoa
                .Where(x => anhCu.Contains(x.Trim(), StringComparer.OrdinalIgnoreCase))
                .ToList();
            foreach (var u in anhBiGo) ImageStorage.Xoa(u, _env);

            // Biến thể: gỡ ảnh riêng đang trỏ tới ảnh vừa xoá để biến thể quay về dùng ảnh chính
            if (!laThemMoi)
                await _variants.GoAnhAsync(vm.Id, anhBiGo);

            if (laThemMoi)
            {
                // Giá và tồn kho nằm ở biến thể, nên sản phẩm mới chưa có giá cho đến khi có biến thể đầu tiên
                TempData["ThanhCong"] = "Đã thêm sản phẩm. Hãy thêm biến thể đầu tiên (dung lượng, màu, giá, tồn kho) để sản phẩm bán được.";
                return RedirectToAction("Create", "ProductVariant", new { productId = vm.Id });
            }

            TempData["ThanhCong"] = "Đã cập nhật sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var (ok, loi) = await _service.DoiTrangThaiAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã đổi trạng thái sản phẩm.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            var (ok, loi) = await _service.DoiNoiBatAsync(id);
            if (!ok) TempData["Loi"] = loi; else TempData["ThanhCong"] = "Đã đổi cờ nổi bật.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ProductFormViewModel> NapDanhMuc(ProductFormViewModel vm)
        {
            vm.CacDanhMuc = await _service.LayDanhMucAsync();
            vm.CacThuongHieu = await _service.LayThuongHieuAsync();
            return vm;
        }

        private async Task<IActionResult> VeLaiForm(ProductFormViewModel vm)
        {
            DatTieuDe(vm.LaThemMoi ? "Thêm sản phẩm" : "Sửa sản phẩm");
            return View("Form", await NapDanhMuc(vm));
        }

        private void DatTieuDe(string tieuDe)
        {
            ViewData["Title"] = tieuDe;
            ViewData["ActiveMenu"] = "san-pham";
        }
    }
}
