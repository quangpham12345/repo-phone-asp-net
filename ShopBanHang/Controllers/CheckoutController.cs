using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers
{
    /// <summary>Đặt hàng: chọn địa chỉ, áp mã giảm giá, chọn COD hoặc chuyển khoản QR, tạo đơn.</summary>
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICheckoutService _checkout;
        private readonly IOrderService _orders;
        private readonly IPaymentService _payment;

        public CheckoutController(ICheckoutService checkout, IOrderService orders, IPaymentService payment)
        {
            _checkout = checkout;
            _orders = orders;
            _payment = payment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = await _checkout.ChuanBiAsync(User.LayId());

            if (vm.Gio.TrongRong)
            {
                TempData["Loi"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index", "Cart");
            }
            if (vm.Gio.CoVanDe)
            {
                TempData["Loi"] = "Giỏ hàng có sản phẩm hết hàng hoặc vượt tồn kho, vui lòng sửa lại trước khi đặt.";
                return RedirectToAction("Index", "Cart");
            }

            ViewData["Title"] = "Đặt hàng";
            return View(vm);
        }

        /// <summary>Nút "Áp dụng" mã giảm giá, gọi bằng AJAX. Lúc đặt hàng service còn kiểm lại lần nữa.</summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyCoupon(string? code)
            => Json(await _checkout.ApMaAsync(User.LayId(), code));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder([Bind(Prefix = "Form")] CheckoutForm form)
        {
            int userId = User.LayId();
            ViewData["Title"] = "Đặt hàng";

            if (!ModelState.IsValid)
                return View(nameof(Index), await _checkout.ChuanBiAsync(userId, form));

            var kq = await _checkout.DatHangAsync(userId, form);
            if (!kq.Ok)
            {
                ModelState.AddModelError(string.Empty, kq.Loi!);
                var vm = await _checkout.ChuanBiAsync(userId, form);

                // Lỗi do giỏ (hết hàng giữa chừng...) thì đưa về giỏ để khách sửa
                if (vm.Gio.TrongRong || vm.Gio.CoVanDe)
                {
                    TempData["Loi"] = kq.Loi;
                    return RedirectToAction("Index", "Cart");
                }
                return View(nameof(Index), vm);
            }

            return RedirectToAction(nameof(Success), new { code = kq.Don!.OrderCode });
        }

        /// <summary>Trang cảm ơn sau khi đặt. Đơn chuyển khoản thì hiện mã QR.</summary>
        [HttpGet]
        public async Task<IActionResult> Success(string code)
        {
            var don = await _orders.LayCuaToiTheoMaAsync(User.LayId(), code);
            if (don == null) return RedirectToAction("Index", "Order");

            ViewData["Title"] = "Đặt hàng thành công";

            var vm = new MyOrderDetailViewModel { Don = don };
            if (_payment.CanThanhToanQr(don))
            {
                vm.QrUrl = _payment.TaoQrUrl(don);
                vm.ChuyenKhoan = _payment.LayThongTinChuyenKhoan(don);
            }
            return View(vm);
        }
    }
}
