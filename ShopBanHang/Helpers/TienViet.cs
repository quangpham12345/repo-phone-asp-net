using System.Globalization;

namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Định dạng tiền Việt một chỗ duy nhất, để mọi view hiện giống nhau.
    /// 34990000 thành "34.990.000 đ".
    /// </summary>
    public static class TienViet
    {
        /// <summary>
        /// Quy tắc số cố định, KHÔNG dùng culture của tiến trình.
        ///
        /// Để ToString("#,##0") tự lấy culture thì máy chạy en-US sẽ in
        /// "34,990,000" kiểu Mỹ, còn máy vi-VN in "34.990.000" - cùng một
        /// mã nguồn ra hai kiểu khác nhau tuỳ máy. Chốt sẵn ở đây cho chắc.
        /// </summary>
        private static readonly NumberFormatInfo KieuViet = new()
        {
            NumberGroupSeparator = ".",
            NumberDecimalSeparator = ",",
            NumberGroupSizes = new[] { 3 }
        };

        public static string Dinh(decimal tien) => tien.ToString("#,##0", KieuViet) + " đ";

        public static string Dinh(decimal? tien) => tien.HasValue ? Dinh(tien.Value) : "";

        /// <summary>Phần trăm giảm giá, làm tròn xuống. Không giảm thì trả 0.</summary>
        public static int PhanTramGiam(decimal gia, decimal? giaGoc)
        {
            if (!giaGoc.HasValue || giaGoc.Value <= gia || giaGoc.Value <= 0) return 0;
            return (int)Math.Floor((giaGoc.Value - gia) / giaGoc.Value * 100);
        }
    }
}
