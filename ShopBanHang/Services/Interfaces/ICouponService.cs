using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>Kết quả kiểm tra một mã ở bước thanh toán.</summary>
    public record CouponKetQua(bool Ok, string? Loi, Coupon? Coupon, decimal SoTienGiam);

    /// <summary>Nghiệp vụ mã giảm giá: quản trị (CRUD) và kiểm tra/tính giảm khi thanh toán.</summary>
    public interface ICouponService
    {
        // ---------- Khu quản trị ----------

        /// <summary>Danh sách có lọc theo từ khoá + trạng thái, phân trang bằng PagedResult dùng chung.</summary>
        Task<PagedResult<Coupon>> LayDanhSachAsync(string? tuKhoa, CouponStatus? trangThai, int page, int pageSize);

        Task<Coupon?> LayTheoIdAsync(int id);
        Task<(bool ok, string? loi)> LuuAsync(Coupon coupon);
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id);

        /// <summary>Chỉ xoá được mã chưa ai dùng; mã đã dùng thì tắt đi.</summary>
        Task<(bool ok, string? loi)> XoaAsync(int id);

        // ---------- Dùng ở thanh toán (phần của Quang gọi vào) ----------

        /// <summary>Kiểm tra mã với tổng tiền hàng và trả luôn số tiền được giảm.</summary>
        Task<CouponKetQua> KiemTraAsync(string? code, decimal tongTien);

        /// <summary>Tính tiền giảm của một mã cho tổng tiền hàng (đã chặn trần và không vượt quá tổng tiền).</summary>
        decimal TinhGiam(Coupon coupon, decimal tongTien);

        /// <summary>
        /// Cộng 1 lượt dùng, an toàn khi nhiều người đặt cùng lúc. Trả false nếu mã vừa hết lượt.
        /// Gọi trong lúc tạo đơn, sau khi KiemTraAsync đã ok.
        /// </summary>
        Task<bool> TangLuotDungAsync(int id);

        /// <summary>Trả lại 1 lượt dùng khi đơn có dùng mã bị huỷ.</summary>
        Task GiamLuotDungAsync(int id);
    }
}