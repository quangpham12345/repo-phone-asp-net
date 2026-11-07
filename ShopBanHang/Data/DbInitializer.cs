using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;

namespace ShopBanHang.Data
{
    /// <summary>
    /// Áp migration còn thiếu rồi nạp dữ liệu mẫu nếu bảng đang trống.
    /// Nhờ vậy người trong nhóm kéo code về chỉ cần F5, không phải chạy lệnh nào.
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ShopContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Xoá sạch CSDL rồi nạp lại dữ liệu mẫu từ đầu:
            //     dotnet run -- --NapLaiDuLieu=true
            // Chỉ chạy ở môi trường Development để không bao giờ lỡ tay xoá dữ liệu thật.
            if (env.IsDevelopment() && config.GetValue<bool>("NapLaiDuLieu"))
            {
                db.Database.EnsureDeleted();
            }

            db.Database.Migrate();

            SeedRolesVaUsers(db, userManager, roleManager).GetAwaiter().GetResult();
            SeedDanhMucVaThuongHieu(db);
            SeedSanPham(db, env.WebRootPath);
            SeedBanner(db, env.WebRootPath);
            SeedDonHang(db);
            SeedMaGiamGia(db);

            // Mỗi lần khởi động: sản phẩm nào đang dùng ảnh minh hoạ mà đã có ảnh thật trong
            // wwwroot/images/sanpham/{slug}/ thì đổi sang ảnh thật. Chép ảnh vào rồi chạy lại là thấy.
            DongBoAnhThat(db, env.WebRootPath);
        }

        /// <summary>
        /// Sáu đơn mẫu rải đủ 5 trạng thái và rải ra nhiều tháng, để màn quản lý đơn
        /// và phần thống kê có dữ liệu thật mà xem. Đơn đã Hoàn thành thì cộng luôn
        /// SoldCount cho khớp với cách OrderService làm khi đổi trạng thái.
        /// </summary>
        private static void SeedDonHang(ShopContext db)
        {
            if (db.Orders.Any()) return;

            // Include biến thể vì giá của sản phẩm tính từ chúng
            var sp = db.Products.Include(p => p.Variants).ToList();
            if (sp.Count == 0) return;

            int? idKhach1 = db.Users.FirstOrDefault(u => u.Email == "khach1@shop.com")?.Id;

            var mau = new[]
            {
                new { Ten = "Nguyễn Văn Khách", Dt = "0900000002", Dc = "Số 1 Cầu Giấy, Hà Nội",
                      Tt = OrderStatus.HoanThanh, SoNgayTruoc = 95, Uid = idKhach1,
                      Dong = new[] { (0, 1), (7, 2) } },

                new { Ten = "Nguyễn Văn Khách", Dt = "0900000002", Dc = "Số 1 Cầu Giấy, Hà Nội",
                      Tt = OrderStatus.HoanThanh, SoNgayTruoc = 61, Uid = idKhach1,
                      Dong = new[] { (2, 1) } },

                new { Ten = "Trần Thị Hoa", Dt = "0911222333", Dc = "12 Lê Lợi, Đà Nẵng",
                      Tt = OrderStatus.HoanThanh, SoNgayTruoc = 6, Uid = (int?)null,
                      Dong = new[] { (3, 2), (7, 1) } },

                new { Ten = "Lê Minh Tuấn", Dt = "0988777666", Dc = "45 Nguyễn Trãi, TP HCM",
                      Tt = OrderStatus.DangGiao, SoNgayTruoc = 4, Uid = (int?)null,
                      Dong = new[] { (5, 1) } },

                new { Ten = "Phạm Thu Hà", Dt = "0977123456", Dc = "9 Trần Phú, Hải Phòng",
                      Tt = OrderStatus.ChoXacNhan, SoNgayTruoc = 1, Uid = (int?)null,
                      Dong = new[] { (1, 1), (7, 1) } },

                new { Ten = "Đỗ Quang Huy", Dt = "0933444555", Dc = "77 Hùng Vương, Huế",
                      Tt = OrderStatus.DaHuy, SoNgayTruoc = 12, Uid = (int?)null,
                      Dong = new[] { (4, 1) } }
            };

            int stt = 1;
            foreach (var m in mau)
            {
                DateTime ngay = DateTime.Now.AddDays(-m.SoNgayTruoc);

                var don = new Order
                {
                    OrderCode = $"DH{ngay:yyyyMMdd}-{stt:0000}",
                    CustomerName = m.Ten,
                    Phone = m.Dt,
                    Address = m.Dc,
                    Email = null,
                    Status = m.Tt,
                    PaymentMethod = stt % 2 == 0 ? PaymentMethod.ChuyenKhoan : PaymentMethod.COD,
                    CreatedAt = ngay,
                    UserId = m.Uid
                };

                foreach (var (chiSo, soLuong) in m.Dong)
                {
                    var hang = sp[chiSo % sp.Count];

                    // Phân loại rẻ nhất đang bán, chỉ để hoá đơn mẫu có chữ "256GB · Đen" mà xem.
                    // Không gán ProductVariantId: đơn mẫu không trừ kho, nên huỷ cũng không được hoàn kho.
                    var bt = hang.Variants.Where(v => v.IsActive).OrderBy(v => v.Price).FirstOrDefault();

                    don.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = hang.Id,
                        ProductName = hang.Name,
                        VariantName = bt == null ? null : PhanLoai.Ten(bt.Storage, bt.Color),
                        Quantity = soLuong,
                        UnitPrice = hang.DisplayPrice   // chốt giá tại thời điểm mua
                    });

                    if (m.Tt == OrderStatus.HoanThanh) hang.SoldCount += soLuong;
                }

                don.Subtotal = don.OrderDetails.Sum(d => d.Quantity * d.UnitPrice);
                don.Total = don.Subtotal;

                // Đơn đã giao xong coi như đã thu tiền
                if (m.Tt == OrderStatus.HoanThanh)
                {
                    don.IsPaid = true;
                    don.PaidAt = ngay;
                }
                if (m.Tt == OrderStatus.DaHuy)
                {
                    don.CancelledAt = ngay.AddHours(2);
                    don.CancelReason = "Khách đổi ý (dữ liệu mẫu)";
                }

                db.Orders.Add(don);
                stt++;
            }

            db.SaveChanges();
        }


        /// <summary>
        /// Banner trang chủ. Ảnh thật đặt ở wwwroot/images/banners/ (1.jpg, 2.jpg... theo thứ tự),
        /// thiếu thì dùng ảnh minh hoạ trong DuLieuMau.Banner.
        /// </summary>
        private static void SeedBanner(ShopContext db, string webRoot)
        {
            if (db.Banners.Any()) return;

            var anhThat = AnhTrongThuMuc(webRoot, "images/banners");
            for (int i = 0; i < DuLieuMau.Banner.Length; i++)
            {
                var (tieuDe, anh, link) = DuLieuMau.Banner[i];
                db.Banners.Add(new Banner
                {
                    Title = tieuDe,
                    ImageUrl = i < anhThat.Count ? anhThat[i] : anh,
                    LinkUrl = link,
                    SortOrder = i + 1
                });
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Tạo 2 vai trò và 3 tài khoản mẫu qua UserManager để mật khẩu được
        /// băm đúng cách của Identity, và vai trò được ghi vào bảng nối.
        /// </summary>
        private static async Task SeedRolesVaUsers(ShopContext db,
                                                   UserManager<User> userManager,
                                                   RoleManager<Role> roleManager)
        {
            if (!await roleManager.RoleExistsAsync(Role.Admin))
                await roleManager.CreateAsync(new Role(Role.Admin) { Description = "Quản trị hệ thống" });

            if (!await roleManager.RoleExistsAsync(Role.Customer))
                await roleManager.CreateAsync(new Role(Role.Customer) { Description = "Khách hàng" });

            if (db.Users.Any()) return;

            await TaoUser(userManager, "admin@shop.com", "Admin@123", "Quản trị viên",
                          "0900000001", "Hà Nội", Role.Admin, true);

            await TaoUser(userManager, "khach1@shop.com", "Khach@123", "Nguyễn Văn Khách",
                          "0900000002", "Số 1 Cầu Giấy, Hà Nội", Role.Customer, true);

            // Để sẵn một tài khoản bị tắt, tiện thử bộ lọc và thử chặn đăng nhập
            await TaoUser(userManager, "khach2@shop.com", "Khach@123", "Trần Thị Mai",
                          "0900000003", "Số 2 Thanh Xuân, Hà Nội", Role.Customer, false);
        }

        private static async Task TaoUser(UserManager<User> userManager, string email, string matKhau,
                                          string hoTen, string dienThoai, string diaChi,
                                          string vaiTro, bool dangHoatDong)
        {
            var u = new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = hoTen,
                PhoneNumber = dienThoai,
                Address = diaChi,
                IsActive = dangHoatDong,
                CreatedAt = DateTime.Now
            };

            var kq = await userManager.CreateAsync(u, matKhau);
            if (kq.Succeeded) await userManager.AddToRoleAsync(u, vaiTro);
        }

        /// <summary>
        /// Danh mục và thương hiệu nạp trước để giai đoạn làm sản phẩm có sẵn dữ liệu chọn.
        /// </summary>
        private static void SeedDanhMucVaThuongHieu(ShopContext db)
        {
            if (!db.Categories.Any())
            {
                string[] ten = { "Điện thoại", "Tablet", "Laptop", "Phụ kiện" };
                for (int i = 0; i < ten.Length; i++)
                {
                    db.Categories.Add(new Category
                    {
                        Name = ten[i],
                        Slug = SlugHelper.Tao(ten[i]),
                        DisplayOrder = i + 1
                    });
                }
                db.SaveChanges();
            }

            // Thêm hãng còn thiếu (chạy được cả trên CSDL cũ đã có 4 hãng ban đầu)
            var daCo = db.Brands.Select(x => x.Slug).ToHashSet();
            foreach (var (ten, _) in DuLieuMau.ThuongHieu)
            {
                string slug = SlugHelper.Tao(ten);
                if (!daCo.Contains(slug)) db.Brands.Add(new Brand { Name = ten, Slug = slug });
            }
            db.SaveChanges();

            // Logo dạng data-URI SVG (chữ trên nền màu hãng), không phụ thuộc mạng.
            // Chỉ điền cho hãng chưa có logo hoặc đang dùng logo SVG tự sinh, không đè logo admin tải lên.
            var mauLogo = DuLieuMau.ThuongHieu.ToDictionary(x => SlugHelper.Tao(x.Ten), x => x.MauLogo);
            var canLogo = db.Brands.ToList()
                .Where(x => mauLogo.ContainsKey(x.Slug)
                            && (string.IsNullOrEmpty(x.LogoUrl) || x.LogoUrl.StartsWith("data:image/svg")))
                .ToList();
            foreach (var hang in canLogo) hang.LogoUrl = LogoSvg(hang.Name, mauLogo[hang.Slug]);
            if (canLogo.Count > 0) db.SaveChanges();
        }

        private static string LogoSvg(string ten, string mauNen)
        {
            string svg = "<svg xmlns='http://www.w3.org/2000/svg' width='80' height='28' viewBox='0 0 80 28'>"
                       + $"<rect width='80' height='28' rx='6' fill='{mauNen}'/>"
                       + "<text x='40' y='19' text-anchor='middle' font-family='Arial,sans-serif' font-size='11' "
                       + $"font-weight='700' fill='#fff'>{System.Net.WebUtility.HtmlEncode(ten)}</text></svg>";
            return "data:image/svg+xml," + Uri.EscapeDataString(svg);
        }

        /// <summary>
        /// Sản phẩm mẫu theo mẫu máy có thật (Data/DuLieuMau.cs), kèm biến thể dung lượng x màu và thông số.
        /// Thứ tự trong DuLieuMau quan trọng: SeedDonHang chọn sản phẩm theo VỊ TRÍ, và sản phẩm
        /// đứng trước được coi là mới hơn (mục "Sản phẩm mới nhất").
        /// </summary>
        private static void SeedSanPham(ShopContext db, string webRoot)
        {
            if (db.Products.Any()) return;

            var danhMuc = db.Categories.ToDictionary(c => c.Slug, c => c.Id);
            var thuongHieu = db.Brands.ToDictionary(b => b.Slug, b => b.Id);

            int stt = 0;
            foreach (var m in DuLieuMau.SanPham)
            {
                var p = new Product
                {
                    Name = m.Ten,
                    Slug = SlugHelper.Tao(m.Ten),
                    CategoryId = danhMuc[m.DanhMuc],
                    BrandId = thuongHieu[SlugHelper.Tao(m.ThuongHieu)],
                    SoldCount = m.DaBan,
                    IsFeatured = m.NoiBat,
                    IsActive = !m.An,
                    Description = m.MoTa,
                    Ram = m.Ram,
                    Os = m.Os,
                    Screen = m.ManHinh,
                    Chip = m.Chip,
                    Battery = m.Pin,
                    Camera = m.Camera,
                    CreatedAt = DateTime.Now.AddDays(-stt).AddHours(-stt)
                };

                int i = 0;
                foreach (var gia in m.Gia)
                {
                    foreach (var mau in m.Mau)
                    {
                        p.Variants.Add(new ProductVariant
                        {
                            Storage = gia.DungLuong,
                            Color = mau,
                            Price = gia.Gia,
                            OldPrice = gia.GiaGoc > gia.Gia ? gia.GiaGoc : null,
                            Stock = m.HetHang ? 0 : 3 + (stt * 7 + i * 5) % 28,   // tồn kho rải đều, cố định
                            IsActive = mau != m.MauNgungBan
                        });
                        i++;
                    }
                }

                var anhMinhHoa = DuLieuMau.AnhMinhHoa[m.DanhMuc];
                p.ImageUrl = anhMinhHoa[stt % anhMinhHoa.Length];
                GanAnhThat(p, webRoot);

                db.Products.Add(p);
                stt++;
            }
            db.SaveChanges();
        }

        // ===== Ảnh thật do nhóm tự thêm =====
        //
        // wwwroot/images/sanpham/{slug-san-pham}/
        //     chinh.jpg          ảnh đại diện (không có thì lấy file đầu tiên theo tên)
        //     cam-vu-tru.jpg     ảnh riêng của biến thể màu "Cam Vũ Trụ" (tên file = slug của màu)
        //     2.jpg, 3.jpg...    các file còn lại thành ảnh phụ (gallery)
        // Nhận .jpg .jpeg .png .webp .avif

        private static readonly string[] DuoiAnh = { ".jpg", ".jpeg", ".png", ".webp", ".avif" };

        /// <summary>Đường dẫn web của các ảnh trong một thư mục con của wwwroot, sắp theo tên file.</summary>
        private static List<string> AnhTrongThuMuc(string webRoot, string thuMucWeb)
        {
            string dir = Path.Combine(webRoot, thuMucWeb.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(dir)) return new List<string>();

            return Directory.GetFiles(dir)
                .Where(f => DuoiAnh.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .OrderBy(f => Path.GetFileName(f), StringComparer.OrdinalIgnoreCase)
                .Select(f => $"/{thuMucWeb}/{Uri.EscapeDataString(Path.GetFileName(f))}")
                .ToList();
        }

        /// <summary>Có ảnh thật trong thư mục của sản phẩm thì gán vào sản phẩm và biến thể. Trả về true nếu có.</summary>
        private static bool GanAnhThat(Product p, string webRoot)
        {
            var anh = AnhTrongThuMuc(webRoot, $"images/sanpham/{p.Slug}");
            if (anh.Count == 0) return false;

            static string Ten(string url) => Path.GetFileNameWithoutExtension(Uri.UnescapeDataString(url)).ToLowerInvariant();

            string chinh = anh.FirstOrDefault(u => Ten(u) == "chinh") ?? anh[0];
            p.ImageUrl = chinh;

            foreach (var v in p.Variants)
            {
                string? anhMau = anh.FirstOrDefault(u => Ten(u) == SlugHelper.Tao(v.Color));
                if (anhMau != null) v.ImageUrl = anhMau;
            }

            // Ảnh phụ: mọi ảnh trừ ảnh chính, tối đa 10, không vượt độ dài cột GalleryUrls
            var phu = new List<string>();
            foreach (var u in anh.Where(u => u != chinh).Take(10))
            {
                if (string.Join(";", phu.Append(u)).Length > 1000) break;
                phu.Add(u);
            }
            p.GalleryUrls = phu.Count > 0 ? string.Join(";", phu) : null;
            return true;
        }

        /// <summary>
        /// Chạy mỗi lần khởi động. Chỉ đụng tới sản phẩm còn dùng ảnh minh hoạ (Unsplash) hoặc ảnh SVG mẫu cũ,
        /// nên ảnh admin đã tải lên qua trang quản trị không bao giờ bị đè.
        /// </summary>
        private static void DongBoAnhThat(ShopContext db, string webRoot)
        {
            static bool LaAnhMau(string? url) => string.IsNullOrEmpty(url)
                || url.StartsWith("https://images.unsplash.com/", StringComparison.OrdinalIgnoreCase)
                || (url.StartsWith("/images/products/") && url.EndsWith(".svg"))
                || url.StartsWith("/images/sanpham/");

            var dsSanPham = db.Products.Include(p => p.Variants).ToList()
                .Where(p => LaAnhMau(p.ImageUrl))
                .ToList();

            bool doi = false;
            foreach (var p in dsSanPham) doi |= GanAnhThat(p, webRoot);

            static bool LaBannerMau(string url) => url.StartsWith("https://images.unsplash.com/")
                || url.StartsWith("/images/banners/");

            var anhBanner = AnhTrongThuMuc(webRoot, "images/banners");
            var dsBanner = db.Banners.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToList();
            for (int i = 0; i < dsBanner.Count && i < anhBanner.Count; i++)
            {
                if (LaBannerMau(dsBanner[i].ImageUrl) && dsBanner[i].ImageUrl != anhBanner[i])
                {
                    dsBanner[i].ImageUrl = anhBanner[i];
                    doi = true;
                }
            }

            if (doi) db.SaveChanges();
        }

        private static void SeedMaGiamGia(ShopContext db)
        {
            if (db.Coupons.Any()) return;

            DateTime homNay = DateTime.Today;
            DateTime CuoiNgay(DateTime d) => d.Date.AddDays(1).AddSeconds(-1);

            db.Coupons.AddRange(
                new Coupon
                {
                    Code = "GIAM10",
                    Description = "Giảm 10% tối đa 500.000đ cho đơn từ 2 triệu",
                    DiscountType = DiscountType.PhanTram,
                    DiscountValue = 10,
                    MaxDiscount = 500_000m,
                    MinOrderValue = 2_000_000m,
                    UsageLimit = 100,
                    UsedCount = 12,
                    StartDate = homNay.AddDays(-10),
                    EndDate = CuoiNgay(homNay.AddDays(30))
                },
                new Coupon
                {
                    Code = "GIAM200K",
                    Description = "Giảm thẳng 200.000đ cho đơn từ 5 triệu",
                    DiscountType = DiscountType.SoTien,
                    DiscountValue = 200_000m,
                    MinOrderValue = 5_000_000m,
                    UsageLimit = null,
                    StartDate = homNay.AddDays(-5),
                    EndDate = CuoiNgay(homNay.AddDays(60))
                },
                new Coupon
                {
                    Code = "SALE11",
                    Description = "Khuyến mãi 11/11, giảm 15%",
                    DiscountType = DiscountType.PhanTram,
                    DiscountValue = 15,
                    MaxDiscount = 1_000_000m,
                    MinOrderValue = 0,
                    UsageLimit = 50,
                    StartDate = homNay.AddDays(10),
                    EndDate = CuoiNgay(homNay.AddDays(20))
                },
                new Coupon
                {
                    Code = "TETCU",
                    Description = "Mã khuyến mãi đợt trước, đã hết hạn",
                    DiscountType = DiscountType.PhanTram,
                    DiscountValue = 5,
                    MinOrderValue = 0,
                    UsageLimit = 200,
                    UsedCount = 87,
                    StartDate = homNay.AddDays(-90),
                    EndDate = CuoiNgay(homNay.AddDays(-30))
                },
                new Coupon
                {
                    Code = "NOIBO",
                    Description = "Mã nội bộ, đang tắt",
                    DiscountType = DiscountType.SoTien,
                    DiscountValue = 100_000m,
                    MinOrderValue = 0,
                    UsageLimit = 10,
                    IsActive = false,
                    StartDate = homNay.AddDays(-20),
                    EndDate = CuoiNgay(homNay.AddDays(20))
                },
                new Coupon
                {
                    Code = "HETLUOT",
                    Description = "Mã đã dùng hết lượt",
                    DiscountType = DiscountType.SoTien,
                    DiscountValue = 50_000m,
                    MinOrderValue = 0,
                    UsageLimit = 5,
                    UsedCount = 5,
                    StartDate = homNay.AddDays(-15),
                    EndDate = CuoiNgay(homNay.AddDays(15))
                });
            db.SaveChanges();
        }
    }
}