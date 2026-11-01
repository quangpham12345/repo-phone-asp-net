using ShopBanHang.Models.Entities;

namespace ShopBanHang.Helpers
{
    /// <summary>
    /// Tính giá và tồn kho của cả sản phẩm từ các biến thể của nó. Đây là nơi duy nhất
    /// quyết định luật này: service khi lưu biến thể và DbInitializer khi seed cùng gọi vào đây.
    ///
    /// Product.Price và Product.Stock vẫn là cột thật để danh sách, lọc giá, sắp xếp
    /// và thống kê chạy bằng một cột như cũ, nhưng khi sản phẩm có biến thể thì hai số đó
    /// do hàm này tính ra, không nhập tay nữa.
    /// </summary>
    public static class TongHopBienThe
    {
        public sealed record KetQua(decimal Gia, decimal? GiaGoc, int TonKho);

        /// <summary>Không có biến thể nào thì trả về null, nghĩa là giữ nguyên giá và tồn kho nhập tay.</summary>
        public static KetQua? Tinh(IEnumerable<ProductVariant> bienThe)
        {
            var ds = bienThe.ToList();
            if (ds.Count == 0) return null;

            var dangBan = ds.Where(v => v.IsActive).ToList();

            // Có biến thể nhưng không cái nào đang bán: giữ giá thấp nhất để hiển thị, tồn kho coi như 0
            if (dangBan.Count == 0)
            {
                var re = RePhatNhat(ds);
                return new KetQua(re.Price, re.OldPrice, 0);
            }

            var reNhat = RePhatNhat(dangBan);
            return new KetQua(reNhat.Price, reNhat.OldPrice, dangBan.Sum(v => v.Stock));
        }

        // Cùng giá thì lấy biến thể có giá gốc cao hơn, để hiện mức giảm lớn nhất
        private static ProductVariant RePhatNhat(List<ProductVariant> ds)
            => ds.OrderBy(v => v.Price).ThenByDescending(v => v.OldPrice ?? 0).First();
    }
}