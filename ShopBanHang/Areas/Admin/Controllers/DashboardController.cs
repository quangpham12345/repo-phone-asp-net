using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Entities;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Areas.Admin.Controllers
{
    /// <summary>
    /// Trang tổng quan khu quản trị.
    /// Thống kê gộp luôn vào đây, không làm trang Thống kê riêng.
    /// </summary>
    [Area("Admin")]
    [Authorize(Roles = Role.Admin)]
    public class DashboardController : Controller
    {
        private readonly IThongKeService _thongKe;
        private readonly IProductService _products;

        public DashboardController(IThongKeService thongKe, IProductService product)
        {
            _thongKe = thongKe;
            _products = product;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Tổng quan";
            ViewData["ActiveMenu"] = "dashboard";

            var vm = await _thongKe.LaySoLieuAsync();
            vm.CanhBaoTonKho = await _products.LayCanhBaoTonKhoAsync();
            return View(vm);
        }
    }
}
