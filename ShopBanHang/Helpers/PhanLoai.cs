namespace ShopBanHang.Helpers
{
    /// <summary>Chữ hiển thị cho một biến thể, dùng chung cho giỏ hàng và đơn hàng.</summary>
    public static class PhanLoai
    {
        /// <summary>Ví dụ "256GB · Titan Đen".</summary>
        public static string Ten(string? storage, string? color)
            => string.Join(" · ", new[] { storage, color }.Where(x => !string.IsNullOrWhiteSpace(x)));

        /// <summary>Ảnh của biến thể nếu có, không thì ảnh sản phẩm, không nữa thì ảnh trống.</summary>
        public static string Anh(string? anhBienThe, string? anhSanPham)
            => !string.IsNullOrWhiteSpace(anhBienThe) ? anhBienThe
             : !string.IsNullOrWhiteSpace(anhSanPham) ? anhSanPham
             : "/images/products/no-image.svg";
    }
}
