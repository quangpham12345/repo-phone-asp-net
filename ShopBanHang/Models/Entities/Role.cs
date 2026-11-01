using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    /// <summary>Vai trò người dùng: Admin và Customer.</summary>
    public class Role
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên vai trò bắt buộc nhập")]
        [StringLength(50)]
        [Display(Name = "Tên vai trò")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();

        // Dùng hằng số thay vì gõ chuỗi rải rác trong [Authorize(Roles = ...)]
        public const string Admin = "Admin";
        public const string Customer = "Customer";
    }
}
