namespace ShopBanHang.Models.ViewModels
{
    /// <summary>Doanh thu một tháng, dùng vẽ cột trên trang tổng quan.</summary>
    public class DoanhThuThang
    {
        public int Nam { get; set; }
        public int Thang { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoDon { get; set; }

        public string Nhan => $"{Thang:00}/{Nam}";
    }

    /// <summary>Một dòng trong bảng sản phẩm bán chạy.</summary>
    public class SanPhamBanChay
    {
        public int Id { get; set; }
        public string Ten { get; set; } = string.Empty;
        public string? Anh { get; set; }
        public int SoLuongBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    /// <summary>Một dòng trong bảng khách mua nhiều nhất.</summary>
    public class KhachHangTop
    {
        public string Ten { get; set; } = string.Empty;
        public string? DienThoai { get; set; }
        public int SoDon { get; set; }
        public decimal TongTien { get; set; }
    }

    /// <summary>
    /// Toàn bộ số liệu của trang tổng quan.
    /// Gộp thống kê vào đây thay vì làm trang Thống kê riêng.
    /// </summary>
    public class DashboardViewModel
    {
        // Thẻ số liệu
        public int SoSanPham { get; set; }
        public int SoKhachHang { get; set; }
        public int SoAdmin { get; set; }
        public int SoBanner { get; set; }
        public int SoDon { get; set; }
        public decimal DoanhThuThangNay { get; set; }

        // Cảnh báo tồn kho
        public List<CanhBaoTonKhoItem> CanhBaoTonKho { get; set; } = new();

        // Đơn theo trạng thái
        public int DonChoXacNhan { get; set; }
        public int DonDaXacNhan { get; set; }
        public int DonDangGiao { get; set; }
        public int DonHoanThanh { get; set; }
        public int DonDaHuy { get; set; }

        // Bảng và biểu đồ
        public List<DoanhThuThang> DoanhThu12Thang { get; set; } = new();
        public List<SanPhamBanChay> TopSanPham { get; set; } = new();
        public List<KhachHangTop> TopKhachHang { get; set; } = new();

        /// <summary>Doanh thu tháng cao nhất, dùng để tính chiều cao cột cho biểu đồ.</summary>
        public decimal DoanhThuCaoNhat =>
            DoanhThu12Thang.Count == 0 ? 0 : DoanhThu12Thang.Max(x => x.DoanhThu);
    }
}
