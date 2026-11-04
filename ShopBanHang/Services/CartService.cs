using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Repositories.Interfaces;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class CartService : ICartService
    {
        /// <summary>Trần số lượng một dòng, khớp với [Range] trên CartItem.Quantity.</summary>
        public const int ToiDaMotDong = 99;

        private readonly ICartRepository _cart;
        private readonly IProductVariantRepository _variants;
        private readonly IUnitOfWork _uow;

        public CartService(ICartRepository cart, IProductVariantRepository variants, IUnitOfWork uow)
        {
            _cart = cart;
            _variants = variants;
            _uow = uow;
        }

        // ====================== Khách đã đăng nhập ======================

        public async Task<CartViewModel> LayGioAsync(int userId)
        {
            var dong = await _cart.LayCuaUserAsync(userId);
            return new CartViewModel
            {
                Lines = dong.Where(c => c.ProductVariant?.Product != null).Select(TaoDong).ToList()
            };
        }

        public async Task<(bool ok, string? loi)> ThemAsync(int userId, int variantId, int soLuong)
        {
            if (soLuong < 1) soLuong = 1;

            var v = await _variants.LayKemSanPhamAsync(variantId);
            var dong = await _cart.LayDongAsync(userId, variantId);
            int daCo = dong?.Quantity ?? 0;

            string? loi = KiemTraThem(v, daCo, soLuong);
            if (loi != null) return (false, loi);

            if (dong == null)
            {
                _cart.Them(new CartItem
                {
                    UserId = userId,
                    ProductVariantId = variantId,
                    Quantity = soLuong,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                });
            }
            else
            {
                dong.Quantity = daCo + soLuong;
                dong.UpdatedAt = DateTime.Now;
            }

            await _uow.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> CapNhatAsync(int userId, int cartItemId, int soLuong)
        {
            var dong = await _cart.LayTheoIdAsync(cartItemId, userId);
            if (dong == null) return (false, "Không tìm thấy sản phẩm trong giỏ.");

            if (soLuong <= 0)
            {
                _cart.Xoa(dong);
                await _uow.SaveChangesAsync();
                return (true, null);
            }

            string? loi = KiemTraSoLuong(dong.ProductVariant?.Stock ?? 0, soLuong);
            if (loi != null) return (false, loi);

            dong.Quantity = soLuong;
            dong.UpdatedAt = DateTime.Now;
            await _uow.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> XoaAsync(int userId, int cartItemId)
        {
            var dong = await _cart.LayTheoIdAsync(cartItemId, userId);
            if (dong == null) return (false, "Không tìm thấy sản phẩm trong giỏ.");

            _cart.Xoa(dong);
            await _uow.SaveChangesAsync();
            return (true, null);
        }

        public async Task XoaHetAsync(int userId)
        {
            var dong = await _cart.LayCuaUserAsync(userId);
            if (dong.Count == 0) return;

            _cart.XoaNhieu(dong);
            await _uow.SaveChangesAsync();
        }

        public Task<int> DemAsync(int userId) => _cart.DemSoLuongAsync(userId);

        // ====================== Khách chưa đăng nhập ======================

        public async Task<CartViewModel> LayGioKhachAsync(IReadOnlyList<GuestCartLine> gio)
        {
            var bienThe = (await _variants.LayNhieuKemSanPhamAsync(gio.Select(x => x.VariantId)))
                .ToDictionary(v => v.Id);

            // Dựng CartItem tạm (không lưu) để dùng chung TaoDong với giỏ trong CSDL.
            // Id của dòng chính là VariantId, view dùng nó để sửa/xoá.
            var lines = gio
                .Where(x => bienThe.ContainsKey(x.VariantId) && bienThe[x.VariantId].Product != null)
                .Select(x => TaoDong(new CartItem
                {
                    Id = x.VariantId,
                    ProductVariantId = x.VariantId,
                    ProductVariant = bienThe[x.VariantId],
                    Quantity = x.Quantity
                }))
                .ToList();

            return new CartViewModel { Lines = lines };
        }

        public async Task<(bool ok, string? loi)> ThemKhachAsync(List<GuestCartLine> gio, int variantId, int soLuong)
        {
            if (soLuong < 1) soLuong = 1;

            var v = await _variants.LayKemSanPhamAsync(variantId);
            var dong = gio.FirstOrDefault(x => x.VariantId == variantId);
            int daCo = dong?.Quantity ?? 0;

            string? loi = KiemTraThem(v, daCo, soLuong);
            if (loi != null) return (false, loi);

            if (dong == null) gio.Add(new GuestCartLine { VariantId = variantId, Quantity = soLuong });
            else dong.Quantity = daCo + soLuong;

            return (true, null);
        }

        public async Task<(bool ok, string? loi)> CapNhatKhachAsync(List<GuestCartLine> gio, int variantId, int soLuong)
        {
            var dong = gio.FirstOrDefault(x => x.VariantId == variantId);
            if (dong == null) return (false, "Không tìm thấy sản phẩm trong giỏ.");

            if (soLuong <= 0)
            {
                gio.Remove(dong);
                return (true, null);
            }

            var v = await _variants.LayKemSanPhamAsync(variantId);
            string? loi = KiemTraSoLuong(v?.Stock ?? 0, soLuong);
            if (loi != null) return (false, loi);

            dong.Quantity = soLuong;
            return (true, null);
        }

        public async Task GopGioKhachAsync(int userId, IReadOnlyList<GuestCartLine> gio)
        {
            if (gio.Count == 0) return;

            var bienThe = (await _variants.LayNhieuKemSanPhamAsync(gio.Select(x => x.VariantId)))
                .ToDictionary(v => v.Id);
            var hienCo = (await _cart.LayCuaUserAsync(userId)).ToDictionary(c => c.ProductVariantId);

            foreach (var x in gio)
            {
                if (!bienThe.TryGetValue(x.VariantId, out var v)) continue;
                if (!v.IsActive || v.Product?.IsActive != true || v.Stock <= 0) continue;

                // Cộng dồn với giỏ cũ của tài khoản, nhưng không vượt tồn kho và trần một dòng
                int tran = Math.Min(v.Stock, ToiDaMotDong);

                if (hienCo.TryGetValue(x.VariantId, out var cu))
                {
                    cu.Quantity = Math.Min(cu.Quantity + x.Quantity, tran);
                    cu.UpdatedAt = DateTime.Now;
                }
                else
                {
                    _cart.Them(new CartItem
                    {
                        UserId = userId,
                        ProductVariantId = x.VariantId,
                        Quantity = Math.Min(x.Quantity, tran),
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    });
                }
            }

            await _uow.SaveChangesAsync();
        }

        // ====================== Dùng chung ======================

        /// <summary>Ghép dòng giỏ với giá, tồn kho hiện tại. CheckoutService cũng dùng hàm này.</summary>
        internal static CartLineViewModel TaoDong(CartItem c)
        {
            var v = c.ProductVariant!;
            var sp = v.Product!;

            return new CartLineViewModel
            {
                CartItemId = c.Id,
                VariantId = v.Id,
                ProductId = sp.Id,
                ProductName = sp.Name,
                Slug = sp.Slug,
                VariantName = PhanLoai.Ten(v.Storage, v.Color),
                ImageUrl = PhanLoai.Anh(v.ImageUrl, sp.ImageUrl),
                UnitPrice = v.Price,
                OldPrice = v.OldPrice,
                Quantity = c.Quantity,
                Stock = v.Stock,
                DangBan = v.IsActive && sp.IsActive
            };
        }

        /// <summary>Kiểm tra thêm <paramref name="soLuong"/> vào dòng đang có <paramref name="daCo"/>. Null là được.</summary>
        private static string? KiemTraThem(ProductVariant? v, int daCo, int soLuong)
        {
            if (v == null || v.Product == null) return "Không tìm thấy sản phẩm.";
            if (!v.IsActive || !v.Product.IsActive) return "Sản phẩm này hiện không còn bán.";
            if (v.Stock <= 0) return "Phân loại này đã hết hàng.";

            int moi = daCo + soLuong;

            // Không cho cộng dồn vượt tồn kho. Báo rõ trong giỏ đã có bao nhiêu để khách hiểu vì sao.
            if (moi > v.Stock)
                return daCo > 0
                    ? $"Chỉ còn {v.Stock} sản phẩm, trong giỏ của bạn đã có {daCo}."
                    : $"Chỉ còn {v.Stock} sản phẩm.";

            if (moi > ToiDaMotDong) return $"Mỗi phân loại chỉ đặt tối đa {ToiDaMotDong} sản phẩm.";
            return null;
        }

        private static string? KiemTraSoLuong(int tonKho, int soLuong)
        {
            if (soLuong > ToiDaMotDong) return $"Mỗi phân loại chỉ đặt tối đa {ToiDaMotDong} sản phẩm.";
            if (soLuong > tonKho) return tonKho > 0 ? $"Chỉ còn {tonKho} sản phẩm." : "Phân loại này đã hết hàng.";
            return null;
        }
    }
}
