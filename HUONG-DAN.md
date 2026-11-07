# Hướng dẫn cài đặt, cấu hình và sử dụng ShopBanHang

Website bán điện thoại di động — bài tập lớn môn Lập trình Web.
Tài liệu này là phần "tài liệu kỹ thuật hướng dẫn cài đặt, cấu hình và script để build cơ sở dữ liệu"
trong danh sách sản phẩm cần nộp.

---

## 1. Công nghệ

| Phần | Dùng gì |
|---|---|
| Nền tảng | ASP.NET Core MVC, .NET 8 |
| Truy cập dữ liệu | Entity Framework Core 8 (Code First + Migrations) |
| Cơ sở dữ liệu | SQL Server (Express / Developer / LocalDB đều được) |
| Giao diện | Bootstrap 5, Bootstrap Icons, jQuery, JavaScript thuần (fetch/AJAX) |
| Tài khoản | ASP.NET Core Identity (bảng `AspNetUsers`, `AspNetRoles`…), cookie authentication |
| Mật khẩu | Identity tự băm (PBKDF2 + salt), không lưu mật khẩu gốc |
| API | ASP.NET Core Web API, RESTful, trả JSON |
| Quốc tế hoá | `Microsoft.Extensions.Localization`, file `.resx` (tiếng Việt, tiếng Anh) |

---

## 2. Cần cài trước

1. **Visual Studio 2022** (17.8 trở lên), có workload *ASP.NET and web development*
2. **.NET 8 SDK** (Visual Studio 2022 thường cài kèm). Kiểm tra: `dotnet --version` ra `8.x` trở lên
3. **SQL Server** bản Express hoặc Developer (miễn phí), hoặc LocalDB đi kèm Visual Studio
4. (Tuỳ chọn) **SQL Server Management Studio** để xem dữ liệu và chạy script SQL bằng tay

---

## 3. Chạy dự án

### Cách 1 — Visual Studio

1. Mở `ShopBanHang.sln`
2. Chọn profile **https** trên thanh công cụ, nhấn **F5**

Khi khởi động, `Data/DbInitializer.cs` tự áp các migration còn thiếu (tạo database `ShopBanHang`
nếu chưa có) rồi nạp dữ liệu mẫu. Không cần chạy lệnh nào khác.

### Cách 2 — dòng lệnh

```bash
cd ShopBanHang
dotnet run --launch-profile https
```

Mở `https://localhost:7127` (hoặc `http://localhost:5074`).

> Ảnh sản phẩm và banner mẫu lấy từ Unsplash nên máy chạy cần có Internet để thấy ảnh.
> Bootstrap, Bootstrap Icons và font cũng tải từ CDN.

---

## 4. Cấu hình

Mọi cấu hình nằm trong `ShopBanHang/appsettings.json`.

### 4.1. Chuỗi kết nối CSDL

Mặc định:

