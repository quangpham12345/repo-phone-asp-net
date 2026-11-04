using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;

namespace ShopBanHang.Models.ViewModels
{
    /// <summary>Bộ lọc danh sách đơn ở khu quản trị. Bind thẳng từ query string.</summary>
    public class OrderFilter
    {
        public string? TuKhoa { get; set; }
        public OrderStatus? TrangThai { get; set; }
        public PaymentMethod? PhuongThuc { get; set; }
        public bool? DaThanhToan { get; set; }
        public DateTime? TuNgay { get; set; }
        public DateTime? DenNgay { get; set; }

        public bool DangLoc => !string.IsNullOrWhiteSpace(TuKhoa) || TrangThai.HasValue || PhuongThuc.HasValue
                            || DaThanhToan.HasValue || TuNgay.HasValue || DenNgay.HasValue;
    }

    /// <summary>Trang "Đơn hàng của tôi".</summary>
    public class MyOrdersViewModel
    {
        public PagedResult<Order> KetQua { get; set; } = new();
        public OrderStatus? TrangThai { get; set; }
        public Dictionary<OrderStatus, int> SoDonTheoTrangThai { get; set; } = new();
        public int TongSoDon => SoDonTheoTrangThai.Values.Sum();
    }

    /// <summary>Trang chi tiết đơn phía khách (và trang đặt hàng thành công).</summary>
    public class MyOrderDetailViewModel
    {
        public Order Don { get; set; } = new();

        /// <summary>Ảnh QR chuyển khoản. Null nếu không cần hiện (COD, đã trả tiền, đã huỷ).</summary>
        public string? QrUrl { get; set; }
        public ThongTinChuyenKhoan? ChuyenKhoan { get; set; }

        public bool DuocHuy => Don.Status == OrderStatus.ChoXacNhan;
    }

    /// <summary>Màn chi tiết đơn ở khu quản trị.</summary>
    public class AdminOrderDetailViewModel
    {
        public Order Don { get; set; } = new();

        /// <summary>Các bước đi tiếp được theo luồng, kèm lý do nếu tạm thời chưa bấm được.</summary>
        public List<(OrderStatus TrangThai, string? LyDoKhong)> BuocTiepTheo { get; set; } = new();

        public bool CoTheHuy => BuocTiepTheo.Any(b => b.TrangThai == OrderStatus.DaHuy);

        public bool CanXacNhanThanhToan => Don.PaymentMethod == PaymentMethod.ChuyenKhoan
                                        && !Don.IsPaid && Don.Status != OrderStatus.DaHuy;
    }

    /// <summary>Thông tin tài khoản nhận tiền hiện cạnh mã QR.</summary>
    public record ThongTinChuyenKhoan(string NganHang, string SoTaiKhoan, string ChuTaiKhoan,
                                      decimal SoTien, string NoiDung);
}
