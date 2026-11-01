using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Đọc chữ trong [Display(Name = ...)] của một giá trị enum.
    /// Nhờ vậy view chỉ cần gọi một hàm, không phải viết switch hay reflection.
    /// </summary>
    public static class EnumHelper
    {
        public static string Nhan(this Enum giaTri)
        {
            FieldInfo? f = giaTri.GetType().GetField(giaTri.ToString());
            var attr = f?.GetCustomAttribute<DisplayAttribute>();
            return attr?.Name ?? giaTri.ToString();
        }
    }
}
