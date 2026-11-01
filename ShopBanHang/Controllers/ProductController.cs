using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client.Extensions.Msal;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers
{
    /// <summary>
    /// Danh sách và chi tiết sản phẩm phía khách.
    /// </summary>
    public class ProductController : Controller
    {
        private const int CoTrang = 8;

        private readonly IProductService _productService;

        public ProductController(IProductService productService) => _productService = productService;

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ProductFilter loc, int page = 1)
        {
            // Phía khách chỉ thấy hàng đang bán, kể cả khi query string gửi khác
            loc.ChiHangDangBan = true;
            var (rams, storages, oss) = await _productService.LayGiaTriLocAsync();

            var vm = new ProductListViewModel
            {
                Loc = loc,
                KetQua = await _productService.LayDanhSachAsync(loc, page, CoTrang),
                CacDanhMuc = await _productService.LayDanhMucAsync(),
                CacThuongHieu = await _productService.LayThuongHieuAsync(),
                CacRam = rams,
                CacStorage = storages,
                CacOs = oss
            };

            // Tiêu đề đổi theo danh mục hoặc từ khoá đang xem
            if (!string.IsNullOrWhiteSpace(loc.DanhMuc))
            {
                var dm = vm.CacDanhMuc.FirstOrDefault(c => c.Slug == loc.DanhMuc);
                if (dm != null) vm.TieuDe = dm.Name;
            }
            else if (!string.IsNullOrWhiteSpace(loc.TuKhoa))
            {
                vm.TieuDe = $"Kết quả cho \"{loc.TuKhoa}\"";
            }

            ViewData["Title"] = vm.TieuDe;
            return View(vm);
        }

        // Route riêng để địa chỉ đẹp: /Product/Detail/iphone-17-pro-max-256gb
        // Không có dòng này thì đoạn thứ ba của URL rơi vào {id?} của route mặc định,
        // tham số slug sẽ null và trang luôn trả 404.
        [HttpGet]
        [Route("Product/Detail/{slug}")]
        public async Task<IActionResult> Detail(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return NotFound();
            var sp = await _productService.LayTheoSlugAsync(slug);
            if (sp == null) return NotFound();
            ViewData["Title"] = sp.Name;
            return View(new ProductDetailViewModel
            {
                SanPham = sp,
                LienQuan = await _productService.LayLienQuanAsync(sp, 4),
                PhienBanKhac = await _productService.LayPhienBanKhacAsync(sp, 6)
            });
        }
        [HttpGet]
        public async Task<IActionResult> GoiY(string q)
        {
            var list = await _productService.GoiYAsync(q ?? "", 8);
            return Json(list);
        }

        [HttpGet]
        public async Task<IActionResult> Compare([FromQuery] int[] ids)
        {
            if (ids == null || ids.Length < 2)
            {
                TempData["Loi"] = "Hãy chọn ít nhất 2 sản phẩm để so sánh.";
                return RedirectToAction(nameof(Index));
            }
            var list = await _productService.LayTheoIdsAsync(ids.Take(3));
            if (list.Count < 2)
            {
                TempData["Loi"] = "Không tìm đủ sản phẩm để so sánh.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["Title"] = "So sánh sản phẩm";
            return View(new ProductCompareViewModel { SanPham = list });
        }
    }
}
