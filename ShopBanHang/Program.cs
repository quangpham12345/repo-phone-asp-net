using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
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

            builder.Services.AddControllersWithViews();

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
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddScoped<IProductVariantService, ProductVariantService>();

            // Giỏ hàng sẽ dùng Session ở giai đoạn sau, đăng ký sẵn từ bây giờ
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(60);
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
            });

            // Xác thực bằng cookie, tự quản hai bảng Users và Roles
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(o =>
                {
                    o.LoginPath = "/Account/Login";
                    o.LogoutPath = "/Account/Logout";
                    o.AccessDeniedPath = "/Account/AccessDenied";
                    o.ExpireTimeSpan = TimeSpan.FromHours(8);
                    o.SlidingExpiration = true;
                    o.Cookie.Name = "ShopBanHang.Auth";
                    o.Cookie.HttpOnly = true;
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            // ===== Pipeline =====

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

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
