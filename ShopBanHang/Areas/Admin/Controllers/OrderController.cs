using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Quản lý đơn hàng: lọc đơn, xem chi tiết, xác nhận đơn, xác nhận đã thanh toán,
    /// chuyển trạng thái theo luồng một chiều, huỷ đơn (hoàn tồn kho).
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class OrderController : Controller
    {
        private const int CoTrang = 10;

        private readonly IOrderService _service;
        private readonly IPaymentService _payment;

        public OrderController(IOrderService service, IPaymentService payment)
        {
            _service = service;
            _payment = payment;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] OrderFilter loc, int page = 1)
        {
            DatTieuDe("Đơn hàng");

            ViewBag.Loc = loc;
            ViewBag.SoChoXacNhan = await _service.DemTheoTrangThaiAsync(OrderStatus.ChoXacNhan);

            return View(await _service.LayDanhSachAsync(loc, page, CoTrang));
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var don = await _service.LayChiTietAsync(id);
            if (don == null)
            {
                TempData["Loi"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction(nameof(Index));
            }

            DatTieuDe("Đơn " + don.OrderCode);

            var vm = new AdminOrderDetailViewModel
            {
                Don = don,
                BuocTiepTheo = OrderStatusFlow.BuocTiepTheo(don.Status)
                    .Select(tt => (tt, _service.LyDoKhongChuyenDuoc(don, tt)))
                    .ToList()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, OrderStatus moi, string? lyDoHuy)
        {
            var (ok, loi) = await _service.DoiTrangThaiAsync(id, moi, lyDoHuy);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = moi == OrderStatus.DaHuy
                ? "Đã huỷ đơn và hoàn lại tồn kho."
                : "Đã chuyển đơn sang trạng thái mới.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var (ok, loi) = await _payment.XacNhanDaThanhToanAsync(id);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã xác nhận đơn đã thanh toán.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        private void DatTieuDe(string tieuDe)
        {
            ViewData["Title"] = tieuDe;
            ViewData["ActiveMenu"] = "don-hang";
        }
    }
}
