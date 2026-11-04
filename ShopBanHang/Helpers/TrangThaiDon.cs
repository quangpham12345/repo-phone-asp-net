using ShopBanHang.Models.Enums;

namespace ShopBanHang.Helpers
{
    /// <summary>Màu của từng trạng thái đơn, để mọi view hiện cùng một màu.</summary>
    public static class TrangThaiDon
    {
        /// <summary>Lớp CSS khu quản trị (admin.css: .st.dh-*).</summary>
        public static string LopAdmin(OrderStatus tt) => tt switch
        {
            OrderStatus.ChoXacNhan => "dh-cho",
            OrderStatus.DaXacNhan => "dh-xacnhan",
            OrderStatus.DangGiao => "dh-giao",
            OrderStatus.HoanThanh => "dh-xong",
            OrderStatus.DaHuy => "dh-huy",
            _ => ""
        };

        /// <summary>Lớp badge Bootstrap phía khách.</summary>
        public static string LopKhach(OrderStatus tt) => tt switch
        {
            OrderStatus.ChoXacNhan => "bg-warning text-dark",
            OrderStatus.DaXacNhan => "bg-primary",
            OrderStatus.DangGiao => "bg-info text-dark",
            OrderStatus.HoanThanh => "bg-success",
            OrderStatus.DaHuy => "bg-secondary",
            _ => "bg-light text-dark"
        };

        /// <summary>Các bước của thanh tiến trình đơn (không có Đã huỷ vì đó là nhánh rẽ).</summary>
        public static readonly OrderStatus[] CacBuoc =
        {
            OrderStatus.ChoXacNhan, OrderStatus.DaXacNhan, OrderStatus.DangGiao, OrderStatus.HoanThanh
        };
    }
}
