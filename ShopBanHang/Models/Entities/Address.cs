using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    /// <summary>
    /// Một địa chỉ trong sổ địa chỉ giao hàng của khách.
    ///
    /// Tách thành bảng riêng vì một người có nhiều nơi nhận hàng (nhà, công ty,
    /// nhà người thân). Cột Address trên tài khoản chỉ còn là địa chỉ liên hệ
    /// mặc định lúc đăng ký.
    /// </summary>
    public class Address
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [Required(ErrorMessage = "Tên người nhận bắt buộc nhập")]
        [StringLength(100, MinimumLength = 2)]
        [Display(Name = "Người nhận")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại bắt buộc nhập")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ bắt buộc nhập")]
        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string Detail { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ mặc định khi đặt hàng. Mỗi khách chỉ có một, service lo việc
        /// gỡ cờ ở các địa chỉ còn lại khi đặt cái mới làm mặc định.
        /// </summary>
        [Display(Name = "Đặt làm mặc định")]
        public bool IsDefault { get; set; }

        [Display(Name = "Ngày thêm")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>Gộp thành một dòng để hiện ở danh sách và ô chọn khi đặt hàng.</summary>
        public string MotDong() => $"{ReceiverName} · {Phone} · {Detail}";
    }
}
