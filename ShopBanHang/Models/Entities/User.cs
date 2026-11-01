using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ShopBanHang.Models.Entities
{
    /// <summary>
    /// Tài khoản người dùng.
    ///
    /// Kế thừa IdentityUser&lt;int&gt; chứ không phải IdentityUser mặc định:
    /// bản mặc định dùng khoá chính kiểu string (GUID), sẽ kéo theo việc đổi
    /// Orders.UserId và ChatConversations.UserId sang string. Giữ int thì hai
    /// bảng đó không phải sửa gì.
    ///
    /// Email, PasswordHash, PhoneNumber, LockoutEnd... đã có sẵn trong lớp cha.
    /// Dưới đây chỉ khai thêm cột riêng của shop.
    /// </summary>
    public class User : IdentityUser<int>
    {
        [Required(ErrorMessage = "Họ tên bắt buộc nhập")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        /// <summary>
        /// Cờ bật tắt của shop, tách khỏi cơ chế Lockout của Identity.
        /// Lockout dùng cho việc khoá tạm khi nhập sai mật khẩu nhiều lần,
        /// còn cờ này là admin chủ động ngừng cho tài khoản hoạt động.
        /// </summary>
        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
