using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Đổi tên có dấu thành chuỗi dùng được trên URL.
    /// "iPhone 17 Pro Max" thành "iphone-17-pro-max".
    /// </summary>
    public static class SlugHelper
    {
        public static string Tao(string chuoi)
        {
            if (string.IsNullOrWhiteSpace(chuoi)) return string.Empty;

            string s = BoDau(chuoi).ToLowerInvariant();
            s = Regex.Replace(s, "[^a-z0-9]+", "-");   // ký tự lạ thành gạch ngang
            return s.Trim('-');
        }

        /// <summary>
        /// Tách dấu thành ký tự tổ hợp rồi loại bỏ. Riêng chữ đ không có dạng tổ hợp
        /// nên phải đổi tay trước, nếu không nó bị giữ nguyên.
        /// </summary>
        private static string BoDau(string chuoi)
        {
            string s = chuoi.Replace("đ", "d").Replace("Đ", "D");

            var sb = new StringBuilder();
            foreach (char c in s.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
