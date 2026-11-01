using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    /// <summary>
    /// Đánh giá của khách cho một sản phẩm.
    ///
    /// Chỉ khách đã mua mới được đánh giá: điều kiện này kiểm bằng cách tra
    /// xem có dòng hàng nào của sản phẩm nằm trong đơn đã Hoàn thành của khách
    /// hay không. Xem ReviewService.DaMuaAsync.
    /// </summary>
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public int UserId { get; set; }
        public virtual User? User { get; set; }

        [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5")]
        [Display(Name = "Số sao")]
        public int Rating { get; set; } = 5;

        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Nhận xét")]
        public string? Comment { get; set; }

        [Display(Name = "Ngày đánh giá")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