```json
"ConnectionStrings": {
  "ShopContext": "Server=localhost;Database=ShopBanHang;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

| Trường hợp | Chuỗi kết nối |
|---|---|
| SQL Server Express đặt tên instance | `Server=localhost\\SQLEXPRESS;Database=ShopBanHang;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true` |
| LocalDB của Visual Studio | `Server=(localdb)\\MSSQLLocalDB;Database=ShopBanHang;Trusted_Connection=True;MultipleActiveResultSets=true` |
| Tài khoản SQL (sa), cổng khác 1433, Docker | `Server=localhost,1434;Database=ShopBanHang;User Id=sa;Password=matkhau;TrustServerCertificate=True` |

**Không commit chuỗi kết nối riêng của máy mình.** Mỗi thành viên có cấu hình khác nhau thì lưu bằng
User Secrets (nằm ngoài thư mục repo, chỉ có trên máy đó, tự đè lên `appsettings.json` khi chạy Development):

```bash
cd ShopBanHang
dotnet user-secrets set "ConnectionStrings:ShopContext" "Server=localhost,1434;Database=ShopBanHang;User Id=sa;Password=matkhau;TrustServerCertificate=True"
```

Trong Visual Studio: chuột phải project > **Manage User Secrets**.

### 4.2. Tài khoản nhận chuyển khoản (VietQR)

```json
"ThanhToan": {
  "BankId": "MB",              // mã ngân hàng theo VietQR: MB, VCB, TCB, ACB...
  "BankName": "MB Bank",
  "AccountNo": "0000000000",
  "AccountName": "SHOP BAN HANG",
  "Template": "compact2"
}
```

### 4.3. Các thông số cố định trong code

| Thông số | Giá trị | Ở đâu |
|---|---|---|
| Thời gian giữ Session khi không thao tác | 60 phút | `Program.cs` (`AddSession`) |
| Thời gian giữ đăng nhập | 8 giờ, tự gia hạn | `Program.cs` (`ConfigureApplicationCookie`) |
| Khoá tài khoản khi sai mật khẩu | 5 lần, khoá 5 phút | `Program.cs` (`Lockout`) |
| Ngôn ngữ hỗ trợ | `vi-VN` (mặc định), `en-US` | `Program.cs` (`RequestLocalizationOptions`) |
| Ngưỡng cảnh báo tồn kho thấp | xem `Helpers/NguongTonKho.cs` | |

---

## 5. Cơ sở dữ liệu

### 5.1. Ba cách tạo CSDL

| Cách | Làm gì | Khi nào dùng |
|---|---|---|
| **Tự động** | Nhấn F5. `DbInitializer` gọi `Database.Migrate()` rồi nạp dữ liệu mẫu | Mặc định, dễ nhất |
| **Script SQL** | Chạy `ShopBanHang/Data/Scripts/ShopBanHang.sql` (xem 5.2) | Chấm bài, tạo CSDL không cần Visual Studio |
| **EF Core** | Package Manager Console: `Update-Database` | Khi đang phát triển |

Cách tạo bằng script chỉ tạo **bảng rỗng**. Dữ liệu mẫu vẫn do `DbInitializer` nạp ở lần chạy web đầu tiên
(nó thấy bảng trống thì nạp, có dữ liệu rồi thì bỏ qua).

### 5.2. Chạy script SQL

Script sinh ở chế độ *idempotent*: mỗi migration được bọc trong điều kiện kiểm tra bảng
`__EFMigrationsHistory`, nên chạy trên CSDL trống hay CSDL đã có một phần đều được, chạy lại nhiều lần không lỗi.

Bằng SSMS: tạo database rỗng tên `ShopBanHang`, mở file `ShopBanHang.sql`, chọn database đó, nhấn **Execute**.

Bằng dòng lệnh:

```bash
sqlcmd -S localhost -E -Q "IF DB_ID('ShopBanHang') IS NULL CREATE DATABASE ShopBanHang"
sqlcmd -S localhost -E -d ShopBanHang -i ShopBanHang/Data/Scripts/ShopBanHang.sql
```

(Dùng tài khoản SQL thì thay `-E` bằng `-U sa -P matkhau`.)

### 5.3. Sinh lại script SQL (bắt buộc sau mỗi lần thêm migration)

File `.sql` **không** tự cập nhật. Thêm migration mới xong phải sinh lại rồi commit:

```powershell
# Windows, tại thư mục gốc repo
powershell -ExecutionPolicy Bypass -File scripts\xuat-script-sql.ps1
```

```bash
# macOS / Linux
bash scripts/xuat-script-sql.sh
```

Script này chạy `dotnet tool restore` (cài `dotnet-ef` đúng phiên bản ghi trong `.config/dotnet-tools.json`),
build project, rồi chạy `dotnet ef migrations script --idempotent`.

### 5.4. Thêm bảng / sửa cấu trúc

```powershell
# Package Manager Console, Default project = ShopBanHang. Dừng ứng dụng (Shift+F5) trước.
Add-Migration TenViecVuaLam
```

Nhấn F5 là migration được áp. Sau đó chạy lại mục 5.3.

### 5.5. Danh sách bảng

| Nhóm | Bảng |
|---|---|
| Sản phẩm | `Categories`, `Brands`, `Products`, `ProductVariants`, `Reviews` |
| Bán hàng | `CartItems`, `Orders`, `OrderDetails`, `Coupons` |
| Khách hàng | `Addresses` |
| Nội dung | `News`, `Banners`, `ChatConversations`, `ChatMessages` |
| Tài khoản (Identity) | `AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, `AspNetUserClaims`, `AspNetUserLogins`, `AspNetUserTokens`, `AspNetRoleClaims` |
| Hệ thống | `__EFMigrationsHistory` |

