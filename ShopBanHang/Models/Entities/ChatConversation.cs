using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    /// <summary>
    /// Một đoạn hội thoại hỗ trợ giữa khách và quản trị viên.
    ///
    /// Khách có tài khoản thì neo bằng UserId. Khách chưa đăng nhập thì neo bằng
    /// GuestToken - một chuỗi ngẫu nhiên lưu trong cookie của khách - nhờ vậy
    /// tải lại trang hay tắt trình duyệt vẫn còn lịch sử chat.
    ///
    /// Phần code chat làm ở giai đoạn sau; bảng tạo trước để ERD chốt một lần.
    /// </summary>
    public class ChatConversation
    {
        public int Id { get; set; }

        [Display(Name = "Tài khoản")]
        public int? UserId { get; set; }
        public virtual User? User { get; set; }

        /// <summary>
        /// Khoá neo cho khách chưa đăng nhập, lưu trong cookie.
        /// Không đặt unique vì SQL Server chỉ cho phép đúng một dòng NULL trên cột unique.
        /// </summary>
        [StringLength(64)]
        public string? GuestToken { get; set; }

        [StringLength(100)]
        [Display(Name = "Tên khách")]
        public string? GuestName { get; set; }

        [Display(Name = "Đã đóng")]
        public bool IsClosed { get; set; }

        [Display(Name = "Tạo lúc")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Tin cuối")]
        public DateTime LastMessageAt { get; set; } = DateTime.Now;

        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
