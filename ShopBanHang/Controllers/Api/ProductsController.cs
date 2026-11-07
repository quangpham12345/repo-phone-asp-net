using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Dtos;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers.Api
{
    /// <summary>
    /// API sản phẩm theo RESTful: tài nguyên là /api/products, hành động là HTTP method,
    /// kết quả là mã trạng thái HTTP.
    ///
    ///   GET    /api/products               200  danh sách, lọc + phân trang qua query string
    ///   GET    /api/products/{id}          200  một sản phẩm kèm biến thể | 404
    ///   POST   /api/products               201  tạo mới, header Location trỏ tới sản phẩm vừa tạo | 400
    ///   PUT    /api/products/{id}          204  cập nhật toàn bộ | 400 | 404
    ///   DELETE /api/products/{id}          204  xoá mềm (ẩn) | 404
    ///
    /// Đọc thì ai cũng gọi được. Ghi (POST/PUT/DELETE) cần đăng nhập bằng tài khoản Admin;
    /// chưa đăng nhập trả 401, không đủ quyền trả 403 (cấu hình ở Program.cs).
    ///
    /// Controller không tự viết nghiệp vụ: gọi lại IProductService giống hệt màn quản trị,
    /// nên luật (slug không trùng, ảnh hợp lệ, danh mục tồn tại...) chỉ viết một lần.
    /// Ví dụ gọi thử: file ShopBanHang.http ở thư mục gốc dự án.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService) => _productService = productService;

        private bool LaAdmin => User.IsInRole(Role.Admin);

        /// <summary>
        /// GET /api/products?tuKhoa=iphone&amp;danhMuc=dien-thoai&amp;thuongHieu=apple&amp;giaTu=1000000&amp;giaDen=30000000&amp;sapXep=1&amp;page=1&amp;pageSize=10
        /// sapXep: 0 mới nhất, 1 giá tăng, 2 giá giảm, 3 bán chạy.
        /// includeHidden=true (chỉ Admin) để lấy cả sản phẩm đã ẩn.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedDto<ProductDto>>> GetAll(
            [FromQuery] ProductFilter loc,
            [FromQuery] bool includeHidden = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageSize is < 1 or > 100) pageSize = 10;   // chặn gọi pageSize quá lớn
            loc.ChiHangDangBan = !(includeHidden && LaAdmin);

            var kq = await _productService.LayDanhSachAsync(loc, page, pageSize);

            return Ok(new PagedDto<ProductDto>
            {
                Page = kq.Page,
                PageSize = kq.PageSize,
                TotalItems = kq.TotalItems,
                TotalPages = kq.TotalPages,
                Items = kq.Items.Select(p => GanChung(new ProductDto(), p)).ToList()
            });
        }

        /// <summary>GET /api/products/5</summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDetailDto>> GetById(int id)
        {
            var p = await _productService.LayTheoIdAsync(id);

            // Sản phẩm đã ẩn coi như không tồn tại với khách
            if (p == null || (!p.IsActive && !LaAdmin)) return NotFound();

            return Ok(ToDetailDto(p));
        }

        /// <summary>POST /api/products — body JSON là ProductWriteDto.</summary>
        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDetailDto>> Create([FromBody] ProductWriteDto dto)
        {
            var vm = ToFormViewModel(dto, id: 0);
            var (ok, loi) = await _productService.LuuAsync(vm);
            if (!ok) return LoiNghiepVu(loi);

            // LuuAsync gán Id mới vào vm.Id
            var moi = await _productService.LayTheoIdAsync(vm.Id);
            return CreatedAtAction(nameof(GetById), new { id = vm.Id }, ToDetailDto(moi!));
        }

        /// <summary>PUT /api/products/5 — gửi đủ các trường, giống form sửa của admin.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = Role.Admin)]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ProductWriteDto dto)
        {
            var hienTai = await _productService.LayTheoIdAsync(id);
            if (hienTai == null) return NotFound();

            // Không gửi ảnh thì giữ ảnh đang có
            dto.ImageUrl ??= hienTai.ImageUrl;
            dto.GalleryUrls ??= hienTai.GalleryUrls;

            var (ok, loi) = await _productService.LuuAsync(ToFormViewModel(dto, id));
            if (!ok) return LoiNghiepVu(loi);

            return NoContent();
        }

        /// <summary>
        /// DELETE /api/products/5 — xoá MỀM: chuyển sang "ẩn" (IsActive = false).
        /// Không xoá cứng vì OrderDetails của các đơn cũ đang trỏ tới sản phẩm.
        /// Gọi lại nhiều lần vẫn trả 204 (idempotent đúng tinh thần DELETE).
        /// </summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Role.Admin)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var p = await _productService.LayTheoIdAsync(id);
            if (p == null) return NotFound();

            if (p.IsActive)
            {
                var (ok, loi) = await _productService.DoiTrangThaiAsync(id);
                if (!ok) return LoiNghiepVu(loi);
            }
            return NoContent();
        }

        // ===== Chuyển đổi =====

        /// <summary>Lỗi nghiệp vụ trả 400 theo chuẩn ProblemDetails (RFC 7807).</summary>
        private ActionResult LoiNghiepVu(string? loi)
            => Problem(detail: loi, statusCode: StatusCodes.Status400BadRequest, title: "Dữ liệu không hợp lệ");

        private static ProductFormViewModel ToFormViewModel(ProductWriteDto dto, int id) => new()
        {
            Id = id,
            Name = dto.Name,
            CategoryId = dto.CategoryId,
            BrandId = dto.BrandId,
            ImageUrl = dto.ImageUrl,
            GalleryUrls = dto.GalleryUrls,
            Description = dto.Description,
            Ram = dto.Ram,
            Os = dto.Os,
            Screen = dto.Screen,
            Chip = dto.Chip,
            Battery = dto.Battery,
            Camera = dto.Camera,
            IsFeatured = dto.IsFeatured,
            IsActive = dto.IsActive
        };

        private static T GanChung<T>(T dto, Product p) where T : ProductDto
        {
            dto.Id = p.Id;
            dto.Name = p.Name;
            dto.Slug = p.Slug;
            dto.ImageUrl = p.ImageUrl;
            dto.Category = p.Category?.Name;
            dto.Brand = p.Brand?.Name;
            dto.Price = p.HasVariants ? p.DisplayPrice : null;
            dto.OldPrice = p.DisplayOldPrice;
            dto.TotalStock = p.TotalStock;
            dto.SoldCount = p.SoldCount;
            dto.IsFeatured = p.IsFeatured;
            dto.IsActive = p.IsActive;
            return dto;
        }

        private static ProductDetailDto ToDetailDto(Product p)
        {
            var dto = GanChung(new ProductDetailDto(), p);
            dto.CategoryId = p.CategoryId;
            dto.BrandId = p.BrandId;
            dto.Description = p.Description;
            dto.Gallery = ProductGallery.Tach(p.GalleryUrls);
            dto.Ram = p.Ram;
            dto.Os = p.Os;
            dto.Screen = p.Screen;
            dto.Chip = p.Chip;
            dto.Battery = p.Battery;
            dto.Camera = p.Camera;
            dto.CreatedAt = p.CreatedAt;
            dto.Variants = p.Variants
                .OrderBy(v => v.Price)
                .Select(v => new VariantDto
                {
                    Id = v.Id,
                    Storage = v.Storage,
                    Color = v.Color,
                    Price = v.Price,
                    OldPrice = v.OldPrice,
                    Stock = v.Stock,
                    IsActive = v.IsActive
                })
                .ToList();
            return dto;
        }
    }
}
