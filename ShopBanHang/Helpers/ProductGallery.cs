namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Gộp bộ ảnh của một sản phẩm: ảnh hiện có, trừ ảnh bị xoá, cộng ảnh mới,
    /// rồi chọn ảnh đại diện (ImageUrl) và ghép phần còn lại thành GalleryUrls.
    ///
    /// Chỉ xử lý chuỗi đường dẫn, không đụng DB hay đĩa nên dễ kiểm tra và dùng lại được.
    /// </summary>
    public static class ProductGallery
    {
        /// <summary>Số ảnh phụ tối đa, chưa tính ảnh đại diện.</summary>
        public const int ToiDaAnhPhu = 10;

        private const int DoDaiGalleryToiDa = 1000;   // khớp [StringLength(1000)] của Product.GalleryUrls
        private const int DoDaiAnhToiDa = 250;        // khớp [StringLength(250)] của Product.ImageUrl

        public sealed record KetQua(string? AnhChinh, string? Gallery, string? Loi);

        /// <summary>Tách chuỗi ảnh ngăn cách bằng dấu ; thành danh sách, bỏ ô trống và trùng.</summary>
        public static List<string> Tach(string? chuoi)
            => string.IsNullOrWhiteSpace(chuoi)
                ? new List<string>()
                : chuoi.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                       .Distinct(StringComparer.OrdinalIgnoreCase)
                       .ToList();

        /// <param name="anhChinhHienTai">ImageUrl đang lưu của sản phẩm (form gửi lại qua ô ẩn).</param>
        /// <param name="galleryHienTai">GalleryUrls đang lưu.</param>
        /// <param name="anhXoa">Các ảnh admin tích chọn xoá.</param>
        /// <param name="anhChinhChon">Ảnh admin chọn làm đại diện, có thể rỗng.</param>
        /// <param name="anhTaiLenMoi">Đường dẫn các ảnh vừa tải lên trong lần lưu này.</param>
        /// <param name="urlMoi">Đường dẫn ảnh gõ tay, nhiều ảnh cách nhau bằng dấu ;.</param>
        public static KetQua Gop(
            string? anhChinhHienTai,
            string? galleryHienTai,
            IEnumerable<string>? anhXoa,
            string? anhChinhChon,
            IEnumerable<string>? anhTaiLenMoi,
            string? urlMoi)
        {
            var xoa = new HashSet<string>(
                (anhXoa ?? Enumerable.Empty<string>()).Select(x => x.Trim()),
                StringComparer.OrdinalIgnoreCase);

            var tatCa = new List<string>();
            string? loi = null;

            void Them(string? u)
            {
                if (string.IsNullOrWhiteSpace(u)) return;
                u = u.Trim();

                if (xoa.Contains(u)) return;                       // ảnh bị tích xoá thì bỏ, kể cả ảnh đại diện
                if (!HopLe(u)) { loi ??= $"Đường dẫn ảnh không hợp lệ: {u}"; return; }
                if (!tatCa.Contains(u, StringComparer.OrdinalIgnoreCase)) tatCa.Add(u);
            }

            Them(anhChinhHienTai);
            foreach (var u in Tach(galleryHienTai)) Them(u);
            foreach (var u in anhTaiLenMoi ?? Enumerable.Empty<string>()) Them(u);
            foreach (var u in Tach(urlMoi)) Them(u);

            if (loi != null) return new KetQua(null, null, loi);

            // Ưu tiên: ảnh admin vừa chọn, rồi ảnh đại diện cũ nếu còn, rồi ảnh đầu tiên
            string? chinh = TimTrong(tatCa, anhChinhChon)
                            ?? TimTrong(tatCa, anhChinhHienTai)
                            ?? tatCa.FirstOrDefault();

            var phu = tatCa.Where(x => !string.Equals(x, chinh, StringComparison.OrdinalIgnoreCase)).ToList();

            if (phu.Count > ToiDaAnhPhu)
                return new KetQua(null, null,
                    $"Mỗi sản phẩm tối đa {ToiDaAnhPhu} ảnh phụ (ngoài ảnh đại diện). Hãy xoá bớt ảnh.");

            string? gallery = phu.Count == 0 ? null : string.Join(";", phu);

            if (gallery != null && gallery.Length > DoDaiGalleryToiDa)
                return new KetQua(null, null, "Danh sách ảnh phụ quá dài, hãy xoá bớt ảnh hoặc dùng đường dẫn ngắn hơn.");

            if (chinh != null && chinh.Length > DoDaiAnhToiDa)
                return new KetQua(null, null, "Đường dẫn ảnh đại diện quá dài (tối đa 250 ký tự).");

            return new KetQua(chinh, gallery, null);
        }

        private static string? TimTrong(List<string> ds, string? u)
            => string.IsNullOrWhiteSpace(u)
                ? null
                : ds.FirstOrDefault(x => string.Equals(x, u.Trim(), StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Chỉ nhận đường dẫn tương đối bắt đầu bằng một dấu / (ảnh trong wwwroot) hoặc http(s).
        /// Chặn javascript:, data: và đường dẫn dạng //host.
        /// </summary>
        private static bool HopLe(string u)
            => (u.StartsWith('/') && !u.StartsWith("//") && !u.Contains('\\'))
               || u.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
               || u.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}