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
            SeedBienThe(db);
            SeedDonHang(db);
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


        /// <summary>Banner slideshow – ảnh công nghệ thật.</summary>
        private static void SeedBanner(ShopContext db)
        {
            if (db.Banners.Any()) return;

            db.Banners.AddRange(
                new Banner
                {
                    Title = "iPhone 17 Series – Đặt trước nhận quà",
                    ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?auto=format&fit=crop&w=1400&h=420&q=80",
                    LinkUrl = "/Product?danhMuc=dien-thoai",
                    SortOrder = 1
                },
                new Banner
                {
                    Title = "Galaxy S26 Ultra – Camera AI đột phá",
                    ImageUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?auto=format&fit=crop&w=1400&h=420&q=80",
                    LinkUrl = "/Product?thuongHieu=samsung",
                    SortOrder = 2
                },
                new Banner
                {
                    Title = "Phụ kiện chính hãng – Giảm đến 40%",
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=1400&h=420&q=80",
                    LinkUrl = "/Product?danhMuc=phu-kien",
                    SortOrder = 3
                },
                new Banner
                {
                    Title = "MacBook & iPad – Hiệu năng đỉnh cao",
                    ImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=1400&h=420&q=80",
                    LinkUrl = "/Product?danhMuc=laptop",
                    SortOrder = 4
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

            // Logo dạng data-URI SVG (không phụ thuộc CDN / file local — luôn hiện được)
            var logoOnline = new Dictionary<string, string>
            {
                ["apple"] = "data:image/svg+xml,%3Csvg%20xmlns%3D%22http%3A//www.w3.org/2000/svg%22%20width%3D%2280%22%20height%3D%2228%22%20viewBox%3D%220%200%2080%2028%22%3E%0A%20%20%3Crect%20width%3D%2280%22%20height%3D%2228%22%20rx%3D%226%22%20fill%3D%22%23111111%22/%3E%0A%20%20%3Ctext%20x%3D%2240%22%20y%3D%2219%22%20text-anchor%3D%22middle%22%20font-family%3D%22Arial%2Csans-serif%22%20font-size%3D%2211%22%20font-weight%3D%22700%22%20fill%3D%22%23fff%22%3EApple%3C/text%3E%0A%3C/svg%3E",
                ["samsung"] = "data:image/svg+xml,%3Csvg%20xmlns%3D%22http%3A//www.w3.org/2000/svg%22%20width%3D%2280%22%20height%3D%2228%22%20viewBox%3D%220%200%2080%2028%22%3E%0A%20%20%3Crect%20width%3D%2280%22%20height%3D%2228%22%20rx%3D%226%22%20fill%3D%22%231428A0%22/%3E%0A%20%20%3Ctext%20x%3D%2240%22%20y%3D%2219%22%20text-anchor%3D%22middle%22%20font-family%3D%22Arial%2Csans-serif%22%20font-size%3D%2211%22%20font-weight%3D%22700%22%20fill%3D%22%23fff%22%3ESamsung%3C/text%3E%0A%3C/svg%3E",
                ["xiaomi"] = "data:image/svg+xml,%3Csvg%20xmlns%3D%22http%3A//www.w3.org/2000/svg%22%20width%3D%2280%22%20height%3D%2228%22%20viewBox%3D%220%200%2080%2028%22%3E%0A%20%20%3Crect%20width%3D%2280%22%20height%3D%2228%22%20rx%3D%226%22%20fill%3D%22%23FF6900%22/%3E%0A%20%20%3Ctext%20x%3D%2240%22%20y%3D%2219%22%20text-anchor%3D%22middle%22%20font-family%3D%22Arial%2Csans-serif%22%20font-size%3D%2211%22%20font-weight%3D%22700%22%20fill%3D%22%23fff%22%3EXiaomi%3C/text%3E%0A%3C/svg%3E",
                ["oppo"] = "data:image/svg+xml,%3Csvg%20xmlns%3D%22http%3A//www.w3.org/2000/svg%22%20width%3D%2280%22%20height%3D%2228%22%20viewBox%3D%220%200%2080%2028%22%3E%0A%20%20%3Crect%20width%3D%2280%22%20height%3D%2228%22%20rx%3D%226%22%20fill%3D%22%23000000%22/%3E%0A%20%20%3Ctext%20x%3D%2240%22%20y%3D%2219%22%20text-anchor%3D%22middle%22%20font-family%3D%22Arial%2Csans-serif%22%20font-size%3D%2211%22%20font-weight%3D%22700%22%20fill%3D%22%23fff%22%3EOPPO%3C/text%3E%0A%3C/svg%3E"
            };
            var canCapNhat = db.Brands
                .Where(b => logoOnline.Keys.Contains(b.Slug))
                .ToList();
            foreach (var hang in canCapNhat)
            {
                hang.LogoUrl = logoOnline[hang.Slug];
            }
            if (canCapNhat.Count > 0) db.SaveChanges();
        }

        /// <summary>
        /// 90 sản phẩm mẫu. Ảnh lấy từ pool Unsplash cố định theo loại:
        /// điện thoại / tablet / laptop / phụ kiện — không dùng picsum ngẫu nhiên.
        /// Xóa Products + ProductVariants + Banners (hoặc Drop-Database) rồi F5 để seed lại.
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
            int oppo = db.Brands.First(b => b.Slug == "oppo").Id;

            // Pool ảnh thật theo danh mục (Unsplash – ID cố định, crop 600x600)
            string[] phoneImgs =
            {
                "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1510557880182-3d4d3cba35a5?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1601784551446-20c9e07cdbdb?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1598327105666-5b89351aff97?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1580910051074-3eb694886505?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1523206489230-c012c64b2b48?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1605236453806-6ff36851218e?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1610945415295-d9bbf067e59c?auto=format&fit=crop&w=600&h=600&q=80"
            };
            string[] tabletImgs =
            {
                "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1561154464-82e9adf32764?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1585790050230-5dd28404fbb4?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1544866092-1677b00f868b?auto=format&fit=crop&w=600&h=600&q=80"
            };
            string[] laptopImgs =
            {
                "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1525547719571-a2d4ac8945f2?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1588872657578-7b1a5aa2b2d5?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?auto=format&fit=crop&w=600&h=600&q=80"
            };
            string[] accImgs =
            {
                "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1572569511254-d8f925fe2cbb?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1583394838336-acd977736f90?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1606220945770-b5b6c2c55bf1?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1484704849700-f032a568e944?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1625883755856-9f5f86dada84?auto=format&fit=crop&w=600&h=600&q=80"
            };

            string Pick(string[] pool, int i) => pool[i % pool.Length];
            string Gallery(string[] pool, int i) => string.Join(";", new[]
            {
                Pick(pool, i),
                Pick(pool, i + 1),
                Pick(pool, i + 2)
            });

            var ds = new List<Product>();

            // ===== 20 sản phẩm chính =====
            var core = new (string Ten, int DaBan, int Cat, int Brand, string[] Pool, int ImgIdx, bool Hot, string MoTa, string? Ram, string? Os)[]
            {
                ("iPhone 17 Pro Max", 71, dienThoai, apple, phoneImgs, 1, true, "Chip A19 Pro, màn 6.9 inch ProMotion 120Hz, khung titan.", "8GB", "iOS 19"),
                ("iPhone 17 128GB", 31, dienThoai, apple, phoneImgs, 2, true, "Chip A19, màn 6.3 inch, camera kép 48MP.", "8GB", "iOS 19"),
                ("iPhone 16e 128GB", 45, dienThoai, apple, phoneImgs, 0, false, "Chip A18, màn 6.1 inch OLED, Face ID.", "6GB", "iOS 18"),
                ("Samsung Galaxy S26 Ultra 512GB", 28, dienThoai, samsung, phoneImgs, 8, true, "Snapdragon 8 Elite, S Pen, camera 200MP.", "12GB", "Android 16"),
                ("Samsung Galaxy S25 256GB", 52, dienThoai, samsung, phoneImgs, 9, true, "Snapdragon 8 Gen 3, màn 6.2 inch 120Hz.", "12GB", "Android 15"),
                ("Samsung Galaxy A17 5G 128GB", 86, dienThoai, samsung, phoneImgs, 5, false, "Super AMOLED 6.7 inch, pin 5000mAh, 5G.", "6GB", "Android 15"),
                ("Xiaomi 15T 256GB", 19, dienThoai, xiaomi, phoneImgs, 4, false, "Dimensity 8400 Ultra, camera Leica, sạc 67W.", "12GB", "Android 15"),
                ("Xiaomi Redmi Note 14 Pro 5G", 64, dienThoai, xiaomi, phoneImgs, 3, true, "Camera 200MP, sạc 45W, AMOLED 120Hz.", "8GB", "Android 14"),
                ("OPPO Reno13 5G 256GB", 38, dienThoai, oppo, phoneImgs, 6, true, "Camera AI portrait, sạc 80W, mỏng nhẹ.", "12GB", "Android 15"),
                ("OPPO A3x 64GB", 102, dienThoai, oppo, phoneImgs, 7, false, "Màn 6.67 inch 90Hz, pin 5100mAh.", "4GB", "Android 14"),
                ("iPad Air M3 11 inch WiFi", 14, tablet, apple, tabletImgs, 0, true, "Chip M3, Liquid Retina 11 inch, Apple Pencil Pro.", "8GB", "iPadOS 19"),
                ("iPad 11 128GB WiFi", 22, tablet, apple, tabletImgs, 1, false, "Chip A16, màn 11 inch, USB-C.", "6GB", "iPadOS 18"),
                ("Samsung Galaxy Tab S10 FE", 11, tablet, samsung, tabletImgs, 2, false, "Màn 10.9 inch, hỗ trợ S Pen.", "8GB", "Android 15"),
                ("MacBook Air M4 13 inch 16GB", 9, laptop, apple, laptopImgs, 0, true, "Chip M4, RAM 16GB, SSD 256GB, pin 18 giờ.", "16GB", "macOS Sequoia"),
                ("MacBook Pro 14 M4 Pro", 6, laptop, apple, laptopImgs, 4, true, "Chip M4 Pro, Liquid Retina XDR, Thunderbolt 5.", "24GB", "macOS Sequoia"),
                ("Laptop ASUS Vivobook 15 OLED", 17, laptop, xiaomi, laptopImgs, 1, false, "Intel Core Ultra 5, OLED 15.6 inch, SSD 512GB.", "16GB", "Windows 11"),
                ("Tai nghe AirPods Pro 2", 95, phuKien, apple, accImgs, 3, true, "Chống ồn chủ động, Adaptive Audio, MagSafe USB-C.", null, null),
                ("Tai nghe Sony WH-1000XM5", 33, phuKien, samsung, accImgs, 0, false, "Chống ồn hàng đầu, pin 30 giờ.", null, null),
                ("Củ cáp sạc nhanh Xiaomi 67W", 203, phuKien, xiaomi, accImgs, 5, false, "Bộ củ cáp sạc nhanh 67W chính hãng.", null, null),
                ("Ốp lưng iPhone 17 Pro trong suốt", 150, phuKien, apple, accImgs, 2, false, "Ốp trong suốt chống sốc, hỗ trợ MagSafe.", null, null)
            };

            for (int idx = 0; idx < core.Length; idx++)
            {
                var c = core[idx];
                ds.Add(Tao(c.Ten, c.DaBan, c.Cat, c.Brand, Pick(c.Pool, c.ImgIdx), c.Hot, c.MoTa, c.Ram, c.Os, Gallery(c.Pool, c.ImgIdx)));
            }
            ds[14].IsActive = false; // MacBook Pro ẩn test

            // ===== +70 sản phẩm bổ sung – ảnh đúng loại =====
            var brands = new[] { apple, samsung, xiaomi, oppo };
            var brandNames = new[] { "Apple", "Samsung", "Xiaomi", "OPPO" };
            // 0-2 phone, 3 tablet, 4 laptop, 5 accessory
            var catIds = new[] { dienThoai, dienThoai, dienThoai, tablet, laptop, phuKien };
            var catPools = new[] { phoneImgs, phoneImgs, phoneImgs, tabletImgs, laptopImgs, accImgs };
            var catLabels = new[] { "Điện thoại", "Điện thoại", "Điện thoại", "Tablet", "Laptop", "Phụ kiện" };
            var ramPool = new[] { "4GB", "6GB", "8GB", "12GB", "16GB" };
            var osPool = new[] { "Android 14", "Android 15", "iOS 18", "iOS 19", "Windows 11", "iPadOS 18" };

            for (int i = 1; i <= 70; i++)
            {
                int bi = i % brands.Length;
                int ci = i % catIds.Length;
                var pool = catPools[ci];
                string ten = ci switch
                {
                    0 or 1 or 2 => $"{brandNames[bi]} Phone X{i} 128GB",
                    3 => $"{brandNames[bi]} Tablet T{i} 128GB",
                    4 => $"{brandNames[bi]} Laptop L{i} 16GB",
                    _ => $"{brandNames[bi]} Phụ kiện PK{i}"
                };
                bool hot = i % 7 == 0;
                string? ram = ci == 5 ? null : ramPool[i % ramPool.Length];
                string? os = ci == 5 ? null : osPool[i % osPool.Length];
                string moTa = $"Sản phẩm mẫu #{i} – {catLabels[ci]} {brandNames[bi]}.";
                int daBan = (i * 3 + 11) % 120;

                ds.Add(Tao(ten, daBan, catIds[ci], brands[bi], Pick(pool, i), hot, moTa, ram, os, Gallery(pool, i)));
            }

            db.Products.AddRange(ds);
            db.SaveChanges();
        }

        private sealed record BienTheMau(
            string DungLuong, string MauSac, int Ton,
            decimal Gia, decimal? GiaGoc = null, string? Anh = null, bool DangBan = true);

        private static void SeedBienThe(ShopContext db)
        {
            if (db.ProductVariants.Any()) return;

            // Cấu hình chi tiết cho 20 SP chính
            var cauHinh = new Dictionary<string, BienTheMau[]>
            {
                ["iPhone 17 Pro Max"] = new[]
                {
                    new BienTheMau("256GB", "Titan Tự nhiên", 7, 34_990_000m, 37_990_000m),
                    new BienTheMau("256GB", "Titan Đen", 5, 34_990_000m, 37_990_000m),
                    new BienTheMau("512GB", "Titan Tự nhiên", 3, 39_990_000m, 42_990_000m),
                    new BienTheMau("512GB", "Titan Đen", 4, 39_990_000m, 42_990_000m)
                },
                ["iPhone 17 128GB"] = new[]
                {
                    new BienTheMau("128GB", "Đen", 10, 24_990_000m),
                    new BienTheMau("128GB", "Trắng", 6, 24_990_000m),
                    new BienTheMau("128GB", "Xanh", 4, 24_990_000m),
                    new BienTheMau("128GB", "Hồng", 3, 24_990_000m, DangBan: false)
                },
                ["iPhone 16e 128GB"] = new[]
                {
                    new BienTheMau("128GB", "Đen", 15, 13_490_000m, 16_990_000m),
                    new BienTheMau("128GB", "Trắng", 12, 13_490_000m, 16_990_000m)
                },
                ["Samsung Galaxy S26 Ultra 512GB"] = new[]
                {
                    new BienTheMau("512GB", "Xám Titan", 8, 31_490_000m, 33_990_000m),
                    new BienTheMau("512GB", "Đen", 5, 31_490_000m, 33_990_000m)
                },
                ["Samsung Galaxy S25 256GB"] = new[]
                {
                    new BienTheMau("256GB", "Xanh", 14, 18_990_000m, 22_990_000m),
                    new BienTheMau("256GB", "Bạc", 9, 18_990_000m, 22_990_000m)
                },
                ["Samsung Galaxy A17 5G 128GB"] = new[]
                {
                    new BienTheMau("128GB", "Xanh", 25, 5_690_000m, 6_290_000m),
                    new BienTheMau("128GB", "Đen", 20, 5_690_000m, 6_290_000m)
                },
                ["Xiaomi 15T 256GB"] = new[]
                {
                    new BienTheMau("256GB", "Đen", 0, 12_990_000m),
                    new BienTheMau("256GB", "Xanh", 0, 12_990_000m)
                },
                ["Xiaomi Redmi Note 14 Pro 5G"] = new[]
                {
                    new BienTheMau("256GB", "Đen", 18, 7_490_000m, 8_990_000m),
                    new BienTheMau("256GB", "Xanh", 11, 7_490_000m, 8_990_000m)
                },
                ["OPPO Reno13 5G 256GB"] = new[]
                {
                    new BienTheMau("256GB", "Tím", 10, 11_990_000m, 13_490_000m),
                    new BienTheMau("256GB", "Đen", 8, 11_990_000m, 13_490_000m)
                },
                ["OPPO A3x 64GB"] = new[]
                {
                    new BienTheMau("64GB", "Đen", 40, 3_290_000m, 3_690_000m),
                    new BienTheMau("64GB", "Xanh", 35, 3_290_000m, 3_690_000m)
                },
                ["iPad Air M3 11 inch WiFi"] = new[]
                {
                    new BienTheMau("128GB", "Xám", 10, 16_990_000m, 18_490_000m),
                    new BienTheMau("256GB", "Xám", 6, 19_990_000m, 21_990_000m)
                },
                ["iPad 11 128GB WiFi"] = new[]
                {
                    new BienTheMau("128GB", "Bạc", 12, 9_990_000m, 10_990_000m)
                },
                ["Samsung Galaxy Tab S10 FE"] = new[]
                {
                    new BienTheMau("128GB", "Xám", 8, 11_490_000m, 12_990_000m)
                },
                ["MacBook Air M4 13 inch 16GB"] = new[]
                {
                    new BienTheMau("256GB", "Bạc", 6, 27_990_000m),
                    new BienTheMau("512GB", "Bạc", 3, 32_990_000m)
                },
                ["MacBook Pro 14 M4 Pro"] = new[]
                {
                    new BienTheMau("512GB", "Đen", 4, 48_990_000m, 52_990_000m)
                },
                ["Laptop ASUS Vivobook 15 OLED"] = new[]
                {
                    new BienTheMau("512GB", "Bạc", 9, 18_490_000m, 21_990_000m)
                },
                ["Tai nghe AirPods Pro 2"] = new[]
                {
                    new BienTheMau("Tiêu chuẩn", "Trắng", 50, 5_490_000m, 6_490_000m)
                },
                ["Tai nghe Sony WH-1000XM5"] = new[]
                {
                    new BienTheMau("Tiêu chuẩn", "Đen", 15, 7_990_000m, 9_490_000m)
                },
                ["Củ cáp sạc nhanh Xiaomi 67W"] = new[]
                {
                    new BienTheMau("Tiêu chuẩn", "Trắng", 120, 390_000m, 550_000m)
                },
                ["Ốp lưng iPhone 17 Pro trong suốt"] = new[]
                {
                    new BienTheMau("Tiêu chuẩn", "Trong suốt", 80, 290_000m, 390_000m)
                }
            };

            foreach (var (ten, cacBienThe) in cauHinh)
            {
                var sp = db.Products.FirstOrDefault(p => p.Name == ten);
                if (sp == null) continue;

                db.ProductVariants.AddRange(cacBienThe.Select(c => new ProductVariant
                {
                    ProductId = sp.Id,
                    Storage = c.DungLuong,
                    Color = c.MauSac,
                    Price = c.Gia,
                    OldPrice = c.GiaGoc,
                    Stock = c.Ton,
                    ImageUrl = c.Anh,
                    IsActive = c.DangBan
                }));
            }
            db.SaveChanges();

            // Biến thể tự động cho 70 SP còn lại (chưa có variant)
            var daCoVariant = db.ProductVariants.Select(v => v.ProductId).Distinct().ToHashSet();
            var chuaCo = db.Products.Where(p => !daCoVariant.Contains(p.Id)).ToList();

            var rnd = new Random(42);
            foreach (var sp in chuaCo)
            {
                // Giá theo loại tên
                decimal basePrice;
                string storage;
                if (sp.Name.Contains("Laptop", StringComparison.OrdinalIgnoreCase))
                {
                    basePrice = 15_000_000m + rnd.Next(0, 20) * 500_000m;
                    storage = "512GB";
                }
                else if (sp.Name.Contains("Tablet", StringComparison.OrdinalIgnoreCase))
                {
                    basePrice = 6_000_000m + rnd.Next(0, 15) * 300_000m;
                    storage = "128GB";
                }
                else if (sp.Name.Contains("Phụ kiện", StringComparison.OrdinalIgnoreCase))
                {
                    basePrice = 200_000m + rnd.Next(0, 30) * 50_000m;
                    storage = "Tiêu chuẩn";
                }
                else
                {
                    basePrice = 3_000_000m + rnd.Next(0, 40) * 250_000m;
                    storage = "128GB";
                }

                bool giam = rnd.Next(0, 3) == 0;
                decimal? old = giam ? basePrice + rnd.Next(2, 8) * 200_000m : null;
                int ton = rnd.Next(0, 40);

                db.ProductVariants.Add(new ProductVariant
                {
                    ProductId = sp.Id,
                    Storage = storage,
                    Color = rnd.Next(0, 2) == 0 ? "Đen" : "Xanh",
                    Price = basePrice,
                    OldPrice = old,
                    Stock = ton,
                    IsActive = true
                });

                // Một số SP có thêm biến thể dung lượng
                if (!sp.Name.Contains("Phụ kiện", StringComparison.OrdinalIgnoreCase) && rnd.Next(0, 2) == 0)
                {
                    db.ProductVariants.Add(new ProductVariant
                    {
                        ProductId = sp.Id,
                        Storage = storage == "128GB" ? "256GB" : "1TB",
                        Color = "Đen",
                        Price = basePrice + 2_000_000m,
                        OldPrice = old.HasValue ? old + 2_000_000m : null,
                        Stock = rnd.Next(0, 20),
                        IsActive = true
                    });
                }
            }

            db.SaveChanges();
        }

        private static void SeedThongSo(ShopContext db)
        {
            var thongSo = new Dictionary<string, (string manHinh, string chip, string pin, string camera)>
            {
                ["iPhone 17 Pro Max"] = ("6.9 inch ProMotion 120Hz", "Apple A19 Pro", "4800mAh", "48MP + 48MP + 48MP"),
                ["iPhone 17 128GB"] = ("6.3 inch OLED", "Apple A19", "3700mAh", "48MP + 48MP"),
                ["iPhone 16e 128GB"] = ("6.1 inch OLED", "Apple A18", "3600mAh", "48MP"),
                ["Samsung Galaxy S26 Ultra 512GB"] = ("6.9 inch Dynamic AMOLED 2X", "Snapdragon 8 Elite", "5000mAh", "200MP + 50MP + 10MP + 50MP"),
                ["Samsung Galaxy S25 256GB"] = ("6.2 inch Dynamic AMOLED 2X", "Snapdragon 8 Gen 3", "4000mAh", "50MP + 10MP + 12MP"),
                ["Samsung Galaxy A17 5G 128GB"] = ("6.7 inch Super AMOLED", "Exynos 1330", "5000mAh", "50MP + 5MP + 2MP"),
                ["Xiaomi 15T 256GB"] = ("6.83 inch AMOLED 144Hz", "Dimensity 8400 Ultra", "5500mAh", "50MP Leica + 50MP + 12MP"),
                ["Xiaomi Redmi Note 14 Pro 5G"] = ("6.67 inch AMOLED 120Hz", "Dimensity 7300", "5110mAh", "200MP + 8MP + 2MP"),
                ["OPPO Reno13 5G 256GB"] = ("6.7 inch AMOLED 120Hz", "Dimensity 8350", "5600mAh", "50MP + 8MP"),
                ["OPPO A3x 64GB"] = ("6.67 inch LCD 90Hz", "Helio G85", "5100mAh", "8MP"),
                ["iPad Air M3 11 inch WiFi"] = ("11 inch Liquid Retina", "Apple M3", "Dùng tới 10 giờ", "12MP"),
                ["iPad 11 128GB WiFi"] = ("11 inch Liquid Retina", "Apple A16", "Dùng tới 10 giờ", "12MP"),
                ["Samsung Galaxy Tab S10 FE"] = ("10.9 inch TFT", "Exynos 1580", "8000mAh", "13MP"),
                ["MacBook Air M4 13 inch 16GB"] = ("13.6 inch Liquid Retina", "Apple M4", "Dùng tới 18 giờ", "12MP Center Stage"),
                ["MacBook Pro 14 M4 Pro"] = ("14.2 inch Liquid Retina XDR", "Apple M4 Pro", "Dùng tới 18 giờ", "12MP"),
                ["Laptop ASUS Vivobook 15 OLED"] = ("15.6 inch OLED 120Hz", "Intel Core Ultra 5", "Dùng tới 10 giờ", "1080p")
            };

            var chuaCo = db.Products
                .Where(p => p.Screen == null && p.Chip == null && p.Battery == null && p.Camera == null)
                .ToList();

            foreach (var sp in chuaCo)
            {
                if (thongSo.TryGetValue(sp.Name, out var t))
                {
                    sp.Screen = t.manHinh;
                    sp.Chip = t.chip;
                    sp.Battery = t.pin;
                    sp.Camera = t.camera;
                }
                else
                {
                    // Specs mặc định cho SP seed bổ sung
                    sp.Screen = sp.Screen ?? "6.5 inch FHD+";
                    sp.Chip = sp.Chip ?? "Octa-core";
                    sp.Battery = sp.Battery ?? "5000mAh";
                    sp.Camera = sp.Camera ?? "50MP";
                }
            }
            db.SaveChanges();
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


        private static Product Tao(string ten, int daBan,
                                   int idDanhMuc, int idThuongHieu, string anhUrl, bool noiBat, string moTa,
                                   string? ram, string? os, string? galleryUrls = null)
            => new()
            {
                Name = ten,
                Slug = ShopBanHang.Helpers.SlugHelper.Tao(ten),
                SoldCount = daBan,
                CategoryId = idDanhMuc,
                BrandId = idThuongHieu,
                ImageUrl = anhUrl,
                GalleryUrls = galleryUrls,
                IsFeatured = noiBat,
                IsActive = true,
                Description = moTa,
                Ram = ram,
                Os = os,
                CreatedAt = DateTime.Now.AddDays(-daBan % 30)
            };
    }
}