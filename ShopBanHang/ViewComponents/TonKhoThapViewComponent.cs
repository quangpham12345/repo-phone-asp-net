using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.ViewComponents
{
    /// <summary>Số mục tồn kho thấp, hiện cạnh menu Sản phẩm trong khu quản trị.</summary>
    public class TonKhoThapViewComponent : ViewComponent
    {
        private readonly IProductService _products;

        public TonKhoThapViewComponent(IProductService products) => _products = products;

        public async Task<IViewComponentResult> InvokeAsync()
            => View(await _products.DemCanhBaoTonKhoAsync());
    }
}