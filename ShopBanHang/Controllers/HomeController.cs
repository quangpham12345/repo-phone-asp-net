using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly IBannerService _bannerService;

        public HomeController(IProductService productService, IBannerService bannerService)
        {
            _productService = productService;
            _bannerService = bannerService;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Trang chủ";

            ViewBag.Banners = await _bannerService.LayDangHienAsync();
            ViewBag.NoiBat = await _productService.LayNoiBatAsync(5);
            ViewBag.MoiNhat = await _productService.LayMoiNhatAsync(5);
            ViewBag.CacThuongHieu = await _productService.LayThuongHieuAsync(10);
            ViewBag.BanChay = await _productService.LayBanChayAsync(5);

            return View();
        }

        public IActionResult Privacy()
        {
            ViewData["Title"] = "Chính sách";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
