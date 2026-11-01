using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    /// <summary>Ai gửi tin. Khách chưa đăng nhập không có dòng trong Users nên phải có cột riêng.</summary>
    public enum ChatSender
    {
        [Display(Name = "Khách")]
        Khach = 0,

        [Display(Name = "Quản trị")]
        Admin = 1
    }

    public class ChatMessage
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }
        public virtual ChatConversation? Conversation { get; set; }

        /// <summary>Cột quyết định bên gửi, không suy ra từ UserId vì khách lẻ không có tài khoản.</summary>
        [Display(Name = "Người gửi")]
        public ChatSender Sender { get; set; }

        [Required(ErrorMessage = "Nội dung tin nhắn không được để trống")]
        [StringLength(1000)]
        [Display(Name = "Nội dung")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Đã xem")]
        public bool IsRead { get; set; }

        [Display(Name = "Gửi lúc")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
