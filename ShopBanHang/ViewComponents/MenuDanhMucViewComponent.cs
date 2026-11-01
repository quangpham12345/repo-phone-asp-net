using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.ViewComponents
{
    /// <summary>
    /// Thanh danh mục trên header.
    ///
    /// Dùng ViewComponent chứ không Partial View vì khối này phải tự đi lấy
    /// dữ liệu từ CSDL. Partial chỉ nhận dữ liệu từ view cha, mà header nằm
    /// trong layout nên mọi controller sẽ phải nhớ nạp danh mục - rất dễ quên.
    /// </summary>
    public class MenuDanhMucViewComponent : ViewComponent
    {
        private readonly IProductService _productService;

        public MenuDanhMucViewComponent(IProductService productService)
            => _productService = productService;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var danhMuc = await _productService.LayDanhMucAsync();
            return View(danhMuc);   // không truyền tên nên view phải là Default.cshtml
        }
    }
}
