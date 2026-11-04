using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Repositories.Interfaces;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orders;
        private readonly IProductVariantRepository _kho;
        private readonly ICouponService _coupons;
        private readonly IUnitOfWork _uow;

        public OrderService(IOrderRepository orders, IProductVariantRepository kho,
                            ICouponService coupons, IUnitOfWork uow)
        {
            _orders = orders;
            _kho = kho;
            _coupons = coupons;
            _uow = uow;
        }

        // ====================== Khu quản trị ======================

        public Task<PagedResult<Order>> LayDanhSachAsync(OrderFilter loc, int page, int pageSize)
            => _orders.TimKiemAsync(loc, page, pageSize);

        public Task<Order?> LayChiTietAsync(int id) => _orders.LayChiTietAsync(id);

        public string? LyDoKhongChuyenDuoc(Order don, OrderStatus moi)
        {
            // Luật chuyển trạng thái nằm trong OrderStatusFlow, không rải if ở đây
            if (!OrderStatusFlow.ChoPhepChuyen(don.Status, moi))
                return $"Không thể chuyển từ \"{don.Status.Nhan()}\" sang \"{moi.Nhan()}\".";

            // Chuyển khoản mà chưa thấy tiền thì không giao hàng
            if (moi == OrderStatus.DangGiao && don.PaymentMethod == PaymentMethod.ChuyenKhoan && !don.IsPaid)
                return "Đơn chuyển khoản phải được xác nhận đã thanh toán trước khi giao.";

            return null;
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id, OrderStatus moi, string? lyDoHuy = null)
        {
            // Không AsNoTracking vì còn phải sửa và lưu lại
            var don = await _orders.LayDeSuaAsync(id);
            if (don == null) return (false, "Không tìm thấy đơn hàng.");

            string? loi = LyDoKhongChuyenDuoc(don, moi);
            if (loi != null) return (false, loi);

            await using var tx = await _uow.BeginTransactionAsync();

            if (moi == OrderStatus.DaHuy)
            {
                await HuyNoiBoAsync(don, lyDoHuy);
            }
            else
            {
                don.Status = moi;

                if (moi == OrderStatus.HoanThanh)
                {
                    // Chỉ cộng số đã bán khi đơn thật sự hoàn thành.
                    // Cộng sớm từ lúc xác nhận thì đơn bị huỷ giữa đường sẽ làm sai thống kê.
                    foreach (var dong in don.OrderDetails)
                        await _kho.CongDaBanAsync(dong.ProductId, dong.Quantity);

                    // COD: giao thành công nghĩa là shipper đã thu tiền
                    if (!don.IsPaid)
                    {
                        don.IsPaid = true;
                        don.PaidAt = DateTime.Now;
                    }
                }
            }

            await _uow.SaveChangesAsync();
            await tx.CommitAsync();
            return (true, null);
        }

        public Task<int> DemAsync() => _orders.DemAsync();

        public Task<int> DemTheoTrangThaiAsync(OrderStatus trangThai) => _orders.DemTheoTrangThaiAsync(trangThai);

        // ====================== Phía khách ======================

        public async Task<MyOrdersViewModel> LayCuaToiAsync(int userId, OrderStatus? trangThai, int page, int pageSize)
            => new()
            {
                KetQua = await _orders.LayCuaUserAsync(userId, trangThai, page, pageSize),
                TrangThai = trangThai,
                SoDonTheoTrangThai = await _orders.DemCuaUserTheoTrangThaiAsync(userId)
            };

        public async Task<Order?> LayCuaToiTheoMaAsync(int userId, string orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode)) return null;
            return await _orders.LayChiTietTheoMaAsync(orderCode.Trim(), userId);
        }

        public async Task<(bool ok, string? loi)> KhachHuyAsync(int userId, string orderCode, string? lyDo)
        {
            // Tìm theo mã + chủ đơn trước, để khách không huỷ được đơn của người khác bằng cách đoán Id
            var xem = await LayCuaToiTheoMaAsync(userId, orderCode);
            if (xem == null) return (false, "Không tìm thấy đơn hàng.");

            if (xem.Status != OrderStatus.ChoXacNhan)
                return (false, "Đơn đã được shop xác nhận nên không tự huỷ được nữa. Vui lòng liên hệ hotline để được hỗ trợ.");

            string lyDoDayDu = string.IsNullOrWhiteSpace(lyDo) ? "Khách tự huỷ" : "Khách tự huỷ: " + lyDo.Trim();
            if (lyDoDayDu.Length > 250) lyDoDayDu = lyDoDayDu[..250];

            return await DoiTrangThaiAsync(xem.Id, OrderStatus.DaHuy, lyDoDayDu);
        }

        // ====================== Nội bộ ======================

        /// <summary>
        /// Huỷ đơn: hoàn tồn kho cho các dòng đã trừ kho lúc đặt, trả lại lượt dùng mã.
        /// Gọi trong transaction của DoiTrangThaiAsync.
        /// </summary>
        private async Task HuyNoiBoAsync(Order don, string? lyDo)
        {
            // Chỉ dòng có biến thể mới bị trừ kho lúc đặt (đơn cũ trước khi có giỏ hàng thì không)
            foreach (var dong in don.OrderDetails.Where(d => d.ProductVariantId.HasValue))
                await _kho.HoanTonKhoAsync(dong.ProductVariantId!.Value, dong.Quantity);

            if (don.CouponId.HasValue)
                await _coupons.GiamLuotDungAsync(don.CouponId.Value);

            don.Status = OrderStatus.DaHuy;
            don.CancelledAt = DateTime.Now;
            don.CancelReason = string.IsNullOrWhiteSpace(lyDo) ? null : lyDo.Trim();
        }
    }
}
