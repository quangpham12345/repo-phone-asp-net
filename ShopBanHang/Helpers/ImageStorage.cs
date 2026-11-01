namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Lưu ảnh admin tải lên vào wwwroot và trả về đường dẫn web của ảnh.
    ///
    /// Chỉ nhận đúng vài phần mở rộng ảnh, và luôn tự đặt tên file mới bằng Guid.
    /// Lấy nguyên tên file người dùng gửi lên là lỗ hổng: tên có thể chứa "../"
    /// để ghi ra ngoài thư mục, hoặc trùng tên làm mất ảnh cũ.
    /// </summary>
    public static class ImageStorage
    {
        private static readonly string[] DuoiChoPhep = { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
        private const long DungLuongToiDa = 4 * 1024 * 1024;   // 4 MB
        private const int SoAnhToiDaMoiLan = 10;

        /// <summary>
        /// Lưu file vào wwwroot/uploads/{thuMuc}. Trả về (đường dẫn web, lỗi).
        /// File rỗng thì trả về (null, null) - nghĩa là người dùng không chọn ảnh.
        /// </summary>
        public static async Task<(string? duongDan, string? loi)> LuuAsync(
            IFormFile? file, IWebHostEnvironment env, string thuMuc)
        {
            if (file == null || file.Length == 0) return (null, null);

            if (file.Length > DungLuongToiDa)
                return (null, "Ảnh quá lớn, tối đa 4 MB.");

            string duoi = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!DuoiChoPhep.Contains(duoi))
                return (null, "Chỉ nhận ảnh có đuôi jpg, jpeg, png, webp, gif hoặc svg.");

            string thuMucDich = Path.Combine(env.WebRootPath, "uploads", thuMuc);
            Directory.CreateDirectory(thuMucDich);

            string tenFile = Guid.NewGuid().ToString("N") + duoi;
            string duongDanDia = Path.Combine(thuMucDich, tenFile);

            using (var stream = new FileStream(duongDanDia, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return ($"/uploads/{thuMuc}/{tenFile}", null);
        }

        /// <summary>
        /// Lưu nhiều file một lượt. Có một file lỗi thì bỏ cả đợt: các file đã lưu dở
        /// được xoá luôn, không để ảnh mồ côi nằm lại trong wwwroot/uploads.
        /// Ô chọn file bỏ trống thì trả về danh sách rỗng và không có lỗi.
        /// </summary>
        public static async Task<(List<string> duongDan, string? loi)> LuuNhieuAsync(
            IEnumerable<IFormFile>? files, IWebHostEnvironment env, string thuMuc)
        {
            var daLuu = new List<string>();

            var cacFile = (files ?? Enumerable.Empty<IFormFile>())
                .Where(f => f != null && f.Length > 0)
                .ToList();

            if (cacFile.Count > SoAnhToiDaMoiLan)
                return (daLuu, $"Mỗi lần chỉ tải tối đa {SoAnhToiDaMoiLan} ảnh.");

            foreach (var f in cacFile)
            {
                var (duongDan, loi) = await LuuAsync(f, env, thuMuc);
                if (loi != null)
                {
                    foreach (var u in daLuu) Xoa(u, env);
                    return (new List<string>(), $"{f.FileName}: {loi}");
                }
                if (duongDan != null) daLuu.Add(duongDan);
            }

            return (daLuu, null);
        }

        /// <summary>
        /// Xoá file ảnh trong wwwroot/uploads theo đường dẫn web. Trả về true nếu đã xoá thật.
        ///
        /// Chỉ đụng vào file nằm trong wwwroot/uploads, tức là file do chính chức năng tải lên tạo ra.
        /// Ảnh mẫu trong wwwroot/images và link ngoài thì bỏ qua. Đường dẫn do trình duyệt gửi lên
        /// nên phải chặn "../" để không xoá được file ngoài thư mục uploads.
        /// </summary>
        public static bool Xoa(string? duongDanWeb, IWebHostEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(duongDanWeb)) return false;
            if (!duongDanWeb.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)) return false;

            string thuMucGoc = Path.GetFullPath(Path.Combine(env.WebRootPath, "uploads"))
                               + Path.DirectorySeparatorChar;

            string tuongDoi = duongDanWeb.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            string duongDanDia = Path.GetFullPath(Path.Combine(env.WebRootPath, tuongDoi));

            if (!duongDanDia.StartsWith(thuMucGoc, StringComparison.OrdinalIgnoreCase)) return false;
            if (!File.Exists(duongDanDia)) return false;

            File.Delete(duongDanDia);
            return true;
        }
    }
}