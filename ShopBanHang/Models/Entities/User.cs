using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Họ tên bắt buộc nhập")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email bắt buộc nhập")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(150)]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>Mật khẩu đã băm PBKDF2, không bao giờ lưu dạng thô. Xem PasswordHelper.</summary>
        [Required]
        [StringLength(200)]
        public string PasswordHash { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Vai trò")]
        public int RoleId { get; set; }
        public virtual Role? Role { get; set; }

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
