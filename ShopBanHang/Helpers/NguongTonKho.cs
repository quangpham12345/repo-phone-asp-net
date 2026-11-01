namespace ShopBanHang.Helpers
{
    /// <summary>Ngưỡng cảnh báo tồn kho, chỉ định nghĩa ở đây.</summary>
    public static class NguongTonKho
    {
        /// <summary>Tồn từ 1 đến giá trị này là "sắp hết"; từ 0 trở xuống là "hết hàng".</summary>
        public const int Nguong = 5;
    }
}