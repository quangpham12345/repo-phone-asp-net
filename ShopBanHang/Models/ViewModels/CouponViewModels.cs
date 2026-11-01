using System.ComponentModel.DataAnnotations;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;

namespace ShopBanHang.Models.ViewModels
{
    /// <summary>Dữ liệu cho trang danh sách mã giảm giá: điều kiện lọc và một trang kết quả.</summary>
    public class CouponListViewModel
    {
        public string? TuKhoa { get; set; }
        public CouponStatus? TrangThai { get; set; }
        public PagedResult<Coupon> KetQua { get; set; } = new();
    }

    /// <summary>Dữ liệu cho form thêm/sửa mã giảm giá. Không bind thẳng entity.</summary>
    public class CouponFormViewModel
    {
        public int Id { get; set; }

        public bool LaThemMoi => Id == 0;

        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá")]
        [StringLength(30, ErrorMessage = "Mã tối đa 30 ký tự")]
        [RegularExpression(@"^[A-Za-z0-9_-]+$", ErrorMessage = "Mã chỉ gồm chữ, số, gạch ngang và gạch dưới")]
        [Display(Name = "Mã giảm giá")]
        public string Code { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Mô tả tối đa 200 ký tự")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Kiểu giảm")]
        public DiscountType DiscountType { get; set; } = DiscountType.PhanTram;

        [Range(typeof(decimal), "1", "1000000000", ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        [Display(Name = "Giá trị giảm")]
        public decimal DiscountValue { get; set; }

        [Range(typeof(decimal), "1", "1000000000", ErrorMessage = "Mức giảm tối đa phải lớn hơn 0")]
        [Display(Name = "Giảm tối đa (VNĐ)")]
        public decimal? MaxDiscount { get; set; }

        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Giá trị đơn tối thiểu không hợp lệ")]
        [Display(Name = "Đơn tối thiểu (VNĐ)")]
        public decimal MinOrderValue { get; set; }

        [Range(1, 1_000_000, ErrorMessage = "Số lượt dùng phải từ 1 trở lên")]
        [Display(Name = "Số lượt dùng tối đa")]
        public int? UsageLimit { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Từ ngày")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Đến ngày")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);

        [Display(Name = "Đang bật")]
        public bool IsActive { get; set; } = true;

        /// <summary>Chỉ để hiển thị ở form sửa; service không tin giá trị này mà đọc lại từ DB.</summary>
        public int UsedCount { get; set; }
    }
}