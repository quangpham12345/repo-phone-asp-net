using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Services.Interfaces
{
    /// <summary>Nghiệp vụ đơn hàng.</summary>
    public interface IOrderService
    {
        Task<PagedResult<Order>> LayDanhSachAsync(string? tuKhoa, OrderStatus? trangThai,
                                                  int page, int pageSize);

        /// <summary>Lấy đơn kèm các dòng hàng và sản phẩm của từng dòng.</summary>
        Task<Order?> LayChiTietAsync(int id);

        /// <summary>
        /// Đổi trạng thái đơn. Chỉ cho đi theo luồng trong OrderStatusFlow,
        /// và khi sang Hoàn thành thì cộng dồn SoldCount cho từng sản phẩm.
        /// </summary>
        Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id, OrderStatus moi);

        Task<int> DemAsync();
        Task<int> DemTheoTrangThaiAsync(OrderStatus trangThai);
    }
}
