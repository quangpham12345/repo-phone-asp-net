using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers
{
    /// <summary>
    /// Giỏ hàng theo biến thể.
    ///
    /// Chưa đăng nhập vẫn thêm được vào giỏ: giỏ nằm trong Session, đăng nhập thì
    /// AccountController gộp sang bảng CartItems. Đặt hàng cũng không bắt đăng nhập.
    ///
    /// Thêm vào giỏ trả JSON nếu gọi bằng AJAX (nút trên trang chi tiết), còn lại thì redirect.
    /// </summary>
    public class CartController : Controller
    {
        private readonly ICartService _cart;

        public CartController(ICartService cart) => _cart = cart;

        private int UserId => User.LayId();
        private bool DaDangNhap => UserId > 0;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Giỏ hàng";

            var vm = DaDangNhap
                ? await _cart.LayGioAsync(UserId)
                : await _cart.LayGioKhachAsync(GioKhachLe.Lay(HttpContext.Session));

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int variantId, int soLuong = 1, bool muaNgay = false, string? returnUrl = null)
        {
            bool ok;
            string? loi;
            int soLuongGio;

            if (DaDangNhap)
            {
                (ok, loi) = await _cart.ThemAsync(UserId, variantId, soLuong);
                soLuongGio = await _cart.DemAsync(UserId);
            }
            else
            {
                var gio = GioKhachLe.Lay(HttpContext.Session);
                (ok, loi) = await _cart.ThemKhachAsync(gio, variantId, soLuong);
                if (ok) GioKhachLe.Luu(HttpContext.Session, gio);
                soLuongGio = gio.Sum(x => x.Quantity);
            }

            if (LaAjax())
                return Json(new CartAjaxResult(ok, ok ? "Đã thêm vào giỏ hàng." : loi!, soLuongGio));

            if (!ok)
            {
                TempData["Loi"] = loi;
                return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl!) : RedirectToAction(nameof(Index));
            }

            // "Mua ngay" đi thẳng sang bước đặt hàng (khách vãng lai cũng đặt được, không cần đăng nhập)
            if (muaNgay) return RedirectToAction("Index", "Checkout");

            TempData["ThanhCong"] = "Đã thêm vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        /// <summary>id là CartItemId (đã đăng nhập) hoặc VariantId (giỏ khách lẻ), đúng như CartLineViewModel.CartItemId.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, int soLuong)
        {
            (bool ok, string? loi) kq;

            if (DaDangNhap)
            {
                kq = await _cart.CapNhatAsync(UserId, id, soLuong);
            }
            else
            {
                var gio = GioKhachLe.Lay(HttpContext.Session);
                kq = await _cart.CapNhatKhachAsync(gio, id, soLuong);
                if (kq.ok) GioKhachLe.Luu(HttpContext.Session, gio);
            }

            if (!kq.ok) TempData["Loi"] = kq.loi;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            (bool ok, string? loi) kq;

            if (DaDangNhap)
            {
                kq = await _cart.XoaAsync(UserId, id);
            }
            else
            {
                var gio = GioKhachLe.Lay(HttpContext.Session);
                kq = await _cart.CapNhatKhachAsync(gio, id, 0);
                if (kq.ok) GioKhachLe.Luu(HttpContext.Session, gio);
            }

            if (!kq.ok) TempData["Loi"] = kq.loi;
            else TempData["ThanhCong"] = "Đã xoá sản phẩm khỏi giỏ.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            if (DaDangNhap) await _cart.XoaHetAsync(UserId);
            else GioKhachLe.Xoa(HttpContext.Session);

            TempData["ThanhCong"] = "Đã làm trống giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        private bool LaAjax() => Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
}
