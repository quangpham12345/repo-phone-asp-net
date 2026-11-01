# Hướng dẫn cài đặt và chạy ShopBanHang

Website bán điện thoại — bài tập lớn môn Lập trình Web.

---

## 1. Công nghệ dùng trong dự án

| Phần | Dùng gì |
|---|---|
| Nền tảng | ASP.NET Core MVC, .NET 8 |
| Truy cập dữ liệu | Entity Framework Core 8 (Code First + Migrations) |
| Cơ sở dữ liệu | SQL Server |
| Giao diện | Bootstrap 5, Bootstrap Icons, jQuery |
| Đăng nhập | Cookie Authentication (tự quản bảng `Users` / `Roles`) |
| Mã hoá mật khẩu | PBKDF2 — SHA-256, 100.000 vòng |
| API | ASP.NET Core Web API, trả JSON theo kiểu RESTful |

---

## 2. Cần cài gì trước

1. **Visual Studio 2022** (hoặc mới hơn), có tích workload *ASP.NET and web development*
2. **.NET 8 SDK** — Visual Studio 2022 thường cài kèm
3. **SQL Server** — bản *Express* hoặc *Developer* đều được, miễn phí

Kiểm tra .NET đã có chưa, mở Terminal gõ:

```bash
dotnet --version
```

Ra số bắt đầu bằng `8.` hoặc lớn hơn là được.

---

## 3. Chạy dự án

### Cách 1 — Visual Studio (dễ nhất)

1. Mở file `ShopBanHang.sln`
2. Nhấn **F5**

Xong. **Không phải tạo database, không phải chạy lệnh nào.** Lúc khởi động,
chương trình tự tạo database `ShopBanHang`, tạo đủ 12 bảng và nạp dữ liệu mẫu.

### Cách 2 — dòng lệnh

```bash
cd ShopBanHang
dotnet run
```

Rồi mở trình duyệt vào địa chỉ mà cửa sổ lệnh in ra (dạng `http://localhost:5xxx`).

---

## 4. Nếu không kết nối được database

Mặc định chương trình tìm SQL Server ở `localhost`. Nếu máy bạn khác, mở file
`ShopBanHang/appsettings.json` và sửa dòng `ShopContext`:

```json
"ConnectionStrings": {
  "ShopContext": "Server=localhost;Database=ShopBanHang;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

| Trường hợp | Sửa thành |
|---|---|
| SQL Server tên khác | `Server=TEN-MAY-CUA-BAN\\SQLEXPRESS;...` |
| Đăng nhập bằng tài khoản SQL | `Server=localhost;Database=ShopBanHang;User Id=sa;Password=matkhau;TrustServerCertificate=True` |
| Dùng LocalDB | `Server=(localdb)\\MSSQLLocalDB;Database=ShopBanHang;Trusted_Connection=True` |

---

## 5. Tài khoản để thử

| Vai trò | Email | Mật khẩu | Đăng nhập xong vào đâu |
|---|---|---|---|
| Quản trị | `admin@shop.com` | `Admin@123` | Khu quản trị `/Admin` |
| Khách hàng | `khach1@shop.com` | `Khach@123` | Trang bán hàng `/` |
| Khách bị tắt | `khach2@shop.com` | `Khach@123` | Không vào được (để thử) |

> Đây là tài khoản demo. Đổi mật khẩu trước khi nộp bài.

---

## 6. Dữ liệu mẫu có sẵn

4 danh mục · 4 thương hiệu · **8 sản phẩm** · 3 banner · **6 đơn hàng** (đủ 5 trạng thái)

Dữ liệu được cố tình làm đa dạng để thử giao diện: có hàng giảm giá, hàng hết kho,
một hàng đã ẩn (khách không thấy, admin vẫn thấy), đơn của khách đăng nhập và đơn
của khách không đăng nhập.

---

## 7. Cấu trúc thư mục

```
ShopBanHang/
│
├── Controllers/            Phía khách: trang chủ, sản phẩm, tài khoản
│   └── Api/                API trả JSON
│
├── Areas/Admin/            Khu quản trị — tách riêng hoàn toàn
│   ├── Controllers/        Tổng quan, đơn hàng, sản phẩm, danh mục,
│   │                       thương hiệu, tin tức, banner, tài khoản
│   └── Views/              Giao diện quản trị + layout riêng
│
├── Models/
│   ├── Entities/           11 bảng dữ liệu, có Annotation để kiểm tra nhập liệu
│   ├── ViewModels/         Dữ liệu cho form và danh sách
│   ├── Dtos/               Dữ liệu trả ra API
│   └── Enums/              Trạng thái đơn hàng, hình thức thanh toán
│
├── Services/               NƠI CHỨA NGHIỆP VỤ
│                           Controller chỉ gọi vào đây, không tự xử lý
│
├── Data/
│   ├── ShopContext.cs      Khai báo bảng và quan hệ
│   ├── DbInitializer.cs    Tạo database + nạp dữ liệu mẫu
│   └── Scripts/            File .sql để tạo database bằng tay
│
├── Helpers/                Hàm dùng chung: băm mật khẩu, tạo slug,
│                           định dạng tiền, lưu ảnh tải lên
│
├── ViewComponents/         Khối tự lấy dữ liệu riêng (thanh danh mục)
│
├── Views/                  Giao diện phía khách
│   └── Shared/             Layout khách, thẻ sản phẩm dùng lại
│
├── Migrations/             Lịch sử thay đổi cấu trúc database
│
└── wwwroot/                File tĩnh
    ├── css/                admin.css (quản trị) · style.css (khách)
    ├── images/             Ảnh mẫu sản phẩm và banner
    └── uploads/            Ảnh admin tải lên
