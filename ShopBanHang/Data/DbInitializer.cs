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

            db.Database.Migrate();

            SeedRolesVaUsers(db, userManager, roleManager).GetAwaiter().GetResult();
            SeedDanhMucVaThuongHieu(db);
            SeedSanPham(db);
            SeedBanner(db);
            SeedDonHang(db);
            SeedBienThe(db);
            SeedThongSo(db);
            SeedMaGiamGia(db);
        }

        /// <summary>
        /// Sáu đơn mẫu rải đủ 5 trạng thái và rải ra nhiều tháng, để màn quản lý đơn
        /// và phần thống kê có dữ liệu thật mà xem. Đơn đã Hoàn thành thì cộng luôn
        /// SoldCount cho khớp với cách OrderService làm khi đổi trạng thái.
        /// </summary>
        private static void SeedDonHang(ShopContext db)
        {
            if (db.Orders.Any()) return;

            var sp = db.Products.ToList();
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
                    don.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = hang.Id,
                        Quantity = soLuong,
                        UnitPrice = hang.Price   // chốt giá tại thời điểm mua
                    });

                    if (m.Tt == OrderStatus.HoanThanh) hang.SoldCount += soLuong;
                }

                don.Total = don.OrderDetails.Sum(d => d.Quantity * d.UnitPrice);
                db.Orders.Add(don);
                stt++;
            }

            db.SaveChanges();
        }

        /// <summary>Ba banner mẫu cho slideshow trang chủ, ảnh SVG nằm trong wwwroot.</summary>
        private static void SeedBanner(ShopContext db)
        {
            if (db.Banners.Any()) return;

            db.Banners.AddRange(
                new Banner
                {
                    Title = "Điện thoại chính hãng, bảo hành 12 tháng",
                    ImageUrl = "/images/banners/banner-dien-thoai.svg",
                    LinkUrl = "/Product?danhMuc=dien-thoai",
                    SortOrder = 1
                },
                new Banner
                {
                    Title = "Phụ kiện giảm đến 40%",
                    ImageUrl = "/images/banners/banner-phu-kien.svg",
                    LinkUrl = "/Product?danhMuc=phu-kien",
                    SortOrder = 2
                },
                new Banner
                {
                    Title = "Trả góp 0%, duyệt nhanh trong 15 phút",
                    ImageUrl = "/images/banners/banner-tra-gop.svg",
                    LinkUrl = null,          // banner không bấm được, để thử nhánh LinkUrl rỗng
                    SortOrder = 3
                });
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

            if (!db.Brands.Any())
            {
                foreach (string ten in new[] { "Apple", "Samsung", "Xiaomi", "OPPO" })
                {
                    db.Brands.Add(new Brand { Name = ten, Slug = SlugHelper.Tao(ten) });
                }
                db.SaveChanges();
            }

            // Logo mẫu (SVG trong wwwroot/images/brands) cho bốn hãng seed. Chạy cả với database cũ,
            // và chỉ điền khi LogoUrl còn trống nên không đè logo admin đã tải lên.
            var hangSeed = new[] { "apple", "samsung", "xiaomi", "oppo" };
            var chuaCoLogo = db.Brands.Where(b => b.LogoUrl == null && hangSeed.Contains(b.Slug)).ToList();
            foreach (var hang in chuaCoLogo) hang.LogoUrl = $"/images/brands/{hang.Slug}.svg";
            if (chuaCoLogo.Count > 0) db.SaveChanges();
        }

        /// <summary>
        /// Tám sản phẩm mẫu trải đủ 4 danh mục và 3 thương hiệu, có hàng giảm giá,
        /// có hàng hết kho và một hàng đã ẩn - để thử bộ lọc và các trạng thái hiển thị.
        /// Ảnh là file SVG trong wwwroot nên không phụ thuộc mạng.
        /// </summary>
        private static void SeedSanPham(ShopContext db)
        {
            if (db.Products.Any()) return;

            int dienThoai = db.Categories.First(c => c.Slug == "dien-thoai").Id;
            int tablet = db.Categories.First(c => c.Slug == "tablet").Id;
            int laptop = db.Categories.First(c => c.Slug == "laptop").Id;
            int phuKien = db.Categories.First(c => c.Slug == "phu-kien").Id;

            int apple = db.Brands.First(b => b.Slug == "apple").Id;
            int samsung = db.Brands.First(b => b.Slug == "samsung").Id;
            int xiaomi = db.Brands.First(b => b.Slug == "xiaomi").Id;

            // Thứ tự trong danh sách này quan trọng: SeedDonHang chọn sản phẩm theo VỊ TRÍ.
            // Thêm sản phẩm mới thì thêm vào CUỐI danh sách, chèn vào giữa sẽ làm lệch các đơn mẫu.
            var ds = new List<Product>
            {
                // Một sản phẩm, nhiều biến thể (256GB và 512GB, hai màu). Giá, giá gốc và tồn kho ở đây
                // là số tổng hợp; SeedBienThe tính lại từ các biến thể nên hai bên luôn khớp.
                Tao("iPhone 17 Pro Max", 34_990_000m, 37_990_000m, 19, 70,
                    dienThoai, apple, "iphone-17-pro-max", true,
                    "Chip A19 Pro, màn 6.9 inch ProMotion 120Hz, khung titan. Máy chính hãng VN/A.",
                    "8GB", "256GB", "iOS 19", "Titan Tự nhiên"),

                Tao("iPhone 17 128GB", 24_990_000m, null, 20, 31,
                    dienThoai, apple, "iphone-17", true,
                    "Chip A19, màn 6.3 inch, camera kép 48MP. Bản quốc tế, bảo hành 12 tháng.",
                    "8GB", "128GB", "iOS 19", "Đen"),

                Tao("Samsung Galaxy S26 Ultra 512GB", 31_490_000m, 33_990_000m, 8, 27,
                    dienThoai, samsung, "galaxy-s26-ultra", true,
                    "Snapdragon 8 Elite, bút S Pen, camera 200MP, màn Dynamic AMOLED 2X.",
                    "12GB", "512GB", "Android 16", "Xám"),

                Tao("Samsung Galaxy A17 5G 128GB", 5_690_000m, 6_290_000m, 45, 86,
                    dienThoai, samsung, "galaxy-a17-5g", false,
                    "Màn Super AMOLED 6.7 inch, pin 5000mAh, hỗ trợ 5G.",
                    "6GB", "128GB", "Android 15", "Xanh"),

                Tao("Xiaomi 15T 256GB", 12_990_000m, null, 0, 19,
                    dienThoai, xiaomi, "xiaomi-15t", false,
                    "Dimensity 8400 Ultra, camera Leica, sạc nhanh 67W. Hiện đang hết hàng.",
                    "12GB", "256GB", "Android 15", "Đen"),

                Tao("iPad Air M3 11 inch WiFi", 16_990_000m, 18_490_000m, 10, 14,
                    tablet, apple, "ipad-air-m3", true,
                    "Chip M3, màn Liquid Retina 11 inch, hỗ trợ Apple Pencil Pro.",
                    "8GB", "128GB", "iPadOS 19", "Xám"),

                Tao("MacBook Air M4 13 inch 16GB", 27_990_000m, null, 6, 9,
                    laptop, apple, "macbook-air-m4", false,
                    "Chip M4, RAM 16GB, SSD 256GB, pin tới 18 giờ, nặng 1.24kg.",
                    "16GB", "256GB", "macOS Sequoia", "Bạc"),

                Tao("Củ cáp sạc nhanh Xiaomi 67W", 390_000m, 550_000m, 120, 203,
                    phuKien, xiaomi, "cu-sac-67w", false,
                    "Bộ củ cáp sạc nhanh 67W chính hãng.",
                    null, null, null, "Trắng")
            };

            // MacBook Air (vị trí 6) ẩn đi để kiểm chứng phía khách không thấy mà admin vẫn thấy
            ds[6].IsActive = false;

            // Ảnh phụ (gallery). Pro Max có đủ ảnh theo màu và ảnh mặt trước, để biến thể chọn ảnh riêng.
            // S26 Ultra không có biến thể: thử nhánh sản phẩm nhiều ảnh nhưng giá và tồn kho nhập tay.
            ds[0].GalleryUrls = "/images/products/iphone-17-pro-max-tu-nhien.svg;"
                              + "/images/products/iphone-17-pro-max-den.svg;"
                              + "/images/products/iphone-17-pro-max-mat-truoc.svg";
            ds[2].GalleryUrls = "/images/products/galaxy-s26-ultra-mat-sau.svg;"
                              + "/images/products/galaxy-s26-ultra-mat-truoc.svg";

            db.Products.AddRange(ds);
            db.SaveChanges();
        }

        /// <summary>Một dòng cấu hình biến thể mẫu. Gia và GiaGoc để trống thì lấy theo sản phẩm.</summary>
        private sealed record BienTheMau(
            string DungLuong, string MauSac, int Ton,
            decimal? Gia = null, decimal? GiaGoc = null, string? Anh = null, bool DangBan = true);

        /// <summary>
        /// Biến thể mẫu cho 4 điện thoại, đủ các tình huống để thử: nhiều dung lượng với giá khác nhau
        /// (iPhone 17 Pro Max), ảnh riêng theo màu, biến thể đã tắt (iPhone 17 màu Hồng không tính vào
        /// tồn kho), biến thể còn ít hàng và hết hàng. Các sản phẩm còn lại để trống biến thể, dùng để
        /// thử nhánh "giá và tồn kho nhập tay".
        /// </summary>
        private static void SeedBienThe(ShopContext db)
        {
            if (db.ProductVariants.Any()) return;

            const string AnhProMax = "/images/products/iphone-17-pro-max-";

            var cauHinh = new Dictionary<string, BienTheMau[]>
            {
                ["iPhone 17 Pro Max"] = new[]
                {
                    new BienTheMau("256GB", "Titan Tự nhiên", 7, Anh: AnhProMax + "tu-nhien.svg"),
                    new BienTheMau("256GB", "Titan Đen", 5, Anh: AnhProMax + "den.svg"),
                    new BienTheMau("512GB", "Titan Tự nhiên", 3, Gia: 39_990_000m, GiaGoc: 42_990_000m, Anh: AnhProMax + "tu-nhien.svg"),
                    new BienTheMau("512GB", "Titan Đen", 4, Gia: 39_990_000m, GiaGoc: 42_990_000m, Anh: AnhProMax + "den.svg")
                },
                ["iPhone 17 128GB"] = new[]
                {
                    new BienTheMau("128GB", "Đen", 10),
                    new BienTheMau("128GB", "Trắng", 6),
                    new BienTheMau("128GB", "Xanh", 4),
                    new BienTheMau("128GB", "Hồng", 3, DangBan: false)
                },
                ["Samsung Galaxy A17 5G 128GB"] = new[]
                {
                    new BienTheMau("128GB", "Xanh", 25),
                    new BienTheMau("128GB", "Đen", 20)
                },
                ["Xiaomi 15T 256GB"] = new[]
                {
                    new BienTheMau("256GB", "Đen", 0),
                    new BienTheMau("256GB", "Xanh", 0)
                }
            };

            foreach (var (ten, cacBienThe) in cauHinh)
            {
                var sp = db.Products.FirstOrDefault(p => p.Name == ten);
                if (sp == null) continue;

                var ds = cacBienThe.Select(c => new ProductVariant
                {
                    ProductId = sp.Id,
                    Storage = c.DungLuong,
                    Color = c.MauSac,
                    Price = c.Gia ?? sp.Price,
                    OldPrice = c.Gia.HasValue ? c.GiaGoc : sp.OldPrice,
                    Stock = c.Ton,
                    ImageUrl = c.Anh,
                    IsActive = c.DangBan
                }).ToList();
                db.ProductVariants.AddRange(ds);

                // Cùng luật với service: giá thấp nhất, tổng tồn kho của các biến thể đang bán
                var tong = TongHopBienThe.Tinh(ds);
                if (tong != null)
                {
                    sp.Price = tong.Gia;
                    sp.OldPrice = tong.GiaGoc;
                    sp.Stock = tong.TonKho;
                }
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Thông số mẫu cho các sản phẩm seed, để trang chi tiết và bảng so sánh có dữ liệu để xem.
        /// Số liệu chỉ để thử giao diện, không phải thông số chính thức. Chỉ điền cho sản phẩm chưa có
        /// thông số nào, nên không đè lên dữ liệu admin đã nhập. Củ sạc cố tình để trống để thử nhánh thiếu thông số.
        /// </summary>
        private static void SeedThongSo(ShopContext db)
        {
            var thongSo = new Dictionary<string, (string manHinh, string chip, string pin, string camera)>
            {
                ["iPhone 17 Pro Max"] = ("6.9 inch ProMotion 120Hz", "Apple A19 Pro", "4800mAh", "48MP + 48MP + 48MP"),
                ["iPhone 17 128GB"] = ("6.3 inch OLED", "Apple A19", "3700mAh", "48MP + 48MP"),
                ["Samsung Galaxy S26 Ultra 512GB"] = ("6.9 inch Dynamic AMOLED 2X", "Snapdragon 8 Elite", "5000mAh", "200MP + 50MP + 10MP + 50MP"),
                ["Samsung Galaxy A17 5G 128GB"] = ("6.7 inch Super AMOLED", "Exynos 1330", "5000mAh", "50MP + 5MP + 2MP"),
                ["Xiaomi 15T 256GB"] = ("6.83 inch AMOLED 144Hz", "Dimensity 8400 Ultra", "5500mAh", "50MP Leica + 50MP + 12MP"),
                ["iPad Air M3 11 inch WiFi"] = ("11 inch Liquid Retina", "Apple M3", "Dùng tới 10 giờ", "12MP"),
                ["MacBook Air M4 13 inch 16GB"] = ("13.6 inch Liquid Retina", "Apple M4", "Dùng tới 18 giờ", "12MP Center Stage")
            };

            var chuaCo = db.Products
                .Where(p => p.Screen == null && p.Chip == null && p.Battery == null && p.Camera == null)
                .ToList();

            foreach (var sp in chuaCo)
            {
                if (!thongSo.TryGetValue(sp.Name, out var t)) continue;
                sp.Screen = t.manHinh;
                sp.Chip = t.chip;
                sp.Battery = t.pin;
                sp.Camera = t.camera;
            }
            db.SaveChanges();
        }

        /// <summary>
        /// Sáu mã mẫu phủ đủ các trạng thái: đang chạy (giảm % có trần, giảm tiền), sắp diễn ra,
        /// hết hạn, đã tắt, hết lượt - để thử bộ lọc trạng thái và các nhánh báo lỗi khi áp mã.
        /// </summary>
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

        private static Product Tao(string ten, decimal gia, decimal? giaGoc, int tonKho, int daBan,
                                   int idDanhMuc, int idThuongHieu, string anh, bool noiBat, string moTa,
                                   string? ram, string? storage, string? os, string? color)
            => new()
            {
                Name = ten,
                Slug = SlugHelper.Tao(ten),
                Price = gia,
                OldPrice = giaGoc,
                Stock = tonKho,
                SoldCount = daBan,
                CategoryId = idDanhMuc,
                BrandId = idThuongHieu,
                ImageUrl = $"/images/products/{anh}.svg",
                IsFeatured = noiBat,
                IsActive = true,
                Description = moTa,
                Ram = ram,
                Storage = storage,
                Os = os,
                Color = color,
                CreatedAt = DateTime.Now.AddDays(-daBan % 30)
            };
    }
}