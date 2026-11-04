using System.Text.Json;

namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Danh sách mã đơn mà phiên trình duyệt hiện tại được phép xem khi KHÔNG đăng nhập:
    /// đơn vừa đặt xong, hoặc đơn vừa tra cứu đúng mã + số điện thoại.
    ///
    /// Nhờ vậy khách lẻ xem được trang đơn (kể cả mã QR) mà người khác không thể
    /// chỉ đoán mã đơn rồi gõ thẳng URL vào xem.
    /// </summary>
    public static class DonKhachLe
    {
        private const string Khoa = "DonKhachLe";
        private const int ToiDa = 20;

        public static void ChoPhep(ISession session, string orderCode)
        {
            var ds = Lay(session);
            ds.Remove(orderCode);
            ds.Insert(0, orderCode);
            if (ds.Count > ToiDa) ds.RemoveRange(ToiDa, ds.Count - ToiDa);
            session.SetString(Khoa, JsonSerializer.Serialize(ds));
        }

        public static bool DuocXem(ISession session, string? orderCode)
            => !string.IsNullOrWhiteSpace(orderCode)
            && Lay(session).Contains(orderCode.Trim(), StringComparer.OrdinalIgnoreCase);

        private static List<string> Lay(ISession session)
        {
            string? json = session.GetString(Khoa);
            if (string.IsNullOrEmpty(json)) return new List<string>();
            try { return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(); }
            catch (JsonException) { return new List<string>(); }
        }
    }
}