```

**Ý chính của cấu trúc:** nghiệp vụ nằm ở `Services`. Controller chỉ nhận yêu cầu,
gọi service, rồi chọn cách trả về — trả HTML cho trang web, hoặc trả JSON cho API.
Nhờ vậy một quy tắc chỉ viết một lần, hai bên dùng chung.

---

## 8. Các đường dẫn chính

**Phía khách**

| Đường dẫn | Trang |
|---|---|
| `/` | Trang chủ — banner, sản phẩm nổi bật, sản phẩm mới |
| `/Product` | Danh sách sản phẩm, có lọc và phân trang |
| `/Product/Detail/{duong-dan}` | Chi tiết sản phẩm |
| `/Account/Login` | Đăng nhập |
| `/Account/Register` | Đăng ký |
| `/Account/Profile` | Tài khoản của tôi |

**Khu quản trị** (phải đăng nhập bằng tài khoản Admin)

| Đường dẫn | Trang |
|---|---|
| `/Admin` | Tổng quan + thống kê |
| `/Admin/Order` | Đơn hàng |
| `/Admin/Product` | Sản phẩm |
| `/Admin/Category` | Danh mục |
| `/Admin/Brand` | Thương hiệu |
| `/Admin/News` | Tin tức |
| `/Admin/Banner` | Banner trang chủ |
| `/Admin/User` | Tài khoản |

**API**

| Đường dẫn | Việc |
|---|---|
| `GET /api/users` | Danh sách tài khoản, có phân trang |
| `GET /api/users/{id}` | Một tài khoản |

---

## 9. Khi cần thêm bảng mới

Trong Visual Studio mở **Tools → NuGet Package Manager → Package Manager Console**,
chọn Default project là `ShopBanHang` rồi gõ:

```powershell
Add-Migration TenViecVuaLam
```

Sau đó chỉ cần **F5** — chương trình tự áp thay đổi vào database.

> Phải **Shift+F5** tắt chương trình trước khi gõ `Add-Migration`, vì lệnh này cần
> build lại dự án mà chương trình đang chạy thì giữ file.

Muốn xuất lại file .sql để nộp:

```bash
dotnet ef migrations script -o Data/Scripts/ShopBanHang.sql --idempotent
```

---

## 10. Lỗi hay gặp

| Hiện tượng | Cách xử lý |
|---|---|
| `A network-related or instance-specific error` | SQL Server chưa chạy, hoặc sai tên server trong `appsettings.json`. Mở **SQL Server Configuration Manager** kiểm tra dịch vụ đã Start chưa |
| `Login failed for user` | Sai tài khoản database. Thử dùng `Trusted_Connection=True` để đăng nhập bằng user Windows |
| `MSB3027 ... file is locked by: ShopBanHang` | Chương trình đang chạy. Bấm **Shift+F5** rồi build lại |
| Trang trắng, không có màu sắc | File CSS chưa tải. Kiểm tra `wwwroot/css/style.css` có tồn tại |
| Mất hết biểu tượng | Bootstrap Icons tải từ Internet. Máy không có mạng thì biểu tượng trống, bố cục vẫn đúng |
| Database bị sai, muốn làm lại từ đầu | Xoá database `ShopBanHang` trong SQL Server rồi F5 — chương trình tự tạo lại và nạp dữ liệu mẫu |

---

## 11. Phần đã làm và chưa làm

**Đã làm**

- Cơ sở dữ liệu 11 bảng, dữ liệu mẫu, file .sql để nộp
- Đăng nhập rẽ nhánh theo vai trò, đăng ký, đổi mật khẩu
- Trang chủ, danh sách sản phẩm (lọc + phân trang), chi tiết sản phẩm
- Khu quản trị đầy đủ: tổng quan kèm thống kê, đơn hàng theo luồng trạng thái,
  sản phẩm, danh mục, thương hiệu, tin tức, banner, tài khoản
- API tài khoản

**Chưa làm**

- Giỏ hàng và đặt hàng
- Tra cứu đơn hàng phía khách
- Trang tin tức phía khách
- Chat hỗ trợ
- Tìm kiếm gợi ý bằng AJAX
- Đa ngôn ngữ Việt / Anh
