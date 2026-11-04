using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>Nghiệp vụ đơn hàng sau khi đã đặt: tra cứu, chuyển trạng thái, huỷ.</summary>
    public interface IOrderService
    {
        // ---------- Khu quản trị ----------

        Task<PagedResult<Order>> LayDanhSachAsync(OrderFilter loc, int page, int pageSize);

        /// <summary>Lấy đơn kèm các dòng hàng, sản phẩm và biến thể của từng dòng.</summary>
        Task<Order?> LayChiTietAsync(int id);

        /// <summary>
        /// Đổi trạng thái đơn. Chỉ cho đi theo luồng trong OrderStatusFlow.
        /// Đơn chuyển khoản phải được xác nhận thanh toán trước khi giao.
        /// Sang Hoàn thành thì cộng SoldCount (và đánh dấu đã thu tiền nếu là COD).
        /// Sang Đã huỷ thì hoàn tồn kho và trả lượt dùng mã.
        /// </summary>
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id, OrderStatus moi, string? lyDoHuy = null);

        /// <summary>Lý do không chuyển được sang trạng thái này (để view tắt nút), null là được.</summary>
        string? LyDoKhongChuyenDuoc(Order don, OrderStatus moi);

        Task<int> DemAsync();
        Task<int> DemTheoTrangThaiAsync(OrderStatus trangThai);

        // ---------- Phía khách ----------

        Task<MyOrdersViewModel> LayCuaToiAsync(int userId, OrderStatus? trangThai, int page, int pageSize);

        /// <summary>Chỉ trả về nếu đơn thuộc về chính khách đang hỏi.</summary>
        Task<Order?> LayCuaToiTheoMaAsync(int userId, string orderCode);

        /// <summary>Khách tự huỷ, chỉ được khi đơn còn Chờ xác nhận.</summary>
        Task<(bool ok, string? loi)> KhachHuyAsync(int userId, string orderCode, string? lyDo);
    }
}
