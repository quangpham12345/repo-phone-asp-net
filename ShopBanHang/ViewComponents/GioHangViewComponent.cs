using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.ViewComponents
{
    /// <summary>
    /// Nút giỏ hàng kèm badge số lượng trên header. Tự lấy số lượng: đã đăng nhập thì
    /// đếm trong CSDL, chưa đăng nhập thì đếm trong Session.
    /// </summary>
    public class GioHangViewComponent : ViewComponent
    {
        private readonly ICartService _cart;

        public GioHangViewComponent(ICartService cart) => _cart = cart;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int userId = UserClaimsPrincipal.LayId();
            int soLuong = userId > 0
                ? await _cart.DemAsync(userId)
                : GioKhachLe.Dem(HttpContext.Session);

            return View(soLuong);
        }
    }
}
