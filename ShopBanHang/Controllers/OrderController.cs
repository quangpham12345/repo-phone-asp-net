using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers
{
    /// <summary>
    /// "Đơn hàng của tôi" phía khách: danh sách, chi tiết, huỷ khi còn chờ xác nhận.
    /// Trùng tên lớp với Areas/Admin/Controllers/OrderController nhưng khác namespace và
    /// không có [Area], nên route mặc định /Order/... chỉ rơi vào lớp này.
    /// </summary>
    [Authorize]
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

        [HttpGet]
        public async Task<IActionResult> Index(OrderStatus? trangThai, int page = 1)
        {
            ViewData["Title"] = "Đơn hàng của tôi";
            return View(await _orders.LayCuaToiAsync(User.LayId(), trangThai, page, CoTrang));
        }

        [HttpGet]
        [Route("Order/Detail/{code}")]
        public async Task<IActionResult> Detail(string code)
        {
            var don = await _orders.LayCuaToiTheoMaAsync(User.LayId(), code);
            if (don == null) return NotFound();

            ViewData["Title"] = "Đơn " + don.OrderCode;

            var vm = new MyOrderDetailViewModel { Don = don };
            if (_payment.CanThanhToanQr(don))
            {
                vm.QrUrl = _payment.TaoQrUrl(don);
                vm.ChuyenKhoan = _payment.LayThongTinChuyenKhoan(don);
            }
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(string code, string? lyDo)
        {
            var (ok, loi) = await _orders.KhachHuyAsync(User.LayId(), code, lyDo);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã huỷ đơn hàng.";

            return RedirectToAction(nameof(Detail), new { code });
        }
    }
}
