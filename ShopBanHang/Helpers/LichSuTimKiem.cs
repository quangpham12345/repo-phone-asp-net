using System.Text.Json;

namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Các từ khoá khách đã tìm trong PHIÊN hiện tại, lưu trong SESSION (phía server).
    /// Hiện thành các nút "Bạn vừa tìm" trên trang danh sách sản phẩm để bấm tìm lại nhanh.
    ///
    /// Dùng Session vì đây là dữ liệu tạm của một lần ghé: đóng trình duyệt hoặc để yên
    /// quá IdleTimeout (60 phút, cấu hình ở Program.cs) thì tự mất, không cần dọn.
    /// </summary>
    public static class LichSuTimKiem
    {
        private const string Khoa = "LichSuTimKiem";
        private const int ToiDa = 5;

        public static List<string> Lay(ISession session)
        {
            string? json = session.GetString(Khoa);
            if (string.IsNullOrEmpty(json)) return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch (JsonException)
            {
                return new List<string>();
            }
        }

        public static void Them(ISession session, string? tuKhoa)
        {
            tuKhoa = tuKhoa?.Trim();
            if (string.IsNullOrEmpty(tuKhoa) || tuKhoa.Length > 100) return;

            var ds = Lay(session);
            ds.RemoveAll(x => string.Equals(x, tuKhoa, StringComparison.OrdinalIgnoreCase));
            ds.Insert(0, tuKhoa);
            if (ds.Count > ToiDa) ds = ds.Take(ToiDa).ToList();

            session.SetString(Khoa, JsonSerializer.Serialize(ds));
        }

        public static void Xoa(ISession session) => session.Remove(Khoa);
    }
}
