using System.Text.Json;

namespace ShopBanHang.Helpers
{
    /// <summary>Một dòng giỏ của khách chưa đăng nhập, lưu trong Session.</summary>
    public class GuestCartLine
    {
        public int VariantId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Đọc/ghi giỏ của khách chưa đăng nhập trong Session (dạng JSON).
    /// Khi khách đăng nhập, AccountController gộp giỏ này vào giỏ trong CSDL rồi xoá đi.
    /// </summary>
    public static class GioKhachLe
    {
        private const string Khoa = "GioHang";

        public static List<GuestCartLine> Lay(ISession session)
        {
            string? json = session.GetString(Khoa);
            if (string.IsNullOrEmpty(json)) return new List<GuestCartLine>();

            try
            {
                return JsonSerializer.Deserialize<List<GuestCartLine>>(json) ?? new List<GuestCartLine>();
            }
            catch (JsonException)
            {
                // Dữ liệu hỏng thì coi như giỏ trống, không làm sập trang
                return new List<GuestCartLine>();
            }
        }

        public static void Luu(ISession session, List<GuestCartLine> gio)
        {
            if (gio.Count == 0) session.Remove(Khoa);
            else session.SetString(Khoa, JsonSerializer.Serialize(gio));
        }

        public static void Xoa(ISession session) => session.Remove(Khoa);

        public static int Dem(ISession session) => Lay(session).Sum(x => x.Quantity);
    }
}
