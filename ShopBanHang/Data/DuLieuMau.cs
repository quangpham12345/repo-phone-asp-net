namespace ShopBanHang.Data
{
    /// <summary>
    /// Danh mục sản phẩm mẫu theo các mẫu máy có bán ngoài thị trường, để web trông giống cửa hàng thật.
    /// Giá, cấu hình chỉ là SỐ LIỆU MINH HOẠ cho bài tập, không phải giá niêm yết chính thức.
    ///
    /// Mỗi sản phẩm sinh biến thể = (mỗi dung lượng) x (mỗi màu). Giá đi theo dung lượng.
    ///
    /// ẢNH: DbInitializer ưu tiên ảnh đặt trong wwwroot/images/sanpham/{slug}/ (xem HUONG-DAN.md mục 7),
    /// không có thì dùng ảnh minh hoạ (Unsplash) theo loại sản phẩm.
    /// </summary>
    public static class DuLieuMau
    {
        public record GiaMau(string DungLuong, decimal Gia, decimal? GiaGoc = null);

        public record SanPhamMau(
            string Ten, string DanhMuc, string ThuongHieu, bool NoiBat, int DaBan, string MoTa,
            string? Ram, string? Os, string? ManHinh, string? Chip, string? Pin, string? Camera,
            GiaMau[] Gia, string[] Mau,
            bool HetHang = false,          // mọi biến thể tồn kho 0
            bool An = false,               // sản phẩm đã ẩn: khách không thấy, admin thấy
            string? MauNgungBan = null);   // một màu đã tắt (không tính tồn kho, không bán)

        /// <summary>Thương hiệu: tên, màu nền logo.</summary>
        public static readonly (string Ten, string MauLogo)[] ThuongHieu =
        {
            ("Apple", "#111111"), ("Samsung", "#1428A0"), ("Xiaomi", "#FF6900"), ("OPPO", "#0B8A4A"),
            ("vivo", "#415FFF"), ("ASUS", "#00539B"), ("Lenovo", "#E2231A"), ("Dell", "#007DB8"),
            ("Sony", "#000000"), ("Anker", "#00A7E1")
        };

        private const string DT = "dien-thoai", TB = "tablet", LT = "laptop", PK = "phu-kien";

        private static GiaMau[] G(params GiaMau[] g) => g;
        private static GiaMau M(string dl, decimal gia, decimal? goc = null) => new(dl, gia, goc);

        public static readonly SanPhamMau[] SanPham =
        {
            // ===================== ĐIỆN THOẠI =====================
            new("iPhone 17 Pro Max", DT, "Apple", true, 412,
                "Khung nhôm nguyên khối, chip A19 Pro, màn 6.9 inch ProMotion 120Hz, cụm 3 camera 48MP zoom quang 8x. Máy chính hãng VN/A.",
                "12GB", "iOS 26", "6.9 inch Super Retina XDR, ProMotion 120Hz", "Apple A19 Pro", "Xem video tới 37 giờ", "48MP chính + 48MP góc rộng + 48MP tele 4x",
                G(M("256GB", 37_490_000, 37_990_000), M("512GB", 43_990_000, 44_490_000), M("1TB", 50_490_000, 50_990_000)),
                new[] { "Cam Vũ Trụ", "Xanh Đậm", "Bạc" }),

            new("iPhone 17 Pro", DT, "Apple", true, 268,
                "Chip A19 Pro, tản nhiệt buồng hơi, màn 6.3 inch ProMotion, camera tele 48MP. Bản chính hãng VN/A.",
                "12GB", "iOS 26", "6.3 inch Super Retina XDR, ProMotion 120Hz", "Apple A19 Pro", "Xem video tới 31 giờ", "48MP chính + 48MP góc rộng + 48MP tele 4x",
                G(M("256GB", 34_490_000, 34_990_000), M("512GB", 40_990_000, 41_490_000)),
                new[] { "Cam Vũ Trụ", "Xanh Đậm", "Bạc" }),

            new("iPhone Air", DT, "Apple", true, 96,
                "Thiết kế siêu mỏng 5.6mm khung titan, chip A19 Pro, màn 6.5 inch ProMotion.",
                "12GB", "iOS 26", "6.5 inch Super Retina XDR, ProMotion 120Hz", "Apple A19 Pro", "Xem video tới 27 giờ", "48MP Fusion",
                G(M("256GB", 31_490_000, 31_990_000), M("512GB", 37_490_000, 37_990_000)),
                new[] { "Đen Không Gian", "Trắng Mây", "Vàng Nhạt", "Xanh Da Trời" }),

            new("iPhone 17", DT, "Apple", true, 355,
                "Màn 6.3 inch ProMotion 120Hz lần đầu trên bản thường, chip A19, camera kép 48MP, sạc nhanh 40W.",
                "8GB", "iOS 26", "6.3 inch Super Retina XDR, ProMotion 120Hz", "Apple A19", "Xem video tới 30 giờ", "48MP chính + 48MP góc rộng",
                G(M("256GB", 24_490_000, 24_990_000), M("512GB", 30_990_000, 31_490_000)),
                new[] { "Đen", "Trắng", "Xanh Sương Mù", "Tím Oải Hương", "Xanh Lá Xô Thơm" }),

            new("iPhone 16", DT, "Apple", false, 521,
                "Chip A18, nút Camera Control, Apple Intelligence, camera 48MP. Giá tốt sau khi có đời mới.",
                "8GB", "iOS 26", "6.1 inch Super Retina XDR", "Apple A18", "Xem video tới 22 giờ", "48MP chính + 12MP góc rộng",
                G(M("128GB", 19_490_000, 22_990_000), M("256GB", 22_490_000, 25_990_000)),
                new[] { "Đen", "Trắng", "Xanh Mòng Két", "Hồng" }, MauNgungBan: "Hồng"),

            new("iPhone 16e", DT, "Apple", false, 233,
                "iPhone giá dễ tiếp cận với chip A18, Face ID, màn OLED 6.1 inch, pin bền nhất dòng 6.1 inch.",
                "8GB", "iOS 26", "6.1 inch Super Retina XDR OLED", "Apple A18", "Xem video tới 26 giờ", "48MP Fusion",
                G(M("128GB", 15_490_000, 16_990_000), M("256GB", 18_490_000, 19_990_000)),
                new[] { "Đen", "Trắng" }),

            new("iPhone 15", DT, "Apple", false, 640,
                "Dynamic Island, camera 48MP, cổng USB-C. Lựa chọn iPhone tiết kiệm.",
                "6GB", "iOS 26", "6.1 inch Super Retina XDR", "Apple A16 Bionic", "Xem video tới 20 giờ", "48MP chính + 12MP góc rộng",
                G(M("128GB", 16_290_000, 19_990_000)),
                new[] { "Đen", "Hồng", "Xanh Dương" }),

            new("Samsung Galaxy S26 Ultra", DT, "Samsung", true, 287,
                "Khung titan, bút S Pen tích hợp, camera 200MP zoom 100x, Galaxy AI, Snapdragon for Galaxy.",
                "12GB", "Android 16, One UI 8.5", "6.9 inch Dynamic AMOLED 2X, 120Hz", "Snapdragon 8 Elite Gen 5 for Galaxy", "5000mAh, sạc 60W", "200MP + 50MP góc rộng + 50MP tele 5x + 10MP tele 3x",
                G(M("256GB", 32_990_000, 33_990_000), M("512GB", 36_990_000, 37_990_000), M("1TB", 43_990_000, 44_990_000)),
                new[] { "Đen Titan", "Xám Titan", "Xanh Titan" }),

            new("Samsung Galaxy S26 Plus", DT, "Samsung", false, 104,
                "Màn 6.7 inch QHD+, Galaxy AI, camera 50MP, sạc nhanh 45W.",
                "12GB", "Android 16, One UI 8.5", "6.7 inch Dynamic AMOLED 2X, 120Hz", "Snapdragon 8 Elite Gen 5 for Galaxy", "4900mAh, sạc 45W", "50MP + 12MP góc rộng + 10MP tele 3x",
                G(M("256GB", 25_990_000, 26_990_000), M("512GB", 29_990_000, 30_990_000)),
                new[] { "Đen", "Bạc", "Xanh Navy" }),

            new("Samsung Galaxy S26", DT, "Samsung", false, 176,
                "Flagship nhỏ gọn 6.3 inch, Galaxy AI, 7 năm cập nhật hệ điều hành.",
                "12GB", "Android 16, One UI 8.5", "6.3 inch Dynamic AMOLED 2X, 120Hz", "Snapdragon 8 Elite Gen 5 for Galaxy", "4300mAh, sạc 25W", "50MP + 12MP góc rộng + 10MP tele 3x",
                G(M("256GB", 21_990_000, 22_990_000), M("512GB", 25_990_000, 26_990_000)),
                new[] { "Đen", "Bạc", "Xanh Navy", "Tím" }),

            new("Samsung Galaxy Z Fold7", DT, "Samsung", true, 58,
                "Gập mở mỏng nhẹ nhất của Samsung, màn chính 8 inch, camera 200MP.",
                "12GB", "Android 16, One UI 8", "8.0 inch chính + 6.5 inch phụ, Dynamic AMOLED 2X", "Snapdragon 8 Elite for Galaxy", "4400mAh, sạc 25W", "200MP + 12MP góc rộng + 10MP tele 3x",
                G(M("256GB", 44_990_000, 46_990_000), M("512GB", 48_990_000, 50_990_000)),
                new[] { "Xanh Navy", "Bạc", "Đen" }),

            new("Samsung Galaxy Z Flip7", DT, "Samsung", false, 83,
                "Gập vỏ sò, màn ngoài FlexWindow 4.1 inch tràn viền, Exynos 2500.",
                "12GB", "Android 16, One UI 8", "6.9 inch chính + 4.1 inch phụ", "Exynos 2500", "4300mAh, sạc 25W", "50MP + 12MP góc rộng",
                G(M("256GB", 26_990_000, 28_990_000), M("512GB", 30_990_000, 32_990_000)),
                new[] { "Đen", "Xanh Navy", "Đỏ San Hô" }),

            new("Samsung Galaxy S25 FE", DT, "Samsung", false, 147,
                "Flagship giá mềm: Galaxy AI, camera 50MP, chống nước IP68.",
                "8GB", "Android 16, One UI 8", "6.7 inch Dynamic AMOLED 2X, 120Hz", "Exynos 2400", "4900mAh, sạc 45W", "50MP + 12MP góc rộng + 8MP tele 3x",
                G(M("128GB", 13_990_000, 15_990_000), M("256GB", 15_490_000, 17_490_000)),
                new[] { "Xanh Navy", "Trắng", "Đen" }),

            new("Samsung Galaxy A56 5G", DT, "Samsung", false, 302,
                "Khung kim loại, màn 6.7 inch 120Hz, sạc nhanh 45W, 6 năm cập nhật.",
                "8GB", "Android 15, One UI 7", "6.7 inch Super AMOLED, 120Hz", "Exynos 1580", "5000mAh, sạc 45W", "50MP + 12MP góc rộng + 5MP macro",
                G(M("128GB", 9_490_000, 9_990_000), M("256GB", 10_490_000, 10_990_000)),
                new[] { "Đen", "Hồng", "Xám" }),

            new("Samsung Galaxy A17 5G", DT, "Samsung", false, 488,
                "Màn Super AMOLED 6.7 inch, pin 5000mAh, hỗ trợ 5G, giá phổ thông.",
                "6GB", "Android 15, One UI 7", "6.7 inch Super AMOLED, 90Hz", "Exynos 1330", "5000mAh, sạc 25W", "50MP + 5MP + 2MP",
                G(M("128GB", 5_290_000, 5_990_000)),
                new[] { "Đen", "Xanh", "Xám" }),

            new("Xiaomi 15T Pro", DT, "Xiaomi", true, 74,
                "Camera Leica tele 5x, Dimensity 9400+, sạc nhanh 90W, màn 144Hz.",
                "12GB", "Android 15, HyperOS 3", "6.83 inch AMOLED, 144Hz", "MediaTek Dimensity 9400+", "5500mAh, sạc 90W", "50MP Leica + 50MP tele 5x + 12MP góc rộng",
                G(M("512GB", 19_490_000, 19_990_000), M("1TB", 22_490_000, 22_990_000)),
                new[] { "Đen", "Xám", "Vàng Mocha" }),

            new("Xiaomi 15T", DT, "Xiaomi", false, 129,
                "Camera Leica, Dimensity 8400 Ultra, sạc 67W. Hiện đang tạm hết hàng.",
                "12GB", "Android 15, HyperOS 3", "6.83 inch AMOLED, 120Hz", "MediaTek Dimensity 8400 Ultra", "5500mAh, sạc 67W", "50MP Leica + 50MP tele + 12MP góc rộng",
                G(M("256GB", 13_490_000, 13_990_000), M("512GB", 14_990_000, 15_490_000)),
                new[] { "Đen", "Xám", "Hồng" }, HetHang: true),

            new("Xiaomi Redmi Note 14 Pro+ 5G", DT, "Xiaomi", true, 366,
                "Camera 200MP, sạc 120W HyperCharge, màn cong 1.5K, chống nước IP68.",
                "12GB", "Android 14, HyperOS", "6.67 inch AMOLED 1.5K, 120Hz", "Snapdragon 7s Gen 3", "5110mAh, sạc 120W", "200MP + 8MP góc rộng + 2MP macro",
                G(M("256GB", 10_490_000, 11_490_000), M("512GB", 11_990_000, 12_990_000)),
                new[] { "Đen", "Tím", "Xanh Băng" }),

            new("Xiaomi Redmi Note 14", DT, "Xiaomi", false, 712,
                "Màn AMOLED 120Hz, camera 108MP, pin 5500mAh. Bán chạy phân khúc phổ thông.",
                "8GB", "Android 14, HyperOS", "6.67 inch AMOLED, 120Hz", "MediaTek Helio G99 Ultra", "5500mAh, sạc 33W", "108MP + 2MP + 2MP",
                G(M("128GB", 4_690_000, 4_990_000), M("256GB", 5_290_000, 5_590_000)),
                new[] { "Đen", "Xanh Lá", "Tím" }),

            new("OPPO Find X8", DT, "OPPO", false, 61,
                "Camera Hasselblad, Dimensity 9400, sạc nhanh 80W, màn phẳng 6.59 inch.",
                "16GB", "Android 15, ColorOS 15", "6.59 inch AMOLED, 120Hz", "MediaTek Dimensity 9400", "5630mAh, sạc 80W", "50MP Hasselblad + 50MP tele 3x + 50MP góc rộng",
                G(M("512GB", 22_490_000, 22_990_000)),
                new[] { "Đen Vũ Trụ", "Trắng Ngọc Trai" }),

            new("OPPO Reno14 5G", DT, "OPPO", true, 214,
                "Camera tele 50MP chụp chân dung, khung kim loại, chống nước IP69.",
                "12GB", "Android 15, ColorOS 15", "6.59 inch AMOLED, 120Hz", "MediaTek Dimensity 8350", "6000mAh, sạc 80W", "50MP + 50MP tele 3.5x + 8MP góc rộng",
                G(M("256GB", 14_990_000, 15_990_000), M("512GB", 16_990_000, 17_990_000)),
                new[] { "Trắng Ngọc Trai", "Xanh Ngọc", "Xám" }),

            new("OPPO Reno14 F 5G", DT, "OPPO", false, 158,
                "Thiết kế mỏng nhẹ, chống nước IP69, pin 6000mAh.",
                "12GB", "Android 15, ColorOS 15", "6.57 inch AMOLED, 120Hz", "Snapdragon 6 Gen 1", "6000mAh, sạc 45W", "50MP + 8MP góc rộng + 2MP",
                G(M("256GB", 10_190_000, 10_990_000)),
                new[] { "Xanh Lá", "Hồng", "Xanh Dương" }),

            new("OPPO A5 Pro 5G", DT, "OPPO", false, 245,
                "Bền bỉ chuẩn quân đội, chống nước IP69, pin 5800mAh.",
                "8GB", "Android 15, ColorOS 15", "6.67 inch LCD, 120Hz", "MediaTek Dimensity 6300", "5800mAh, sạc 45W", "50MP + 2MP",
                G(M("128GB", 6_490_000, 6_990_000), M("256GB", 7_290_000, 7_790_000)),
                new[] { "Đen", "Nâu" }),

            new("vivo V50", DT, "vivo", false, 132,
                "Camera ZEISS chân dung, màn cong 120Hz, pin 6000mAh, sạc 90W.",
                "12GB", "Android 15, Funtouch OS 15", "6.77 inch AMOLED cong, 120Hz", "Snapdragon 7 Gen 3", "6000mAh, sạc 90W", "50MP ZEISS + 50MP góc rộng",
                G(M("256GB", 12_990_000, 13_990_000), M("512GB", 14_490_000, 15_490_000)),
                new[] { "Xám Titan", "Hồng" }),

            new("vivo Y29", DT, "vivo", false, 199,
                "Pin 6500mAh, chống nước IP64, loa kép lớn.",
                "8GB", "Android 14, Funtouch OS 14", "6.68 inch LCD, 120Hz", "MediaTek Helio G85", "6500mAh, sạc 44W", "50MP + 0.08MP",
                G(M("128GB", 4_490_000, 4_990_000)),
                new[] { "Đen", "Trắng" }),

            // ===================== TABLET =====================
            new("iPad Pro M5 11 inch WiFi", TB, "Apple", true, 37,
                "Chip M5, màn Ultra Retina XDR tandem OLED, mỏng 5.3mm.",
                "12GB", "iPadOS 26", "11 inch Ultra Retina XDR OLED", "Apple M5", "Dùng tới 10 giờ", "12MP",
                G(M("256GB", 28_490_000, 28_990_000), M("512GB", 34_490_000, 34_990_000)),
                new[] { "Đen Không Gian", "Bạc" }),

            new("iPad Air M3 11 inch WiFi", TB, "Apple", true, 141,
                "Chip M3, màn Liquid Retina 11 inch, hỗ trợ Apple Pencil Pro và Magic Keyboard.",
                "8GB", "iPadOS 26", "11 inch Liquid Retina", "Apple M3", "Dùng tới 10 giờ", "12MP",
                G(M("128GB", 16_490_000, 16_990_000), M("256GB", 19_490_000, 19_990_000)),
                new[] { "Xám Không Gian", "Xanh Dương", "Tím", "Ánh Sao" }),

            new("iPad 11 WiFi", TB, "Apple", false, 263,
                "iPad phổ thông với chip A16, màn 11 inch, cổng USB-C.",
                "6GB", "iPadOS 26", "11 inch Liquid Retina", "Apple A16", "Dùng tới 10 giờ", "12MP",
                G(M("128GB", 9_490_000, 9_990_000), M("256GB", 12_290_000, 12_790_000)),
                new[] { "Xanh Dương", "Hồng", "Vàng", "Bạc" }),

            new("iPad mini 7 WiFi", TB, "Apple", false, 54,
                "Nhỏ gọn 8.3 inch, chip A17 Pro, hỗ trợ Apple Pencil Pro.",
                "8GB", "iPadOS 26", "8.3 inch Liquid Retina", "Apple A17 Pro", "Dùng tới 10 giờ", "12MP",
                G(M("128GB", 13_490_000, 13_990_000)),
                new[] { "Xám Không Gian", "Xanh Dương", "Tím" }),

            new("Samsung Galaxy Tab S11", TB, "Samsung", false, 29,
                "Màn Dynamic AMOLED 2X 11 inch, kèm S Pen, Galaxy AI.",
                "12GB", "Android 16, One UI 8", "11 inch Dynamic AMOLED 2X, 120Hz", "MediaTek Dimensity 9400+", "8400mAh, sạc 45W", "13MP",
                G(M("128GB", 20_490_000, 20_990_000), M("256GB", 22_990_000, 23_490_000)),
                new[] { "Xám", "Bạc" }),

            new("Samsung Galaxy Tab S10 FE", TB, "Samsung", false, 88,
                "Màn 10.9 inch 90Hz, kèm S Pen, chống nước IP68.",
                "8GB", "Android 15, One UI 7", "10.9 inch LCD, 90Hz", "Exynos 1580", "8000mAh, sạc 45W", "13MP",
                G(M("128GB", 10_490_000, 10_990_000)),
                new[] { "Xám", "Bạc", "Xanh Dương" }),

            new("Xiaomi Pad 7", TB, "Xiaomi", false, 117,
                "Màn 3.2K 144Hz, Snapdragon 7+ Gen 3, pin 8850mAh.",
                "8GB", "Android 15, HyperOS 2", "11.2 inch LCD 3.2K, 144Hz", "Snapdragon 7+ Gen 3", "8850mAh, sạc 45W", "13MP",
                G(M("128GB", 8_990_000, 9_490_000), M("256GB", 9_990_000, 10_490_000)),
                new[] { "Xám", "Xanh" }),

            new("Xiaomi Redmi Pad 2", TB, "Xiaomi", false, 176,
                "Màn 2.5K 11 inch, pin 9000mAh, giá dễ mua cho học tập.",
                "6GB", "Android 15, HyperOS 2", "11 inch LCD 2.5K, 90Hz", "MediaTek Helio G100 Ultra", "9000mAh, sạc 18W", "8MP",
                G(M("128GB", 4_790_000, 4_990_000)),
                new[] { "Xám", "Xanh Lá" }),

            // ===================== LAPTOP =====================
            new("MacBook Air M4 13 inch", LT, "Apple", true, 183,
                "Chip M4, RAM 16GB, pin tới 18 giờ, nặng 1.24kg, không quạt hoàn toàn yên lặng.",
                "16GB", "macOS", "13.6 inch Liquid Retina", "Apple M4 (10 nhân CPU, 8 nhân GPU)", "Dùng tới 18 giờ", "12MP Center Stage",
                G(M("256GB SSD", 25_990_000, 26_990_000), M("512GB SSD", 31_490_000, 32_490_000)),
                new[] { "Xanh Da Trời", "Bạc", "Ánh Sao", "Đen Xám" }),

            new("MacBook Air M4 15 inch", LT, "Apple", false, 67,
                "Màn lớn 15.3 inch, chip M4, 6 loa, pin tới 18 giờ.",
                "16GB", "macOS", "15.3 inch Liquid Retina", "Apple M4 (10 nhân CPU, 10 nhân GPU)", "Dùng tới 18 giờ", "12MP Center Stage",
                G(M("256GB SSD", 30_990_000, 31_990_000), M("512GB SSD", 36_490_000, 37_490_000)),
                new[] { "Xanh Da Trời", "Bạc", "Đen Xám" }),

            new("MacBook Pro 14 M5", LT, "Apple", true, 42,
                "Chip M5, màn Liquid Retina XDR 14.2 inch, pin tới 24 giờ.",
                "16GB", "macOS", "14.2 inch Liquid Retina XDR", "Apple M5", "Dùng tới 24 giờ", "12MP Center Stage",
                G(M("512GB SSD", 42_490_000, 42_990_000), M("1TB SSD", 47_990_000, 48_490_000)),
                new[] { "Đen Không Gian", "Bạc" }),

            new("MacBook Pro 14 M4 Pro", LT, "Apple", false, 19,
                "Đời trước, đã ngừng kinh doanh (sản phẩm ẩn để thử: khách không thấy, admin vẫn thấy).",
                "24GB", "macOS", "14.2 inch Liquid Retina XDR", "Apple M4 Pro", "Dùng tới 22 giờ", "12MP Center Stage",
                G(M("512GB SSD", 49_990_000, 52_990_000)),
                new[] { "Đen Không Gian" }, An: true),

            new("ASUS Vivobook 15 OLED", LT, "ASUS", false, 158,
                "Màn OLED 15.6 inch, Intel Core Ultra 5, mỏng nhẹ cho sinh viên văn phòng.",
                "16GB", "Windows 11 Home", "15.6 inch OLED FHD, 60Hz", "Intel Core Ultra 5 125H", "Pin 50Wh", "FHD 1080p",
                G(M("512GB SSD", 16_490_000, 17_990_000)),
                new[] { "Bạc", "Xanh Đen" }),

            new("ASUS Zenbook 14 OLED", LT, "ASUS", true, 71,
                "Vỏ nhôm siêu nhẹ 1.2kg, màn OLED 3K 120Hz, Intel Core Ultra 7, có NPU cho AI.",
                "32GB", "Windows 11 Home", "14 inch OLED 3K, 120Hz", "Intel Core Ultra 7 258V", "Pin 72Wh", "FHD IR",
                G(M("1TB SSD", 29_990_000, 31_990_000)),
                new[] { "Xanh Đen", "Trắng Sứ" }),

            new("ASUS ROG Strix G16", LT, "ASUS", false, 44,
                "Laptop gaming RTX 5060, màn 165Hz, tản nhiệt kim loại lỏng.",
                "16GB", "Windows 11 Home", "16 inch FHD+, 165Hz", "Intel Core i7-14650HX + RTX 5060 8GB", "Pin 90Wh", "720p",
                G(M("512GB SSD", 38_990_000, 40_990_000), M("1TB SSD", 41_490_000, 43_490_000)),
                new[] { "Xám Eclipse" }),

            new("Lenovo IdeaPad Slim 5", LT, "Lenovo", false, 96,
                "Màn OLED 2K, vỏ nhôm, Ryzen 7 hiệu năng tốt, pin bền.",
                "16GB", "Windows 11 Home", "14 inch OLED WUXGA", "AMD Ryzen 7 8845HS", "Pin 84Wh", "FHD 1080p",
                G(M("512GB SSD", 17_490_000, 18_990_000)),
                new[] { "Xám", "Xanh" }),

            new("Lenovo Legion 5 Gen 10", LT, "Lenovo", true, 38,
                "Gaming RTX 5070, màn OLED 2.5K 165Hz, tản nhiệt Legion Coldfront.",
                "32GB", "Windows 11 Home", "15.1 inch OLED 2.5K, 165Hz", "AMD Ryzen 7 260 + RTX 5070 8GB", "Pin 80Wh", "FHD 1080p",
                G(M("1TB SSD", 42_990_000, 44_990_000)),
                new[] { "Xám Lunar" }),

            new("Lenovo ThinkPad E14 Gen 6", LT, "Lenovo", false, 52,
                "Bàn phím ThinkPad gõ êm, bền chuẩn quân đội, phù hợp doanh nghiệp.",
                "16GB", "Windows 11 Pro", "14 inch IPS WUXGA", "Intel Core Ultra 5 125U", "Pin 57Wh", "FHD 1080p",
                G(M("512GB SSD", 21_490_000, 22_990_000)),
                new[] { "Đen" }),

            new("Dell Inspiron 14 5440", LT, "Dell", false, 63,
                "Vỏ nhôm gọn nhẹ, Intel Core 7, màn 16:10 phù hợp học tập và làm việc.",
                "16GB", "Windows 11 Home + Office", "14 inch FHD+ 16:10", "Intel Core 7 150U", "Pin 54Wh", "FHD 1080p",
                G(M("512GB SSD", 18_490_000, 19_990_000)),
                new[] { "Bạc", "Xanh Đen" }),

            // ===================== PHỤ KIỆN =====================
            new("AirPods Pro 3", PK, "Apple", true, 377,
                "Chống ồn chủ động mạnh gấp đôi, đo nhịp tim khi tập luyện, chống nước IP57.",
                null, null, null, "Apple H2", "Tới 8 giờ (bật chống ồn)", null,
                G(M("Tiêu chuẩn", 6_490_000, 6_990_000)),
                new[] { "Trắng" }),

            new("AirPods 4 chống ồn chủ động", PK, "Apple", false, 298,
                "Thiết kế mở, chống ồn chủ động, hộp sạc USB-C.",
                null, null, null, "Apple H2", "Tới 5 giờ", null,
                G(M("Tiêu chuẩn", 4_490_000, 4_990_000)),
                new[] { "Trắng" }),

            new("Apple Watch Series 11 GPS 42mm", PK, "Apple", false, 112,
                "Theo dõi huyết áp, giấc ngủ, pin tới 24 giờ, vỏ nhôm.",
                null, "watchOS 26", "42mm LTPO3 OLED", "Apple S11", "Tới 24 giờ", null,
                G(M("42mm", 10_490_000, 10_990_000), M("46mm", 11_490_000, 11_990_000)),
                new[] { "Đen Bóng", "Bạc", "Vàng Hồng" }),

            new("Ốp lưng iPhone 17 Pro Clear Case MagSafe", PK, "Apple", false, 420,
                "Ốp trong suốt chính hãng Apple, hỗ trợ sạc MagSafe.",
                null, null, null, null, null, null,
                G(M("Tiêu chuẩn", 1_390_000, 1_490_000)),
                new[] { "Trong suốt" }),

            new("Cáp sạc Apple USB-C 1m", PK, "Apple", false, 860,
                "Cáp USB-C bện dù chính hãng, sạc và truyền dữ liệu.",
                null, null, null, null, null, null,
                G(M("Tiêu chuẩn", 490_000, 590_000)),
                new[] { "Trắng" }),

            new("Samsung Galaxy Buds3 Pro", PK, "Samsung", false, 156,
                "Chống ồn thích ứng, âm thanh Hi-Fi 24bit, thiết kế thanh.",
                null, null, null, null, "Tới 6 giờ (bật chống ồn)", null,
                G(M("Tiêu chuẩn", 4_290_000, 4_990_000)),
                new[] { "Bạc", "Trắng" }),

            new("Samsung Galaxy Watch8 44mm", PK, "Samsung", false, 73,
                "Đo chỉ số chống oxy hoá, huấn luyện chạy bộ, Gemini trên cổ tay.",
                null, "Wear OS 6, One UI Watch 8", "1.5 inch Super AMOLED", "Exynos W1000", "435mAh", null,
                G(M("44mm", 7_490_000, 7_990_000)),
                new[] { "Bạc", "Đen" }),

            new("Sony WH-1000XM6", PK, "Sony", true, 64,
                "Tai nghe chụp tai chống ồn hàng đầu, 12 micro, pin 30 giờ.",
                null, null, null, "Sony QN3", "Tới 30 giờ", null,
                G(M("Tiêu chuẩn", 9_490_000, 9_990_000)),
                new[] { "Đen", "Bạc", "Xanh Đêm" }),

            new("Sony WF-1000XM5", PK, "Sony", false, 91,
                "Tai nghe true wireless chống ồn, nhỏ gọn, âm thanh Hi-Res.",
                null, null, null, "Sony V2 + QN2e", "Tới 8 giờ", null,
                G(M("Tiêu chuẩn", 4_990_000, 6_490_000)),
                new[] { "Đen", "Bạc" }),

            new("Sạc nhanh Anker Nano 65W 3 cổng", PK, "Anker", false, 534,
                "Công nghệ GaN nhỏ gọn, 2 cổng USB-C + 1 USB-A, sạc được laptop.",
                null, null, null, null, null, null,
                G(M("Tiêu chuẩn", 990_000, 1_190_000)),
                new[] { "Trắng", "Đen" }),

            new("Pin dự phòng Anker 20000mAh 30W", PK, "Anker", false, 465,
                "Sạc nhanh PD 30W, dung lượng 20000mAh sạc iPhone khoảng 4 lần.",
                null, null, null, null, "20000mAh", null,
                G(M("Tiêu chuẩn", 1_090_000, 1_290_000)),
                new[] { "Đen", "Trắng" }),

            new("Củ sạc Xiaomi 67W kèm cáp", PK, "Xiaomi", false, 623,
                "Bộ củ cáp sạc nhanh 67W chính hãng Xiaomi.",
                null, null, null, null, null, null,
                G(M("Tiêu chuẩn", 390_000, 550_000)),
                new[] { "Trắng" }),
        };

        // ===== Ảnh minh hoạ (Unsplash, giấy phép Unsplash cho phép dùng miễn phí) =====
        // Chỉ dùng khi chưa có ảnh thật trong wwwroot/images/sanpham/{slug}/

        public static readonly Dictionary<string, string[]> AnhMinhHoa = new()
        {
            [DT] = new[]
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
            },
            [TB] = new[]
            {
                "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1561154464-82e9adf32764?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1585790050230-5dd28404fbb4?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1544866092-1677b00f868b?auto=format&fit=crop&w=600&h=600&q=80"
            },
            [LT] = new[]
            {
                "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1525547719571-a2d4ac8945f2?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1588872657578-7b1a5aa2b2d5?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1611186871348-b1ce696e52c9?auto=format&fit=crop&w=600&h=600&q=80"
            },
            [PK] = new[]
            {
                "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1572569511254-d8f925fe2cbb?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1583394838336-acd977736f90?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1606220945770-b5b6c2c55bf1?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1484704849700-f032a568e944?auto=format&fit=crop&w=600&h=600&q=80",
                "https://images.unsplash.com/photo-1625883755856-9f5f86dada84?auto=format&fit=crop&w=600&h=600&q=80"
            }
        };

        /// <summary>Banner trang chủ: tiêu đề, ảnh minh hoạ, link. Ảnh thật đặt ở wwwroot/images/banners/1.jpg, 2.jpg...</summary>
        public static readonly (string TieuDe, string Anh, string Link)[] Banner =
        {
            ("iPhone 17 Series – Thu cũ đổi mới, trợ giá đến 3 triệu",
             "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?auto=format&fit=crop&w=1600&h=520&q=80",
             "/Product?thuongHieu=apple&danhMuc=dien-thoai"),
            ("Galaxy S26 Ultra – Camera 200MP, Galaxy AI",
             "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?auto=format&fit=crop&w=1600&h=520&q=80",
             "/Product?thuongHieu=samsung"),
            ("MacBook & laptop – Ưu đãi mùa tựu trường",
             "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?auto=format&fit=crop&w=1600&h=520&q=80",
             "/Product?danhMuc=laptop"),
            ("Phụ kiện chính hãng – Giảm đến 40%",
             "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?auto=format&fit=crop&w=1600&h=520&q=80",
             "/Product?danhMuc=phu-kien")
        };
    }
}
