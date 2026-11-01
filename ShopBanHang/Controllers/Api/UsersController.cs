using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopBanHang.Models.Dtos;
using ShopBanHang.Models.Entities;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Controllers.Api
{
    /// <summary>
    /// API tài khoản theo RESTful: tài nguyên nằm ở URL, hành động ở HTTP method,
    /// kết quả ở mã trạng thái.
    ///
    ///   GET /api/users            danh sách, có lọc và phân trang qua query string
    ///   GET /api/users/{id}       một tài khoản, không có thì 404
    ///
    /// Dữ liệu tài khoản là thông tin cá nhân nên cả hai endpoint đều yêu cầu
    /// quyền Admin. Vì dùng cookie, Postman phải mượn cookie đăng nhập mới gọi được;
    /// gọi từ chính trang quản trị thì cookie đã có sẵn.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Role.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService) => _userService = userService;

        [HttpGet]
        public async Task<ActionResult<PagedDto<UserDto>>> GetAll(
            [FromQuery] string? tuKhoa,
            [FromQuery] int? roleId,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageSize is < 1 or > 100) pageSize = 10;   // chặn gọi pageSize quá lớn

            var kq = await _userService.LayDanhSachAsync(tuKhoa, roleId, isActive, page, pageSize);

            return Ok(new PagedDto<UserDto>
            {
                Page = kq.Page,
                PageSize = kq.PageSize,
                TotalItems = kq.TotalItems,
                TotalPages = kq.TotalPages,
                Items = kq.Items.Select(ToDto).ToList()
            });
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserDto>> GetById(int id)
        {
            var u = await _userService.LayTheoIdAsync(id);
            return u == null ? NotFound() : Ok(ToDto(u));
        }

        private static UserDto ToDto(User u) => new()
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            Phone = u.Phone,
            Address = u.Address,
            RoleName = u.Role?.Name,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }
}