Ràng buộc quan trọng: xem mục "Cơ sở dữ liệu" trong `README.md`.

### 5.6. Xoá CSDL làm lại từ đầu

```powershell
# Package Manager Console
Drop-Database
```

Rồi F5, CSDL và dữ liệu mẫu được tạo lại.

---

## 6. Tài khoản để thử

| Vai trò | Email | Mật khẩu | Đăng nhập xong vào đâu |
|---|---|---|---|
| Quản trị | `admin@shop.com` | `Admin@123` | Khu quản trị `/Admin` |
| Khách hàng | `khach1@shop.com` | `Khach@123` | Trang bán hàng `/` |
| Khách bị tắt | `khach2@shop.com` | `Khach@123` | Không vào được (để thử) |

> Đây là tài khoản demo. Triển khai thật thì đổi mật khẩu ngay.

---

## 7. Dữ liệu mẫu

4 danh mục · 4 thương hiệu · **90 sản phẩm** (20 sản phẩm chính có biến thể chi tiết + 70 sản phẩm bổ sung để thử phân trang)
· biến thể giá/màu/dung lượng · 4 banner · 6 đơn hàng đủ 5 trạng thái · 4 mã giảm giá (`GIAM10`, `GIAM200K`, `SALE11`, `TETCU`).

Có hàng giảm giá, hàng hết kho, một sản phẩm đã ẩn (MacBook Pro 14 — khách không thấy, admin thấy),
biến thể đã tắt, đơn của khách đăng nhập và đơn của khách vãng lai.

---

## 8. Đối chiếu yêu cầu đề bài

| Yêu cầu | Thực hiện | Xem ở đâu |
|---|---|---|
| Nhiều lớp người dùng | Admin, khách hàng đăng nhập, khách vãng lai | `Areas/Admin` có `[Authorize(Roles = Role.Admin)]` |
| Template giao diện riêng | Layout khách `hh-*`, layout admin riêng | `Views/Shared/_Layout.cshtml`, `wwwroot/css/style.css`, `Areas/Admin/Views/Shared` |
| Responsive | Bootstrap 5 grid + media query | `style.css` |
| Paging | Danh sách sản phẩm, đơn của tôi, các danh sách admin, API | `Models/ViewModels/PagedResult.cs` |
| Filtering | Lọc sản phẩm (danh mục, hãng, giá, RAM, dung lượng, OS, sắp xếp), lọc đơn | `ProductService.LayDanhSachAsync`, `OrderService` |
| AJAX | Gợi ý tìm kiếm, thêm vào giỏ, áp mã giảm giá | `wwwroot/js/site.js`, `Views/Product/Detail.cshtml`, `Views/Checkout/Index.cshtml` |
| Thêm / sửa / xoá | Sản phẩm, biến thể, danh mục, thương hiệu, banner, tin tức, mã giảm giá, tài khoản, địa chỉ, đánh giá | `Areas/Admin/Controllers` |
| Session | Giỏ hàng khách vãng lai, đơn được phép xem, lịch sử tìm kiếm | mục 9 |
| Cookie | Đăng nhập, ngôn ngữ, sản phẩm đã xem | mục 9 |
| Annotation | Validate entity, ViewModel, DTO API; được dịch theo ngôn ngữ | `Models/Entities`, `Models/ViewModels`, `Models/Dtos` |
| Web API RESTful | `/api/products` đủ GET/POST/PUT/DELETE, `/api/users`, `/api/categories`, `/api/brands` | mục 10 |
| Security | Xem mục 12 | |
| Quốc tế hoá | Việt / Anh, chọn trên thanh trên cùng | mục 11 |
| Script build CSDL | `Data/Scripts/ShopBanHang.sql` + script sinh lại | mục 5 |

---

## 9. Session và Cookie

### Session (dữ liệu ở server, trình duyệt chỉ giữ mã phiên)

