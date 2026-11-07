using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ShopBanHang.Controllers
{
    /// <summary>
    /// Đổi ngôn ngữ giao diện. Lựa chọn được lưu vào cookie ".AspNetCore.Culture"
    /// trong 1 năm; middleware UseRequestLocalization (Program.cs) đọc cookie này
    /// ở mỗi request để chọn file Resources/SharedResource.{culture}.resx.
    ///
    ///   GET /NgonNgu/Doi?culture=en-US&amp;returnUrl=/Product
    /// </summary>
    public class NgonNguController : Controller
    {
        private readonly RequestLocalizationOptions _options;

        public NgonNguController(IOptions<RequestLocalizationOptions> options) => _options = options.Value;

        [HttpGet]
        public IActionResult Doi(string culture, string? returnUrl = null)
        {
            // Chỉ nhận ngôn ngữ có trong danh sách hỗ trợ, tránh ghi giá trị lạ vào cookie
            bool hopLe = _options.SupportedUICultures?
                .Any(c => string.Equals(c.Name, culture, StringComparison.OrdinalIgnoreCase)) == true;

            if (hopLe)
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions
                    {
                        Expires = DateTimeOffset.UtcNow.AddYears(1),
                        HttpOnly = true,                 // JavaScript không đọc được
                        IsEssential = true,              // cần cho chức năng, không phải cookie theo dõi
                        SameSite = SameSiteMode.Lax,
                        Secure = Request.IsHttps
                    });
            }

            // Chỉ quay về địa chỉ nội bộ, chặn open redirect (?returnUrl=https://trang-la.com)
            return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl!) : RedirectToAction("Index", "Home");
        }
    }
}
