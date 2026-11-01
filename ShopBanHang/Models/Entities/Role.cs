using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ShopBanHang.Models.Entities
{
    /// <summary>
    /// Vai trò người dùng: Admin và Customer.
    ///
    /// Kế thừa IdentityRole&lt;int&gt; để khoá chính là int, khớp với User.
    /// Identity nối User với Role qua bảng AspNetUserRoles, nên User không
    /// còn cột RoleId nữa - một người về lý thuyết giữ được nhiều vai trò.
    /// </summary>
    public class Role : IdentityRole<int>
    {
        public Role() { }

        public Role(string ten) : base(ten) { }

        [StringLength(100)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        // Dùng hằng số thay vì gõ chuỗi rải rác trong [Authorize(Roles = ...)]
        public const string Admin = "Admin";
        public const string Customer = "Customer";
    }
}
