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
Models/Entities/        các entity, mang Annotation để validate
Models/ViewModels/      dữ liệu cho form và danh sách, không bind trực tiếp entity
Models/Dtos/            hình dạng dữ liệu trả ra API
Models/Enums/           OrderStatus (kèm luật chuyển trạng thái), PaymentMethod
Data/                   ShopContext, DbInitializer, Scripts/ShopBanHang.sql
Services/               nghiệp vụ; cả MVC và API cùng gọi vào đây
Repositories/           truy cập dữ liệu cho luồng giỏ hàng, đơn hàng, tồn kho (+ UnitOfWork)
Helpers/                SlugHelper, TienViet, EnumHelper, ImageStorage, GioKhachLe, ThanhToanOptions...
wwwroot/css/            admin.css cho khu quản trị, style.css cho phía khách
wwwroot/images/         ảnh mẫu sản phẩm và banner (SVG, không phụ thuộc mạng)
wwwroot/uploads/        nơi lưu ảnh admin tải lên
```

Nghiệp vụ nằm ở `Services`. Controller chỉ nhận request, gọi service, chọn cách
trả về — `View()` cho MVC hoặc `Ok()` cho API. Nhờ vậy luật chỉ viết một lần.

## Cơ sở dữ liệu

Bảng nghiệp vụ: `Categories`, `Brands`, `Products`, `ProductVariants`, `Orders`, `OrderDetails`,
`CartItems`, `Coupons`, `Addresses`, `Reviews`, `News`, `Banners`, `ChatConversations`, `ChatMessages`.
Tài khoản dùng ASP.NET Core Identity (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`...).

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
- Mật khẩu do ASP.NET Core Identity băm và quản lý
- `OrderDetails` chép lại `ProductName`, `VariantName`, `UnitPrice` lúc mua; `ProductVariantId` trỏ tới biến thể
  đã trừ kho để huỷ đơn thì hoàn đúng chỗ. Biến thể đã nằm trong đơn không xoá được, chỉ tắt
- Trừ tồn kho bằng một câu `UPDATE ... WHERE Stock >= @n` trong transaction, hai người mua chiếc cuối
  cùng lúc thì chỉ một người được

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

- Cơ sở dữ liệu, migration, script SQL, dữ liệu mẫu
- Đăng nhập (Identity) rẽ nhánh theo vai trò, đăng ký, quên/đặt lại mật khẩu, đổi mật khẩu, sổ địa chỉ
- Trang chủ, danh sách sản phẩm có lọc, chi tiết sản phẩm theo biến thể, so sánh, gợi ý tìm kiếm, đánh giá
- **Giỏ hàng theo biến thể** (`/Cart`): thêm (AJAX), sửa số lượng, xoá, tính tổng, kiểm tra tồn kho.
  Chưa đăng nhập thì giỏ nằm trong Session, đăng nhập thì gộp vào bảng `CartItems`
- **Đặt hàng** (`/Checkout`), **không bắt đăng nhập**: khách vãng lai tự nhập họ tên, số điện thoại,
  địa chỉ, email (đều bắt buộc), đơn lưu với `UserId = null`; khách đã đăng nhập chọn được từ sổ địa chỉ.
  Áp mã giảm giá (AJAX), COD hoặc chuyển khoản QR (VietQR), tạo đơn + trừ tồn kho + cộng lượt mã trong một transaction
- **Đơn hàng của tôi** (`/Order`, cần đăng nhập): danh sách theo trạng thái, chi tiết + tiến trình, huỷ khi còn chờ xác nhận
- **Tra cứu đơn** (`/Order/Lookup`, cho khách vãng lai): nhập mã đơn + số điện thoại để xem đơn, mã QR,
  và huỷ khi còn chờ xác nhận. Trang đơn chỉ mở được trong phiên đã tra cứu đúng (hoặc vừa đặt xong),
  nên biết mã đơn thôi chưa đủ để xem
- **Khu quản trị**: tổng quan thống kê; đơn hàng (lọc theo trạng thái, hình thức, đã/chưa thanh toán,
  khoảng ngày; xác nhận đơn; xác nhận đã thanh toán; chuyển trạng thái; huỷ có lý do và hoàn tồn kho);
  sản phẩm, biến thể, danh mục, thương hiệu, tin tức, banner, mã giảm giá, tài khoản
- `GET /api/users`, `GET /api/users/{id}`

Chưa làm: trang tin tức phía khách, chat hỗ trợ, gửi email thật (email chỉ mới được lưu vào đơn), quốc tế hóa.

## Thanh toán chuyển khoản

Sửa tài khoản nhận tiền trong `appsettings.json`, mục `ThanhToan` (`BankId` theo mã VietQR: MB, VCB, TCB, ACB...).
Sau khi đặt, khách thấy mã QR đã điền sẵn số tiền và nội dung là mã đơn bỏ dấu gạch (`DH202611040001`).
Admin đối soát sao kê rồi bấm **Xác nhận đã thanh toán**; đơn chuyển khoản chưa thanh toán thì chưa giao được.
Đơn COD tự được đánh dấu đã thanh toán khi chuyển sang Hoàn thành.

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

Khi đơn bị **huỷ** (khách tự huỷ lúc Chờ xác nhận, hoặc admin huỷ ở ba bước đầu), tồn kho của
từng biến thể được cộng trả lại và mã giảm giá được trả một lượt.
