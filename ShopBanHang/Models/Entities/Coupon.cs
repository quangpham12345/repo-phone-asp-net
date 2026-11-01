using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ShopBanHang.Models.Enums;

namespace ShopBanHang.Models.Entities
{
    /// <summary>Mã giảm giá áp ở bước thanh toán.</summary>
    public class Coupon
    {
        public int Id { get; set; }

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

        /// <summary>Là số % nếu DiscountType = PhanTram, là số tiền VNĐ nếu = SoTien.</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Range(typeof(decimal), "1", "1000000000", ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        [Display(Name = "Giá trị giảm")]
        public decimal DiscountValue { get; set; }

        /// <summary>Trần số tiền giảm khi giảm theo %. Null = không giới hạn.</summary>
        [Column(TypeName = "decimal(18,0)")]
        [Range(typeof(decimal), "1", "1000000000", ErrorMessage = "Mức giảm tối đa phải lớn hơn 0")]
        [Display(Name = "Giảm tối đa")]
        public decimal? MaxDiscount { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Range(typeof(decimal), "0", "1000000000", ErrorMessage = "Giá trị đơn tối thiểu không hợp lệ")]
        [Display(Name = "Đơn tối thiểu")]
        public decimal MinOrderValue { get; set; }

        /// <summary>Tổng số lượt được dùng. Null = không giới hạn.</summary>
        [Range(1, 1_000_000, ErrorMessage = "Số lượt dùng phải từ 1 trở lên")]
        [Display(Name = "Số lượt dùng tối đa")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Đã dùng")]
        public int UsedCount { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu")]
        [DataType(DataType.Date)]
        [Display(Name = "Từ ngày")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc")]
        [DataType(DataType.Date)]
        [Display(Name = "Đến ngày")]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(30);

        [Display(Name = "Đang bật")]
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public bool ConLuot => UsageLimit == null || UsedCount < UsageLimit;

        /// <summary>Trạng thái tại thời điểm <paramref name="now"/>. Thứ tự kiểm tra cũng là độ ưu tiên.</summary>
        public CouponStatus LayTrangThai(DateTime now)
        {
            if (!IsActive) return CouponStatus.DaTat;
            if (EndDate < now) return CouponStatus.HetHan;
            if (StartDate > now) return CouponStatus.SapDienRa;
            if (!ConLuot) return CouponStatus.HetLuot;
            return CouponStatus.DangChay;
        }
    }
}