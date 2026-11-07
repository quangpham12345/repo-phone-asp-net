namespace ShopBanHang.Helpers
{
    /// <summary>
    /// "Sản phẩm bạn đã xem" lưu trong COOKIE của trình duyệt (khác với giỏ hàng lưu trong Session).
    ///
    /// Vì sao dùng cookie: danh sách này cần nhớ qua nhiều lần ghé (30 ngày), kể cả khi tắt trình
    /// duyệt hay chưa đăng nhập, trong khi Session mất khi hết phiên. Dữ liệu chỉ là vài id sản phẩm,
    /// không nhạy cảm, nên để phía trình duyệt là hợp lý.
    ///
    /// Giá trị cookie dạng "12-5-33" (id mới xem đứng trước). Dùng dấu '-' vì không cần mã hoá URL.
    /// Cookie do người dùng sửa được, nên khi đọc phải kiểm tra lại từng phần tử (xem Doc).
    /// </summary>
    public static class CookieDaXem
    {
        public const string Ten = "ShopBanHang.DaXem";
        public const int ToiDa = 10;
        private const int SoNgayLuu = 30;

        public static List<int> Doc(HttpRequest request)
        {
            if (!request.Cookies.TryGetValue(Ten, out var giaTri) || string.IsNullOrWhiteSpace(giaTri))
                return new List<int>();

            return giaTri.Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .Take(ToiDa)
                .ToList();
        }

        /// <summary>Đưa sản phẩm vừa xem lên đầu danh sách, cắt bớt khi quá ToiDa.</summary>
        public static void Them(HttpContext http, int productId)
        {
            var ds = Doc(http.Request);
            ds.Remove(productId);
            ds.Insert(0, productId);
            if (ds.Count > ToiDa) ds = ds.Take(ToiDa).ToList();

            http.Response.Cookies.Append(Ten, string.Join('-', ds), new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(SoNgayLuu),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = http.Request.IsHttps
            });
        }

        public static void Xoa(HttpContext http) => http.Response.Cookies.Delete(Ten);
    }
}
