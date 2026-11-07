namespace ShopBanHang
{
    /// <summary>
    /// Lớp rỗng, chỉ làm "mốc" cho bộ quốc tế hoá (i18n).
    ///
    /// IStringLocalizer&lt;SharedResource&gt; tìm chuỗi dịch trong file
    /// Resources/SharedResource.{culture}.resx. Khoá (key) chính là câu tiếng Việt gốc,
    /// nên câu nào chưa có bản dịch thì vẫn hiện tiếng Việt, trang không bao giờ trống chữ.
    ///
    /// Thêm câu mới: viết @L["Câu tiếng Việt"] trong view, rồi thêm một dòng
    /// &lt;data name="Câu tiếng Việt"&gt; vào SharedResource.en.resx.
    /// </summary>
    public class SharedResource
    {
    }
}
