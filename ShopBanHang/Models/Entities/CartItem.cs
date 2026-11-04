using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Entities
{
    /// <summary>
    /// Một dòng trong giỏ hàng của khách đã đăng nhập.
    ///
    /// Giỏ gắn với biến thể chứ không gắn với sản phẩm, vì giá và tồn kho nằm ở
    /// biến thể (256GB Đen khác giá 512GB Trắng). Giỏ lưu trong CSDL nên khách
    /// đăng xuất rồi đăng nhập lại, hoặc đổi máy, vẫn còn nguyên giỏ.
    ///
    /// Giỏ KHÔNG chốt giá: giá luôn đọc lại từ ProductVariants mỗi lần hiện giỏ.
    /// Giá chỉ được chốt vào OrderDetails ở bước đặt hàng.
    /// </summary>
    public class CartItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public int ProductVariantId { get; set; }
        public virtual ProductVariant? ProductVariant { get; set; }

        [Range(1, 99, ErrorMessage = "Số lượng từ 1 đến 99")]
        [Display(Name = "Số lượng")]
        public int Quantity { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
