using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email bắt buộc nhập")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu bắt buộc nhập")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; }

        /// <summary>Trang người dùng định vào trước khi bị chặn, để đăng nhập xong đưa về đúng đó.</summary>
        public string? ReturnUrl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Họ tên bắt buộc nhập")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email bắt buộc nhập")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu bắt buộc nhập")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Hai lần nhập mật khẩu không giống nhau")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }
    }

    public class ProfileViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;   // chỉ xem, không cho đổi

        [Required(ErrorMessage = "Họ tên bắt buộc nhập")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Vai trò")]
        public string? RoleName { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mật khẩu hiện tại bắt buộc nhập")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới bắt buộc nhập")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu mới")]
        [Compare(nameof(NewPassword), ErrorMessage = "Hai lần nhập mật khẩu không giống nhau")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email bắt buộc nhập")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email đã đăng ký")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Hệ thống thật gửi liên kết này qua email. Bài tập lớn chưa cấu hình
        /// máy chủ thư nên hiện thẳng ra màn hình để còn thử được.
        /// </summary>
        public string? LienKetDatLai { get; set; }
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>Mã do Identity sinh, chỉ dùng được một lần và có hạn.</summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới bắt buộc nhập")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Nhập lại mật khẩu mới")]
        [Compare(nameof(Password), ErrorMessage = "Hai lần nhập mật khẩu không giống nhau")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
