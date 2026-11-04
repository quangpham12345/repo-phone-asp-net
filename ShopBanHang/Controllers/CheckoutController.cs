using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers
{
    /// <summary>
    /// Đặt hàng: chọn/nhập địa chỉ, áp mã giảm giá, chọn COD hoặc chuyển khoản QR, tạo đơn.
    ///
    /// KHÔNG bắt đăng nhập: khách vãng lai đặt được với giỏ trong Session, tự nhập họ tên,
    /// số điện thoại, địa chỉ, email; đơn lưu với UserId = null. Khách đã đăng nhập thì
    /// được chọn từ sổ địa chỉ và theo dõi đơn ở "Đơn hàng của tôi".
    /// </summary>
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

        private int UserId => User.LayId();

        /// <summary>Giỏ Session; chỉ có ý nghĩa với khách vãng lai, đã đăng nhập thì giỏ nằm trong CSDL.</summary>
        private List<GuestCartLine> GioKhach => UserId > 0 ? new List<GuestCartLine>() : GioKhachLe.Lay(HttpContext.Session);

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var vm = await _checkout.ChuanBiAsync(UserId, GioKhach);

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
            => Json(await _checkout.ApMaAsync(UserId, GioKhach, code));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder([Bind(Prefix = "Form")] CheckoutForm form)
        {
            ViewData["Title"] = "Đặt hàng";
            var gioKhach = GioKhach;

            // Khách vãng lai không có sổ địa chỉ: luôn đi nhánh nhập mới để Validate() bắt đủ các ô
            if (UserId == 0 && form.AddressId.HasValue)
            {
                form.AddressId = null;
                ModelState.Clear();
                TryValidateModel(form, "Form");
            }

            if (!ModelState.IsValid)
                return View(nameof(Index), await _checkout.ChuanBiAsync(UserId, gioKhach, form));

            var kq = await _checkout.DatHangAsync(UserId, gioKhach, form);
            if (!kq.Ok)
            {
                ModelState.AddModelError(string.Empty, kq.Loi!);
                var vm = await _checkout.ChuanBiAsync(UserId, gioKhach, form);

                // Lỗi do giỏ (hết hàng giữa chừng...) thì đưa về giỏ để khách sửa
                if (vm.Gio.TrongRong || vm.Gio.CoVanDe)
                {
                    TempData["Loi"] = kq.Loi;
                    return RedirectToAction("Index", "Cart");
                }
                return View(nameof(Index), vm);
            }

            string code = kq.Don!.OrderCode;
            if (UserId == 0)
            {
                // Đặt xong mới xoá giỏ Session, và cấp quyền cho phiên này xem đơn vừa đặt
                GioKhachLe.Xoa(HttpContext.Session);
                DonKhachLe.ChoPhep(HttpContext.Session, code);
            }

            return RedirectToAction(nameof(Success), new { code });
        }

        /// <summary>Trang cảm ơn sau khi đặt. Đơn chuyển khoản thì hiện mã QR.</summary>
        [HttpGet]
        public async Task<IActionResult> Success(string code)
        {
            bool laKhachLe = UserId == 0;

            var don = laKhachLe
                ? (DonKhachLe.DuocXem(HttpContext.Session, code) ? await _orders.LayTheoMaAsync(code) : null)
                : await _orders.LayCuaToiTheoMaAsync(UserId, code);

            if (don == null)
                return laKhachLe ? RedirectToAction("Lookup", "Order") : RedirectToAction("Index", "Order");

            ViewData["Title"] = "Đặt hàng thành công";

            var vm = new MyOrderDetailViewModel { Don = don, LaKhachLe = laKhachLe };
            if (_payment.CanThanhToanQr(don))
            {
                vm.QrUrl = _payment.TaoQrUrl(don);
                vm.ChuyenKhoan = _payment.LayThongTinChuyenKhoan(don);
            }
            return View(vm);
        }
    }
}
