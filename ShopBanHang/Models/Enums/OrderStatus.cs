using System.ComponentModel.DataAnnotations;

namespace ShopBanHang.Models.Enums
{
    /// <summary>Trạng thái đơn hàng. Luồng chuyển xem OrderStatusFlow.</summary>
    public enum OrderStatus
    {
        [Display(Name = "Chờ xác nhận")]
        ChoXacNhan = 0,

        [Display(Name = "Đã xác nhận")]
        DaXacNhan = 1,

        [Display(Name = "Đang giao")]
        DangGiao = 2,

        [Display(Name = "Hoàn thành")]
        HoanThanh = 3,

        [Display(Name = "Đã huỷ")]
        DaHuy = 4
    }

    /// <summary>
    /// Quy tắc chuyển trạng thái đơn, tách khỏi entity để mọi nơi dùng chung một luật.
    /// </summary>
    public static class OrderStatusFlow
    {
        public static OrderStatus[] BuocTiepTheo(OrderStatus hienTai) => hienTai switch
        {
            OrderStatus.ChoXacNhan => new[] { OrderStatus.DaXacNhan, OrderStatus.DaHuy },
            OrderStatus.DaXacNhan => new[] { OrderStatus.DangGiao, OrderStatus.DaHuy },
            OrderStatus.DangGiao => new[] { OrderStatus.HoanThanh, OrderStatus.DaHuy },
            _ => Array.Empty<OrderStatus>()   // Hoàn thành và Đã huỷ là trạng thái cuối
        };

        public static bool ChoPhepChuyen(OrderStatus tu, OrderStatus den)
            => BuocTiepTheo(tu).Contains(den);
    }
}
