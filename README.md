# ShopBanHang

Bài tập lớn môn Lập trình Web — hệ thống quản lý bán điện thoại di động.
ASP.NET Core MVC (net8.0) + Entity Framework Core + SQL Server.

## Chạy lần đầu

Cần có: .NET SDK 8 trở lên, SQL Server (bản Express cũng được), Visual Studio 2022.

1. Mở `ShopBanHang.sln`
2. Nhấn F5

Không phải chạy lệnh nào khác. Lúc khởi động, `DbInitializer` tự áp migration để
tạo database `ShopBanHang` rồi nạp dữ liệu mẫu.

Nếu SQL Server của bạn không nằm ở `localhost`, sửa `ConnectionStrings:ShopContext`
trong `appsettings.json`:

```json
"Server=localhost;Database=ShopBanHang;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

Dùng tài khoản SQL thay cho Windows auth thì đổi thành:

```json
"Server=localhost;Database=ShopBanHang;User Id=sa;Password=matkhau;TrustServerCertificate=True"
```

## Tài khoản mẫu

| Vai trò | Email | Mật khẩu |
|---|---|---|
| Admin | admin@shop.com | Admin@123 |
| Khách hàng | khach1@shop.com | Khach@123 |
| Khách hàng (đã bị tắt) | khach2@shop.com | Khach@123 |

Dữ liệu mẫu kèm theo: 4 danh mục, 4 thương hiệu, **8 sản phẩm**, **3 banner** trang chủ.

Đăng nhập bằng Admin sẽ vào thẳng khu quản trị `/Admin`, bằng khách hàng thì về trang bán hàng.

## Cấu trúc

```
Areas/Admin/            khu quản trị, tách riêng Controllers + Views + Layout
Controllers/            phía khách
Controllers/Api/        API theo RESTful, trả JSON
ViewComponents/         khối tự lấy dữ liệu riêng (thanh danh mục trên header)
Models/Entities/        11 entity, mang Annotation để validate
Models/ViewModels/      dữ liệu cho form và danh sách, không bind trực tiếp entity
Models/Dtos/            hình dạng dữ liệu trả ra API
Models/Enums/           OrderStatus (kèm luật chuyển trạng thái), PaymentMethod
Data/                   ShopContext, DbInitializer, Scripts/ShopBanHang.sql
Services/               nghiệp vụ; cả MVC và API cùng gọi vào đây
Helpers/                PasswordHelper (PBKDF2), SlugHelper, TienViet, EnumHelper, ImageStorage
wwwroot/css/            admin.css cho khu quản trị, style.css cho phía khách
wwwroot/images/         ảnh mẫu sản phẩm và banner (SVG, không phụ thuộc mạng)
wwwroot/uploads/        nơi lưu ảnh admin tải lên
```

Nghiệp vụ nằm ở `Services`. Controller chỉ nhận request, gọi service, chọn cách
trả về — `View()` cho MVC hoặc `Ok()` cho API. Nhờ vậy luật chỉ viết một lần.

## Cơ sở dữ liệu

11 bảng: `Roles`, `Users`, `Categories`, `Brands`, `Products`, `Orders`,
`OrderDetails`, `News`, `Banners`, `ChatConversations`, `ChatMessages`.

Script để tạo CSDL bằng tay: `ShopBanHang/Data/Scripts/ShopBanHang.sql`
(sinh ở chế độ idempotent nên chạy nhiều lần không lỗi).

Vài quyết định thiết kế cần biết khi đọc code:

- `Orders.UserId` cho phép null — khách không đăng nhập vẫn đặt được hàng
- `OrderDetails.UnitPrice` lưu giá tại thời điểm mua, không join sang `Products`
- `Products` không có cột giá, tồn kho, dung lượng, màu: các thứ này chỉ nằm ở `ProductVariants`.
  Giá hiển thị của sản phẩm là giá thấp nhất trong các biến thể đang bán, tồn kho là tổng các biến thể
  đang bán (`Product.DisplayPrice`, `Product.TotalStock`). Sản phẩm chưa có biến thể thì chưa có giá.
  lấy giá hiện tại, để shop đổi giá thì hoá đơn cũ vẫn đúng
- Tài khoản chỉ **bật/tắt**, không xoá cứng, vì `Orders` đang trỏ tới `Users`
- Giá tiền là `decimal(18,0)` vì VNĐ không có phần xu
- Mật khẩu băm PBKDF2 (SHA-256, 100.000 vòng), mỗi mật khẩu một salt riêng

## Thêm bảng mới

```powershell
# Package Manager Console, Default project = ShopBanHang
Add-Migration TenViecVuaLam
```

Rồi F5 — `DbInitializer` tự áp migration, không cần `Update-Database`.
Phải dừng ứng dụng (Shift+F5) trước khi `Add-Migration`, vì lệnh này build project.

Sau khi thêm migration, xuất lại script để nộp:

```bash
dotnet ef migrations script -o Data/Scripts/ShopBanHang.sql --idempotent
```

## Tiến độ

Đã xong:

- Cơ sở dữ liệu 11 bảng, migration, script SQL, dữ liệu mẫu
- Đăng nhập rẽ nhánh theo vai trò, đăng ký, đổi mật khẩu, trang tài khoản
- Quản lý tài khoản: danh sách có phân trang và lọc, thêm, sửa, bật tắt, đặt lại mật khẩu
- Trang chủ: slideshow banner, sản phẩm nổi bật, sản phẩm mới nhất
- Danh sách sản phẩm: lọc theo danh mục, thương hiệu, khoảng giá, 4 kiểu sắp xếp, phân trang
- Chi tiết sản phẩm kèm sản phẩm cùng danh mục
- **Toàn bộ khu quản trị (back-office)**:
  - Tổng quan kèm thống kê: doanh thu tháng này, doanh thu 12 tháng, đơn theo
    trạng thái, top 5 sản phẩm bán chạy, top 5 khách mua nhiều
  - Đơn hàng: danh sách có lọc theo trạng thái và tra theo mã/tên/điện thoại,
    chi tiết đơn, **chuyển trạng thái theo luồng một chiều**
  - Sản phẩm: thêm, sửa, ẩn hiện, bật tắt nổi bật, tải ảnh lên
  - Danh mục, thương hiệu, tin tức, banner: thêm, sửa, xoá, bật tắt
  - Tài khoản: thêm, sửa, bật tắt, đặt lại mật khẩu
- `GET /api/users`, `GET /api/users/{id}`

Chưa làm: giỏ hàng, đặt hàng, tra cứu đơn phía khách, trang tin tức phía khách,
chat hỗ trợ, quốc tế hóa, AJAX.

## Luồng trạng thái đơn hàng

```
Chờ xác nhận --> Đã xác nhận --> Đang giao --> Hoàn thành
      |                |              |
      +----------------+--------------+--> Đã huỷ
```

Luật nằm ở một chỗ duy nhất: `OrderStatusFlow.BuocTiepTheo` trong
`Models/Enums/OrderStatus.cs`. Màn chi tiết đơn chỉ hiện đúng những nút mà luồng
cho phép, và service kiểm lại lần nữa trước khi lưu — không tin vào nút trên giao diện.

Khi đơn chuyển sang **Hoàn thành**, hệ thống cộng `SoldCount` cho từng sản phẩm
trong đơn. Cộng sớm từ lúc xác nhận thì đơn bị huỷ giữa đường sẽ làm sai thống kê.
