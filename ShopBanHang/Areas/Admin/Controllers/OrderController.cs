using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Quản lý đơn hàng: danh sách có lọc theo trạng thái, xem chi tiết,
    /// và đổi trạng thái theo luồng một chiều.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class OrderController : Controller
    {
        private const int CoTrang = 10;

        private readonly IOrderService _service;

        public OrderController(IOrderService service) => _service = service;

        [HttpGet]
        public async Task<IActionResult> Index(string? tuKhoa, OrderStatus? trangThai, int page = 1)
        {
            DatTieuDe("Đơn hàng");

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;

            return View(await _service.LayDanhSachAsync(tuKhoa, trangThai, page, CoTrang));
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
            return View(don);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, OrderStatus moi)
        {
            var (ok, loi) = await _service.DoiTrangThaiAsync(id, moi);

            if (!ok) TempData["Loi"] = loi;
            else TempData["ThanhCong"] = "Đã chuyển đơn sang trạng thái mới.";

            return RedirectToAction(nameof(Detail), new { id });
        }

        private void DatTieuDe(string tieuDe)
        {
            ViewData["Title"] = tieuDe;
            ViewData["ActiveMenu"] = "don-hang";
        }
    }
}
