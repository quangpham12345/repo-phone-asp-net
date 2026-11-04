namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Tài khoản nhận chuyển khoản, đọc từ mục "ThanhToan" trong appsettings.json.
    /// Mã QR dùng chuẩn VietQR (img.vietqr.io), quét được bằng app của mọi ngân hàng.
    /// </summary>
    public class ThanhToanOptions
    {
        public const string Section = "ThanhToan";

        /// <summary>Mã ngân hàng theo VietQR, ví dụ "MB", "VCB", "TCB", "ACB" hoặc mã BIN "970422".</summary>
        public string BankId { get; set; } = "MB";

        /// <summary>Tên ngân hàng để hiện cho khách đọc.</summary>
        public string BankName { get; set; } = "MB Bank";

        public string AccountNo { get; set; } = "0000000000";
        public string AccountName { get; set; } = "SHOP BAN HANG";

        /// <summary>Kiểu ảnh của VietQR: compact, compact2, qr_only, print.</summary>
        public string Template { get; set; } = "compact2";
    }
}
