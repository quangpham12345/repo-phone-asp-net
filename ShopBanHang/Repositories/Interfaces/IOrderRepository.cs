using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;

namespace ShopBanHang.Repositories.Interfaces
{
    /// <summary>Truy cập bảng Orders và OrderDetails.</summary>
    public interface IOrderRepository
    {
        /// <summary>Danh sách cho admin, lọc theo bộ lọc và phân trang.</summary>
        Task<PagedResult<Order>> TimKiemAsync(OrderFilter loc, int page, int pageSize);

        /// <summary>Đơn của một khách, mới nhất trước.</summary>
        Task<PagedResult<Order>> LayCuaUserAsync(int userId, OrderStatus? trangThai, int page, int pageSize);

        /// <summary>Đơn kèm dòng hàng, sản phẩm, biến thể và tài khoản. Không theo dõi, chỉ để xem.</summary>
        Task<Order?> LayChiTietAsync(int id);

        /// <summary>Như LayChiTietAsync nhưng tìm theo mã đơn và chỉ trả về nếu đơn thuộc người này.</summary>
        Task<Order?> LayChiTietTheoMaAsync(string orderCode, int userId);

        /// <summary>Đơn kèm dòng hàng, có theo dõi để sửa rồi lưu.</summary>
        Task<Order?> LayDeSuaAsync(int id);

        /// <summary>Mã đơn lớn nhất đã có với tiền tố này (ví dụ "DH20261104-"), null nếu chưa có.</summary>
        Task<string?> LayMaLonNhatAsync(string tienTo);

        Task<int> DemTheoTrangThaiAsync(OrderStatus trangThai);
        Task<Dictionary<OrderStatus, int>> DemCuaUserTheoTrangThaiAsync(int userId);
        Task<int> DemAsync();

        void Them(Order don);
    }
}