| Khoá Session | Lưu gì | Code |
|---|---|---|
| `GioHang` | Giỏ hàng của khách chưa đăng nhập (JSON). Đăng nhập thì gộp vào bảng `CartItems` | `Helpers/GioKhachLe.cs`, `CartController`, `AccountController` |
| (đơn khách lẻ) | Các mã đơn khách vãng lai vừa đặt / vừa tra cứu đúng, để chỉ người đó xem được đơn | `Helpers/DonKhachLe.cs`, `OrderController` |
| `LichSuTimKiem` | 5 từ khoá tìm gần nhất trong phiên, hiện thành nút "Bạn vừa tìm" ở `/Product` | `Helpers/LichSuTimKiem.cs`, `ProductController.Index` |

Hết hạn sau 60 phút không thao tác. Cookie mã phiên tên `ShopBanHang.Session`, `HttpOnly`.

### Cookie

| Tên cookie | Lưu gì | Thời hạn | Code |
|---|---|---|---|
| `ShopBanHang.Auth` | Phiếu đăng nhập (đã mã hoá bởi Data Protection) | 8 giờ, tự gia hạn; tick "Ghi nhớ đăng nhập" thì vẫn giữ khi đóng trình duyệt | `Program.cs` |
| `.AspNetCore.Culture` | Ngôn ngữ đã chọn, ví dụ `c=en-US\|uic=en-US` | 1 năm | `Controllers/NgonNguController.cs` |
| `ShopBanHang.DaXem` | Tối đa 10 id sản phẩm đã xem gần đây, dạng `12-5-33` | 30 ngày | `Helpers/CookieDaXem.cs`, `ProductController.Detail`, `ViewComponents/DaXemGanDayViewComponent.cs` |
| `ShopBanHang.Session` | Mã phiên của Session | Hết phiên | `Program.cs` |
| `.AspNetCore.Antiforgery.*` | Token chống CSRF | Hết phiên | ASP.NET Core tự tạo |

Mọi cookie tự tạo đều đặt `HttpOnly`, `SameSite=Lax`, `Secure` khi chạy HTTPS. Cookie `DaXem` do người dùng
sửa được nên khi đọc phải kiểm tra lại từng id (không tin dữ liệu phía client).

Thử: mở vài sản phẩm, đóng trình duyệt, mở lại trang chủ — khối "Sản phẩm bạn đã xem" vẫn còn (cookie),
trong khi "Bạn vừa tìm" đã mất (session).

---

## 10. Web API RESTful

| Method | URL | Quyền | Kết quả |
|---|---|---|---|
| GET | `/api/products?tuKhoa=&danhMuc=&thuongHieu=&giaTu=&giaDen=&ram=&storage=&os=&sapXep=&page=&pageSize=` | Mọi người | 200 + `{ page, pageSize, totalItems, totalPages, items[] }` |
| GET | `/api/products/{id}` | Mọi người | 200 kèm biến thể / 404 |
| POST | `/api/products` | Admin | 201 + header `Location` / 400 / 401 / 403 |
| PUT | `/api/products/{id}` | Admin | 204 / 400 / 404 / 401 / 403 |
| DELETE | `/api/products/{id}` | Admin | 204 (xoá mềm: ẩn sản phẩm) / 404 / 401 / 403 |
| GET | `/api/categories` | Mọi người | 200 |
| GET | `/api/brands` | Mọi người | 200 |
| GET | `/api/users?tuKhoa=&roleId=&isActive=&page=&pageSize=` | Admin | 200 |
| GET | `/api/users/{id}` | Admin | 200 / 404 |

Nguyên tắc REST đã theo: URL là danh từ chỉ tài nguyên (`/api/products/5`), hành động là HTTP method,
kết quả là mã trạng thái chuẩn; dữ liệu trả ra là DTO (không lộ entity, không lộ `PasswordHash`);
lỗi validate trả 400 dạng ProblemDetails (RFC 7807) nhờ `[ApiController]` + annotation trên `ProductWriteDto`;
`DELETE` gọi lại nhiều lần vẫn trả 204 (idempotent).

API dùng chung cookie đăng nhập với website. Gọi `/api/...` khi chưa đăng nhập trả **401**,
không đủ quyền trả **403** (không chuyển hướng sang trang đăng nhập như trang thường).

