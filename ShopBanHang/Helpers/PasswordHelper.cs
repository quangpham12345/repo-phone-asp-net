using System.Security.Cryptography;

namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Băm mật khẩu bằng PBKDF2 (SHA-256, 100.000 vòng), chỉ dùng thư viện có sẵn của .NET.
    ///
    /// Mỗi mật khẩu có salt ngẫu nhiên riêng, nên hai người đặt cùng mật khẩu vẫn ra
    /// hai chuỗi băm khác nhau. Chuỗi lưu trong CSDL có dạng:
    ///     vong.saltBase64.hashBase64
    /// Giữ số vòng ngay trong chuỗi để sau này tăng lên vẫn đọc được mật khẩu băm cũ.
    /// </summary>
    public static class PasswordHelper
    {
        private const int SoVongMacDinh = 100_000;
        private const int DoDaiSalt = 16;   // 128 bit
        private const int DoDaiHash = 32;   // 256 bit

        public static string Hash(string matKhau)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(DoDaiSalt);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                matKhau, salt, SoVongMacDinh, HashAlgorithmName.SHA256, DoDaiHash);

            return $"{SoVongMacDinh}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string matKhau, string chuoiDaLuu)
        {
            if (string.IsNullOrWhiteSpace(chuoiDaLuu)) return false;

            string[] phan = chuoiDaLuu.Split('.');
            if (phan.Length != 3) return false;

            try
            {
                int vong = int.Parse(phan[0]);
                byte[] salt = Convert.FromBase64String(phan[1]);
                byte[] hashDaLuu = Convert.FromBase64String(phan[2]);

                byte[] hashMoi = Rfc2898DeriveBytes.Pbkdf2(
                    matKhau, salt, vong, HashAlgorithmName.SHA256, hashDaLuu.Length);

                // So sánh theo thời gian hằng số để không rò rỉ thông tin qua thời gian phản hồi
                return CryptographicOperations.FixedTimeEquals(hashMoi, hashDaLuu);
            }
            catch
            {
                return false;   // chuỗi trong CSDL bị sai định dạng thì coi như không khớp
            }
        }
    }
}
