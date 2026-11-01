using System.ComponentModel.DataAnnotations;
using ShopBanHang.Models.Entities;

namespace ShopBanHang.Models.ViewModels
{
    /// <summary>Dữ liệu cho màn danh sách tài khoản: một trang kết quả kèm điều kiện lọc đang áp.</summary>
    public class UserListViewModel
    {
        public PagedResult<User> KetQua { get; set; } = new();

        [Display(Name = "Từ khoá")]
        public string? TuKhoa { get; set; }          // tìm theo họ tên hoặc email

        [Display(Name = "Vai trò")]
        public int? RoleId { get; set; }

        [Display(Name = "Trạng thái")]
        public bool? IsActive { get; set; }

        public List<Role> CacVaiTro { get; set; } = new();
    }

    /// <summary>
    /// Form thêm và sửa tài khoản dùng chung.
    /// Không bind thẳng entity User vào form để mật khẩu băm không bị lộ ra view.
    /// </summary>
    public class UserFormViewModel
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

        /// <summary>Thêm mới thì bắt buộc, sửa thì bỏ trống là giữ mật khẩu cũ.</summary>
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Hãy chọn vai trò")]
        [Display(Name = "Vai trò")]
        public int RoleId { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        public List<Role> CacVaiTro { get; set; } = new();

        public bool LaThemMoi => Id == 0;
    }
}
