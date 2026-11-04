using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers
{
    /// <summary>
    /// Đơn hàng phía khách.
    ///
    /// Khách đã đăng nhập: "Đơn hàng của tôi" (Index, Detail, Cancel) — chỉ thấy đơn của chính mình.
    /// Khách vãng lai: tra cứu bằng mã đơn + số điện thoại (Lookup), xem và huỷ ở Guest/GuestCancel.
    /// Hai action Guest chỉ mở được đơn mà phiên hiện tại đã được cấp quyền (Helpers/DonKhachLe),
    /// nên biết mã đơn thôi chưa đủ để xem.
    ///
    /// Trùng tên lớp với Areas/Admin/Controllers/OrderController nhưng khác namespace và
    /// không có [Area], nên route mặc định /Order/... chỉ rơi vào lớp này.
    /// </summary>
    public class OrderController : Controller
    {
        private const int CoTrang = 5;

        private readonly IOrderService _orders;
        private readonly IPaymentService _payment;

        public OrderController(IOrderService orders, IPaymentService payment)
        {
            _orders = orders;
            _payment = payment;
        }

        // ====================== Khách đã đăng nhập ======================

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(OrderStatus? trangThai, int page = 1)
        {
            ViewData["Title"] = "Đơn hàng của tôi";
            return View(await _orders.LayCuaToiAsync(User.LayId(), trangThai, page, CoTrang));
        }

        [Authorize]
        [HttpGet]
        [Route("Order/Detail/{code}")]
        public async Task<IActionResult> Detail(string code)
        {
            var don = await _orders.LayCuaToiTheoMaAsync(User.LayId(), code);
            if (don == null) return NotFound();

            ViewData["Title"] = "Đơn " + don.OrderCode;
            return View(TaoVm(don, laKhachLe: false));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string code, string? lyDo)
        {
            var (ok, loi) = await _orders.KhachHuyAsync(User.LayId(), code, lyDo);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã huỷ đơn hàng.";

            return RedirectToAction(nameof(Detail), new { code });
        }

        // ====================== Khách vãng lai ======================

        [HttpGet]
        public IActionResult Lookup(string? code)
        {
            ViewData["Title"] = "Tra cứu đơn hàng";
            return View(new OrderLookupForm { OrderCode = code });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lookup(OrderLookupForm form)
        {
            ViewData["Title"] = "Tra cứu đơn hàng";
            if (!ModelState.IsValid) return View(form);

            var don = await _orders.TraCuuAsync(form.OrderCode, form.Phone);
            if (don == null)
            {
                // Không nói rõ sai mã hay sai số điện thoại, để người ngoài không dò được mã đơn
                ModelState.AddModelError(string.Empty, "Không tìm thấy đơn khớp mã đơn và số điện thoại này.");
                return View(form);
            }

            // Đơn của chính tài khoản đang đăng nhập thì mở trang "của tôi" cho đủ chức năng
            if (User.LayId() > 0 && don.UserId == User.LayId())
                return RedirectToAction(nameof(Detail), new { code = don.OrderCode });

            DonKhachLe.ChoPhep(HttpContext.Session, don.OrderCode);
            return RedirectToAction(nameof(Guest), new { code = don.OrderCode });
        }

        [HttpGet]
        [Route("Order/Guest/{code}")]
        public async Task<IActionResult> Guest(string code)
        {
            if (!DonKhachLe.DuocXem(HttpContext.Session, code))
                return RedirectToAction(nameof(Lookup), new { code });

            var don = await _orders.LayTheoMaAsync(code);
            if (don == null) return RedirectToAction(nameof(Lookup));

            ViewData["Title"] = "Đơn " + don.OrderCode;
            return View(nameof(Detail), TaoVm(don, laKhachLe: true));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuestCancel(string code, string? lyDo)
        {
            if (!DonKhachLe.DuocXem(HttpContext.Session, code))
                return RedirectToAction(nameof(Lookup), new { code });

            var (ok, loi) = await _orders.KhachLeHuyAsync(code, lyDo);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã huỷ đơn hàng.";

            return RedirectToAction(nameof(Guest), new { code });
        }

        // ====================== Nội bộ ======================

        private MyOrderDetailViewModel TaoVm(Order don, bool laKhachLe)
        {
            var vm = new MyOrderDetailViewModel { Don = don, LaKhachLe = laKhachLe };
            if (_payment.CanThanhToanQr(don))
            {
                vm.QrUrl = _payment.TaoQrUrl(don);
                vm.ChuyenKhoan = _payment.LayThongTinChuyenKhoan(don);
            }
            return vm;
        }
    }
}
