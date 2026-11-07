using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Dtos;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers.Api
{
    /// <summary>
    /// Dữ liệu tra cứu cho client gọi API (để biết categoryId / brandId khi POST sản phẩm).
    ///
    ///   GET /api/categories    các danh mục đang hiện
    ///   GET /api/brands        các thương hiệu đang hiện
    /// </summary>
    [ApiController]
    [Produces("application/json")]
    public class CatalogController : ControllerBase
    {
        private readonly IProductService _productService;

        public CatalogController(IProductService productService) => _productService = productService;

        [HttpGet("api/categories")]
        public async Task<ActionResult<List<LookupDto>>> GetCategories()
            => Ok((await _productService.LayDanhMucAsync())
                .Select(c => new LookupDto { Id = c.Id, Name = c.Name, Slug = c.Slug })
                .ToList());

        [HttpGet("api/brands")]
        public async Task<ActionResult<List<LookupDto>>> GetBrands()
            => Ok((await _productService.LayThuongHieuAsync())
                .Select(b => new LookupDto { Id = b.Id, Name = b.Name, Slug = b.Slug })
                .ToList());
    }
}
