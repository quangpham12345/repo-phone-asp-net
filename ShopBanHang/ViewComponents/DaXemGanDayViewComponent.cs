using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.ViewComponents
{
    /// <summary>
    /// Khối "Sản phẩm bạn đã xem": đọc danh sách id từ cookie (CookieDaXem) rồi lấy sản phẩm.
    /// Gọi trong view: @await Component.InvokeAsync("DaXemGanDay", new { boQuaId = sp.Id })
    /// </summary>
    public class DaXemGanDayViewComponent : ViewComponent
    {
        private readonly IProductService _productService;

        public DaXemGanDayViewComponent(IProductService productService) => _productService = productService;

        /// <param name="boQuaId">Id sản phẩm đang mở, để không hiện lại chính nó.</param>
        public async Task<IViewComponentResult> InvokeAsync(int boQuaId = 0, int soLuong = 5)
        {
            var ids = CookieDaXem.Doc(HttpContext.Request)
                .Where(id => id != boQuaId)
                .Take(soLuong)
                .ToList();

            var ds = await _productService.LayTheoThuTuIdAsync(ids);
            return View(ds);
        }
    }
}