**Gọi thử:** mở `ShopBanHang/ShopBanHang.http` trong Visual Studio, bấm *Send request*.
Hướng dẫn lấy cookie Admin ghi ngay đầu file. Có thể dùng Postman với header
`Cookie: ShopBanHang.Auth=...` tương tự.

Code: `Controllers/Api/ProductsController.cs`, `CatalogController.cs`, `UsersController.cs`, `Models/Dtos/`.

---

## 11. Quốc tế hoá (i18n)

**Cách hoạt động**

1. `Program.cs` đăng ký `AddLocalization`, hai ngôn ngữ `vi-VN` (mặc định) và `en-US`, và middleware
   `UseRequestLocalization`.
2. Ngôn ngữ của mỗi request lấy theo thứ tự: tham số `?culture=en-US` trên URL, rồi cookie
   `.AspNetCore.Culture`. Không theo ngôn ngữ trình duyệt, để trang luôn mặc định tiếng Việt.
3. Bấm **VI | EN** trên thanh đỏ trên cùng → `NgonNguController.Doi` ghi cookie rồi quay lại đúng trang.
4. Trong view, `@L["Câu tiếng Việt"]` (biến `L` khai báo trong `Views/_ViewImports.cshtml`) tra bản dịch
   trong `Resources/SharedResource.en.resx`. Khoá là câu tiếng Việt gốc, câu nào chưa dịch thì vẫn hiện tiếng Việt.
5. `AddDataAnnotationsLocalization` dịch luôn `ErrorMessage` và `Display(Name)` của annotation,
   ví dụ thông báo lỗi ở form đăng nhập.

**Đã dịch:** layout (thanh trên, header, footer), menu danh mục, thẻ sản phẩm, trang chủ, trang danh sách
sản phẩm và bộ lọc, trang đăng nhập (cả thông báo lỗi), khối "Sản phẩm bạn đã xem".
Khu quản trị giữ tiếng Việt vì chỉ nhân viên trong nước dùng. Dữ liệu (tên sản phẩm, mô tả) không dịch
vì là nội dung do admin nhập.

**Thêm một câu cần dịch:** viết `@L["Câu mới"]` trong view, rồi thêm vào `SharedResource.en.resx`
(và `SharedResource.vi.resx` để giữ danh sách khoá đầy đủ):

```xml
<data name="Câu mới" xml:space="preserve"><value>New sentence</value></data>
```

Trong controller: inject `IStringLocalizer<SharedResource>` (xem `ProductController`, `AccountController`).

---

## 12. Bảo mật

| Vấn đề | Cách xử lý | Ở đâu |
|---|---|---|
| Lưu mật khẩu | Identity băm PBKDF2 có salt | `Program.cs` (`AddIdentity`) |
| Dò mật khẩu | Khoá tài khoản 5 phút sau 5 lần sai; thông báo lỗi không nói rõ sai email hay sai mật khẩu | `Program.cs`, `AccountController.Login` |
| Phân quyền | `[Authorize]`, `[Authorize(Roles = Role.Admin)]` trên controller và API | `Areas/Admin/Controllers`, `Controllers/Api` |
| CSRF | Mọi form POST có `[ValidateAntiForgeryToken]`, tag helper tự sinh token | Controllers |
| XSS | Razor tự mã hoá HTML; JavaScript dùng `textContent` khi chèn chữ | Views, `site.js` |
| SQL Injection | Chỉ truy vấn qua EF Core (tham số hoá) | Services, Repositories |
| Mua trùng hàng cuối | Trừ kho bằng `UPDATE ... WHERE Stock >= @n` trong transaction | `ProductVariantRepository` |
| Tải file lên | Giới hạn dung lượng, chỉ nhận đuôi ảnh, đặt tên file mới | `Helpers/ImageStorage.cs` |
| Đường dẫn ảnh | Chặn `javascript:`, `data:`, `//host` | `Helpers/ProductGallery.cs` |
| Open redirect | `returnUrl` chỉ nhận địa chỉ nội bộ (`Url.IsLocalUrl`) | `AccountController`, `NgonNguController`, `CartController` |
| Cookie | `HttpOnly`, `SameSite=Lax`, `Secure` khi HTTPS | `Program.cs`, `Helpers/CookieDaXem.cs` |
| Xem trộm đơn | Khách vãng lai chỉ xem được đơn trong phiên đã tra cứu đúng mã + số điện thoại | `Helpers/DonKhachLe.cs` |
| Lộ dữ liệu qua API | Trả DTO, không trả entity; API tài khoản chỉ Admin | `Models/Dtos` |
| Lộ cấu hình | Chuỗi kết nối riêng để trong User Secrets, không commit | mục 4.1 |
| HTTPS | `UseHttpsRedirection`, HSTS khi chạy Production | `Program.cs` |

