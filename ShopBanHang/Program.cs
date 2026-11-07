using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Repositories;
using ShopBanHang.Repositories.Interfaces;
using ShopBanHang.Services;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ===== Dịch vụ =====

            // ===== Quốc tế hoá (i18n) =====
            // Chuỗi dịch nằm ở Resources/SharedResource.{vi|en}.resx, khoá là câu tiếng Việt gốc.
            builder.Services.AddLocalization(o => o.ResourcesPath = "Resources");

            builder.Services.AddControllersWithViews()
                // ErrorMessage và Display(Name) trong các annotation cũng được dịch,
                // tra trong cùng file SharedResource
                .AddDataAnnotationsLocalization(o =>
                    o.DataAnnotationLocalizerProvider = (_, factory) => factory.Create(typeof(SharedResource)));

            var cacNgonNgu = new[] { new CultureInfo("vi-VN"), new CultureInfo("en-US") };
            builder.Services.Configure<RequestLocalizationOptions>(o =>
            {
                o.DefaultRequestCulture = new RequestCulture("vi-VN");
                o.SupportedCultures = cacNgonNgu;      // định dạng ngày, số
                o.SupportedUICultures = cacNgonNgu;    // chọn file .resx

                // Thứ tự xác định ngôn ngữ: ?culture=en-US trên URL, rồi tới cookie
                // ".AspNetCore.Culture" do NgonNguController ghi. Bỏ Accept-Language của
                // trình duyệt để trang luôn mặc định tiếng Việt, chỉ đổi khi người dùng bấm chọn.
                o.RequestCultureProviders = new List<IRequestCultureProvider>
                {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider()
                };
            });

            builder.Services.AddDbContext<ShopContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("ShopContext")));

            // Nghiệp vụ nằm ở lớp Service, cả MVC và API cùng gọi vào đây
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<IBannerService, BannerService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IBrandService, BrandService>();
            builder.Services.AddScoped<INewsService, NewsService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<IThongKeService, ThongKeService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IReviewService, ReviewService>();
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddScoped<IProductVariantService, ProductVariantService>();

            // Luồng giỏ hàng - đặt hàng - thanh toán đi theo Controller -> Service -> Repository.
            // Repository và UnitOfWork cùng dùng chung một ShopContext trong mỗi request (Scoped).
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();

            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<ICheckoutService, CheckoutService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();

            // Tài khoản nhận chuyển khoản để tạo mã VietQR, sửa trong appsettings.json
            builder.Services.Configure<ThanhToanOptions>(builder.Configuration.GetSection(ThanhToanOptions.Section));

            // Session (lưu phía server, trình duyệt chỉ giữ cookie mã phiên):
            //   - giỏ hàng của khách chưa đăng nhập      Helpers/GioKhachLe.cs
            //   - các đơn khách vãng lai được phép xem   Helpers/DonKhachLe.cs
            //   - lịch sử từ khoá tìm kiếm trong phiên   Helpers/LichSuTimKiem.cs
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(o =>
            {
                o.Cookie.Name = "ShopBanHang.Session";
                o.IdleTimeout = TimeSpan.FromMinutes(60);
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
            });

            // Xác thực bằng ASP.NET Core Identity.
            // Dùng IdentityUser<int> nên khoá chính vẫn là int, Orders.UserId và
            // ChatConversations.UserId không phải đổi kiểu.
            builder.Services
                .AddIdentity<User, Role>(o =>
                {
                    // Nới quy tắc mật khẩu cho vừa bài tập: tối thiểu 6 ký tự,
                    // không bắt ký tự đặc biệt hay chữ hoa
                    o.Password.RequiredLength = 6;
                    o.Password.RequireNonAlphanumeric = false;
                    o.Password.RequireUppercase = false;
                    o.Password.RequireLowercase = false;
                    o.Password.RequireDigit = false;

                    o.User.RequireUniqueEmail = true;

                    // Sai mật khẩu 5 lần thì khoá tạm 5 phút
                    o.Lockout.MaxFailedAccessAttempts = 5;
                    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                })
                .AddEntityFrameworkStores<ShopContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(o =>
            {
                o.LoginPath = "/Account/Login";
                o.LogoutPath = "/Account/Logout";
                o.AccessDeniedPath = "/Account/AccessDenied";
                o.ExpireTimeSpan = TimeSpan.FromHours(8);
                o.SlidingExpiration = true;
                o.Cookie.Name = "ShopBanHang.Auth";
                o.Cookie.HttpOnly = true;
                o.Cookie.SameSite = SameSiteMode.Lax;

                // Gọi /api/... (hoặc AJAX) mà chưa đăng nhập / không đủ quyền thì trả 401/403 đúng kiểu REST,
                // không chuyển hướng (302) sang trang đăng nhập HTML như với trang thường.
                static bool KhongChuyenHuong(HttpRequest r)
                    => r.Path.StartsWithSegments("/api")
                       || string.Equals(r.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.Ordinal);

                o.Events.OnRedirectToLogin = ctx =>
                {
                    if (KhongChuyenHuong(ctx.Request))
                        ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    else
                        ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
                o.Events.OnRedirectToAccessDenied = ctx =>
                {
                    if (KhongChuyenHuong(ctx.Request))
                        ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
                    else
                        ctx.Response.Redirect(ctx.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            var app = builder.Build();

            // ===== Pipeline =====

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Đặt CultureInfo cho từng request theo URL/cookie, trước routing và MVC
            app.UseRequestLocalization();

            app.UseRouting();

            app.UseSession();

            // Thứ tự bắt buộc: Authentication trước Authorization, cả hai sau UseRouting.
            // Đảo lại là mọi trang có [Authorize] đều bị đẩy về trang đăng nhập.
            app.UseAuthentication();
            app.UseAuthorization();

            // Khu quản trị nằm trong Areas/Admin nên cần route riêng cho area
            app.MapControllerRoute(
                name: "admin",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Cho các API dùng [Route("api/...")] hoạt động
            app.MapControllers();

            // Áp migration còn thiếu và nạp dữ liệu mẫu, để nhóm kéo code về là F5 chạy luôn
            DbInitializer.Initialize(app.Services);

            app.Run();
        }
    }
}
