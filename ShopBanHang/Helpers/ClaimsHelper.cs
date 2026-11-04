using System.Security.Claims;

namespace ShopBanHang.Helpers
{
    public static class ClaimsHelper
    {
        /// <summary>Id của người đang đăng nhập, 0 nếu chưa đăng nhập.</summary>
        public static int LayId(this ClaimsPrincipal user)
            => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : 0;
    }
}
