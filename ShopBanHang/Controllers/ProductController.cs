using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using ShopBanHang.Helpers;
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
        private readonly IReviewService _reviewService;
        private readonly IStringLocalizer<SharedResource> _l;

        public ProductController(IProductService productService, IReviewService reviewService,
                                 IStringLocalizer<SharedResource> l)
        {
            _productService = productService;
            _reviewService = reviewService;
            _l = l;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] ProductFilter loc, int page = 1)
        {
            // Phía khách chỉ thấy hàng đang bán, kể cả khi query string gửi khác
            loc.ChiHangDangBan = true;
            var (rams, storages, oss) = await _productService.LayGiaTriLocAsync();

            // SESSION: ghi nhớ từ khoá vừa tìm trong phiên này
            if (!string.IsNullOrWhiteSpace(loc.TuKhoa))
                LichSuTimKiem.Them(HttpContext.Session, loc.TuKhoa);

            var vm = new ProductListViewModel
            {
                Loc = loc,
                KetQua = await _productService.LayDanhSachAsync(loc, page, CoTrang),
                CacDanhMuc = await _productService.LayDanhMucAsync(),
                CacThuongHieu = await _productService.LayThuongHieuAsync(),
                CacRam = rams,
                CacStorage = storages,
                CacOs = oss,
                TuKhoaGanDay = LichSuTimKiem.Lay(HttpContext.Session),
                TieuDe = _l["Tất cả sản phẩm"]
            };

            // Tiêu đề đổi theo danh mục hoặc từ khoá đang xem
            if (!string.IsNullOrWhiteSpace(loc.DanhMuc))
            {
                var dm = vm.CacDanhMuc.FirstOrDefault(c => c.Slug == loc.DanhMuc);
                if (dm != null) vm.TieuDe = dm.Name;
            }
            else if (!string.IsNullOrWhiteSpace(loc.TuKhoa))
            {
                vm.TieuDe = _l["Kết quả cho \"{0}\"", loc.TuKhoa];
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

            // COOKIE: ghi id sản phẩm vào danh sách "đã xem gần đây" (Helpers/CookieDaXem.cs)
            CookieDaXem.Them(HttpContext, sp.Id);

            var (trungBinh, soLuot) = await _reviewService.ThongKeAsync(sp.Id);

            var vm = new ProductDetailViewModel
            {
                SanPham = sp,
                LienQuan = await _productService.LayLienQuanAsync(sp, 5),
                DanhGia = await _reviewService.LayCuaSanPhamAsync(sp.Id),
                DiemTrungBinh = trungBinh,
                SoLuotDanhGia = soLuot
            };

            // Chỉ khách đã đăng nhập mới có chuyện được hay không được đánh giá
            int idToi = LayIdDangNhap();
            if (idToi > 0)
            {
                vm.DuocDanhGia = await _reviewService.DaMuaAsync(sp.Id, idToi);
                vm.DanhGiaCuaToi = await _reviewService.LayCuaToiAsync(sp.Id, idToi);
            }

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> GoiY(string q)
        {
            var list = await _productService.GoiYAsync(q ?? "", 8);
            return Json(list);
        }

        /// <summary>Xoá cookie "đã xem gần đây".</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaDaXem(string? returnUrl = null)
        {
            CookieDaXem.Xoa(HttpContext);
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction(nameof(Index));
        }

        /// <summary>Xoá lịch sử tìm kiếm lưu trong Session.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaLichSuTim()
        {
            LichSuTimKiem.Xoa(HttpContext.Session);
            return RedirectToAction(nameof(Index));
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

        /// <summary>Gửi hoặc sửa đánh giá. Quyền đánh giá do service kiểm lại lần nữa.</summary>
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Review(int productId, string slug, int soSao, string? nhanXet)
        {
            var (ok, loi) = await _reviewService.LuuAsync(productId, LayIdDangNhap(), soSao, nhanXet);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Cảm ơn bạn đã đánh giá sản phẩm.";

            return RedirectToAction(nameof(Detail), new { slug });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReviewDelete(int productId, string slug)
        {
            var (ok, loi) = await _reviewService.XoaCuaToiAsync(productId, LayIdDangNhap());

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã xoá đánh giá của bạn.";

            return RedirectToAction(nameof(Detail), new { slug });
        }

        private int LayIdDangNhap()
            => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;
    }
}