---

## 13. Cấu trúc thư mục

```
repo-phone-asp-net/
├── .config/dotnet-tools.json   Phiên bản dotnet-ef dùng chung cho cả nhóm
├── scripts/                    xuat-script-sql.ps1 / .sh: sinh lại file SQL
├── README.md                   Giới thiệu, thiết kế, tiến độ
├── HUONG-DAN.md                Tài liệu này
└── ShopBanHang/
    ├── Controllers/            Phía khách: trang chủ, sản phẩm, giỏ, đặt hàng, đơn, tài khoản, ngôn ngữ
    │   └── Api/                Web API RESTful trả JSON
    ├── Areas/Admin/            Khu quản trị: Controllers + Views + layout riêng
    ├── Models/
    │   ├── Entities/           Entity (bảng), có annotation
    │   ├── ViewModels/         Dữ liệu cho form và danh sách
    │   ├── Dtos/               Hình dạng dữ liệu vào/ra API
    │   └── Enums/              Trạng thái đơn (kèm luật chuyển), hình thức thanh toán
    ├── Services/               Nghiệp vụ, MVC và API cùng gọi
    ├── Repositories/           Truy cập dữ liệu cho giỏ, đơn, tồn kho (+ UnitOfWork)
    ├── Data/
    │   ├── ShopContext.cs      Khai báo bảng và quan hệ
    │   ├── DbInitializer.cs    Áp migration + nạp dữ liệu mẫu
    │   └── Scripts/            ShopBanHang.sql
    ├── Migrations/             Lịch sử thay đổi cấu trúc CSDL
    ├── Helpers/                Slug, định dạng tiền, ảnh, session (GioKhachLe, DonKhachLe,
    │                           LichSuTimKiem), cookie (CookieDaXem)...
    ├── ViewComponents/         Menu danh mục, nút giỏ hàng, sản phẩm đã xem, cảnh báo tồn kho
    ├── Resources/              SharedResource.vi.resx, SharedResource.en.resx (i18n)
    ├── SharedResource.cs       Lớp mốc cho IStringLocalizer
    ├── Views/                  Giao diện phía khách
    ├── ShopBanHang.http        Gọi thử API
    └── wwwroot/                css, js, ảnh, thư viện, uploads
```

---

## 14. Lỗi thường gặp

| Hiện tượng | Nguyên nhân và cách sửa |
|---|---|
| `A network-related or instance-specific error...` khi F5 | Sai chuỗi kết nối hoặc SQL Server chưa chạy. Xem mục 4.1, kiểm tra service *SQL Server (MSSQLSERVER / SQLEXPRESS)* |
| `Login failed for user` | Dùng Windows auth nhưng SQL chỉ bật SQL auth (hoặc ngược lại). Đổi chuỗi kết nối |
| `The certificate chain was issued by an authority that is not trusted` | Thiếu `TrustServerCertificate=True` trong chuỗi kết nối |
| `Add-Migration` báo file đang bị khoá | Ứng dụng còn chạy. Shift+F5 rồi chạy lại |
| `dotnet ef` không có | Chạy `dotnet tool restore` ở thư mục gốc repo |
| Không thấy ảnh sản phẩm | Máy không có Internet (ảnh mẫu lấy từ Unsplash) |
| Trình duyệt cảnh báo HTTPS | Chạy `dotnet dev-certs https --trust` một lần |
| Gọi API ghi bị 401 | Chưa gửi cookie `ShopBanHang.Auth` của tài khoản Admin (xem đầu file `ShopBanHang.http`) |
