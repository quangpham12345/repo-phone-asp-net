using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    /// <summary>
    /// Tổng hợp số liệu cho trang tổng quan.
    ///
    /// Mọi con số doanh thu chỉ tính trên đơn đã Hoàn thành. Đơn chờ xác nhận
    /// hay đang giao chưa phải là tiền thật, gộp vào là báo cáo sai.
    /// </summary>
    public class ThongKeService : IThongKeService
    {
        private readonly ShopContext _db;

        public ThongKeService(ShopContext db) => _db = db;

        public async Task<DashboardViewModel> LaySoLieuAsync()
        {
            var vm = new DashboardViewModel
            {
                SoSanPham = await _db.Products.CountAsync(),
                SoKhachHang = await _db.Users.CountAsync(u => u.Role!.Name == "Customer"),
                SoAdmin = await _db.Users.CountAsync(u => u.Role!.Name == "Admin"),
                SoBanner = await _db.Banners.CountAsync(b => b.IsActive),
                SoDon = await _db.Orders.CountAsync(),

                DonChoXacNhan = await _db.Orders.CountAsync(o => o.Status == OrderStatus.ChoXacNhan),
                DonDaXacNhan = await _db.Orders.CountAsync(o => o.Status == OrderStatus.DaXacNhan),
                DonDangGiao = await _db.Orders.CountAsync(o => o.Status == OrderStatus.DangGiao),
                DonHoanThanh = await _db.Orders.CountAsync(o => o.Status == OrderStatus.HoanThanh),
                DonDaHuy = await _db.Orders.CountAsync(o => o.Status == OrderStatus.DaHuy)
            };

            DateTime dauThangNay = new(DateTime.Now.Year, DateTime.Now.Month, 1);
            vm.DoanhThuThangNay = await _db.Orders
                .Where(o => o.Status == OrderStatus.HoanThanh && o.CreatedAt >= dauThangNay)
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            vm.DoanhThu12Thang = await LayDoanhThu12ThangAsync();
            vm.TopSanPham = await LayTopSanPhamAsync(5);
            vm.TopKhachHang = await LayTopKhachHangAsync(5);

            return vm;
        }

        /// <summary>
        /// Doanh thu 12 tháng gần nhất. Tháng không có đơn vẫn phải có mặt với giá trị 0,
        /// nếu không biểu đồ sẽ bị khuyết cột và đọc sai xu hướng.
        /// </summary>
        public async Task<List<DoanhThuThang>> LayDoanhThu12ThangAsync()
        {
            DateTime moc = DateTime.Now.AddMonths(-11);
            DateTime dauMoc = new(moc.Year, moc.Month, 1);

            // GroupBy theo năm và tháng rồi để SQL Server tính tổng
            var theoThang = await _db.Orders
                .Where(o => o.Status == OrderStatus.HoanThanh && o.CreatedAt >= dauMoc)
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Tong = g.Sum(x => x.Total),
                    SoDon = g.Count()
                })
                .ToListAsync();

            var ds = new List<DoanhThuThang>();
            for (int i = 0; i < 12; i++)
            {
                DateTime m = dauMoc.AddMonths(i);
                var co = theoThang.FirstOrDefault(x => x.Year == m.Year && x.Month == m.Month);

                ds.Add(new DoanhThuThang
                {
                    Nam = m.Year,
                    Thang = m.Month,
                    DoanhThu = co?.Tong ?? 0,
                    SoDon = co?.SoDon ?? 0
                });
            }
            return ds;
        }

        /// <summary>
        /// Sản phẩm bán chạy tính từ bảng OrderDetails của đơn đã hoàn thành,
        /// không lấy cột SoldCount - để con số khớp đúng với doanh thu bên cạnh.
        /// </summary>
        public async Task<List<SanPhamBanChay>> LayTopSanPhamAsync(int soLuong)
            => await _db.OrderDetails
                .Where(d => d.Order!.Status == OrderStatus.HoanThanh)
                .GroupBy(d => new { d.ProductId, d.Product!.Name, d.Product.ImageUrl })
                .Select(g => new SanPhamBanChay
                {
                    Id = g.Key.ProductId,
                    Ten = g.Key.Name,
                    Anh = g.Key.ImageUrl,
                    SoLuongBan = g.Sum(x => x.Quantity),
                    DoanhThu = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.SoLuongBan)
                .Take(soLuong)
                .ToListAsync();

        /// <summary>
        /// Khách mua nhiều nhất, gom theo số điện thoại chứ không theo UserId,
        /// vì khách không đăng nhập vẫn đặt được hàng và khi đó UserId là null.
        /// </summary>
        public async Task<List<KhachHangTop>> LayTopKhachHangAsync(int soLuong)
            => await _db.Orders
                .Where(o => o.Status == OrderStatus.HoanThanh)
                .GroupBy(o => new { o.Phone, o.CustomerName })
                .Select(g => new KhachHangTop
                {
                    Ten = g.Key.CustomerName,
                    DienThoai = g.Key.Phone,
                    SoDon = g.Count(),
                    TongTien = g.Sum(x => x.Total)
                })
                .OrderByDescending(x => x.TongTien)
                .Take(soLuong)
                .ToListAsync();
    }
}
