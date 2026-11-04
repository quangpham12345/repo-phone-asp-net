namespace ShopBanHang.Models.ViewModels
{
    /// <summary>Một dòng giỏ hàng đã ghép giá, tồn kho hiện tại của biến thể.</summary>
    public class CartLineViewModel
    {
        public int CartItemId { get; set; }
        public int VariantId { get; set; }
        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = "/images/products/no-image.svg";

        /// <summary>Giá hiện tại của biến thể. Giỏ không chốt giá, chỉ đơn hàng mới chốt.</summary>
        public decimal UnitPrice { get; set; }
        public decimal? OldPrice { get; set; }

        public int Quantity { get; set; }
        public int Stock { get; set; }

        /// <summary>Sản phẩm và biến thể đều còn bán.</summary>
        public bool DangBan { get; set; }

        public decimal LineTotal => UnitPrice * Quantity;

        /// <summary>Có vấn đề gì làm dòng này chưa đặt được không. Null là ổn.</summary>
        public string? CanhBao
        {
            get
            {
                if (!DangBan) return "Sản phẩm này đã ngừng bán, vui lòng xoá khỏi giỏ.";
                if (Stock <= 0) return "Phân loại này đã hết hàng.";
                if (Quantity > Stock) return $"Chỉ còn {Stock} sản phẩm, vui lòng giảm số lượng.";
                return null;
            }
        }
    }

    public class CartViewModel
    {
        public List<CartLineViewModel> Lines { get; set; } = new();

        public bool TrongRong => Lines.Count == 0;

        public int TongSoLuong => Lines.Sum(l => l.Quantity);

        /// <summary>Tiền hàng của cả giỏ (chưa trừ mã giảm giá).</summary>
        public decimal TamTinh => Lines.Sum(l => l.LineTotal);

        /// <summary>Còn dòng nào có cảnh báo thì chưa cho sang bước đặt hàng.</summary>
        public bool CoVanDe => Lines.Any(l => l.CanhBao != null);
    }

    /// <summary>Trả về cho JavaScript sau khi thêm vào giỏ bằng AJAX.</summary>
    public record CartAjaxResult(bool Ok, string Message, int SoLuongGio, string? LoginUrl = null);
}
