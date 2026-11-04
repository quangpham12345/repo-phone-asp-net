using Microsoft.EntityFrameworkCore;
using ShopBanHang.Helpers;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Repositories.Interfaces;
using ShopBanHang.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace ShopBanHang.Services
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ICartRepository _cart;
        private readonly IProductVariantRepository _variants;
        private readonly IOrderRepository _orders;
        private readonly IUnitOfWork _uow;
        private readonly ICouponService _coupons;
        private readonly IAddressService _addresses;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<CheckoutService> _log;

        public CheckoutService(ICartRepository cart, IProductVariantRepository variants, IOrderRepository orders,
                               IUnitOfWork uow, ICouponService coupons, IAddressService addresses,
                               UserManager<User> userManager, ILogger<CheckoutService> log)
        {
            _cart = cart;
            _variants = variants;
            _orders = orders;
            _uow = uow;
            _coupons = coupons;
            _addresses = addresses;
            _userManager = userManager;
            _log = log;
        }

        public async Task<CheckoutViewModel> ChuanBiAsync(int userId, IReadOnlyList<GuestCartLine> gioKhach,
                                                          CheckoutForm? form = null)
        {
            bool daDangNhap = userId > 0;
            var dong = await LayDongGioAsync(userId, gioKhach);

            var vm = new CheckoutViewModel
            {
                DaDangNhap = daDangNhap,
                Gio = new CartViewModel { Lines = dong.Select(CartService.TaoDong).ToList() },
                CacDiaChi = daDangNhap ? await _addresses.LayCuaUserAsync(userId) : new List<Address>()
            };

            if (form != null)
            {
                // Trang được vẽ lại sau lỗi: giữ nguyên những gì khách đã nhập
                vm.Form = form;
                if (!daDangNhap) vm.Form.AddressId = null;
            }
            else if (!daDangNhap)
            {
                // Khách vãng lai: form trống, tự điền hết. Không có sổ địa chỉ nên không lưu được.
                vm.Form = new CheckoutForm { LuuDiaChi = false };
            }
            else
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                var macDinh = vm.CacDiaChi.FirstOrDefault(a => a.IsDefault) ?? vm.CacDiaChi.FirstOrDefault();

                vm.Form = new CheckoutForm
                {
                    AddressId = macDinh?.Id,
                    // Chưa có sổ địa chỉ thì điền sẵn từ thông tin tài khoản
                    ReceiverName = user?.FullName,
                    Phone = user?.PhoneNumber,
                    AddressDetail = user?.Address,
                    Email = user?.Email
                };
            }

            if (!string.IsNullOrWhiteSpace(vm.Form.CouponCode) && !vm.Gio.TrongRong)
            {
                var kq = await _coupons.KiemTraAsync(vm.Form.CouponCode, vm.Gio.TamTinh);
                if (kq.Ok) vm.TienGiam = kq.SoTienGiam;
            }

            return vm;
        }

        public async Task<ApMaResult> ApMaAsync(int userId, IReadOnlyList<GuestCartLine> gioKhach, string? code)
        {
            var dong = await LayDongGioAsync(userId, gioKhach);
            decimal tamTinh = dong.Where(c => c.ProductVariant != null).Sum(c => c.ProductVariant!.Price * c.Quantity);

            if (tamTinh <= 0)
                return new ApMaResult(false, "Giỏ hàng đang trống.", 0, 0, TienViet.Dinh(0), TienViet.Dinh(0));

            var kq = await _coupons.KiemTraAsync(code, tamTinh);
            if (!kq.Ok)
                return new ApMaResult(false, kq.Loi ?? "Mã không hợp lệ.", 0, tamTinh, TienViet.Dinh(0), TienViet.Dinh(tamTinh));

            decimal tong = tamTinh - kq.SoTienGiam;
            string moTa = kq.Coupon?.Description is { Length: > 0 } d ? $" ({d})" : "";
            return new ApMaResult(true, $"Đã áp mã {kq.Coupon!.Code}{moTa}: giảm {TienViet.Dinh(kq.SoTienGiam)}.",
                                  kq.SoTienGiam, tong, TienViet.Dinh(kq.SoTienGiam), TienViet.Dinh(tong));
        }

        public async Task<DatHangResult> DatHangAsync(int userId, IReadOnlyList<GuestCartLine> gioKhach, CheckoutForm form)
        {
            bool daDangNhap = userId > 0;

            // ===== 1. Đọc giỏ và kiểm tra từng dòng =====
            var gio = await LayDongGioAsync(userId, gioKhach);
            if (gio.Count == 0) return Loi("Giỏ hàng đang trống.");

            foreach (var c in gio)
            {
                var canhBao = CartService.TaoDong(c).CanhBao;
                if (canhBao != null) return Loi($"{c.ProductVariant!.Product!.Name}: {canhBao}");
            }

            // ===== 2. Địa chỉ nhận hàng =====
            // Email bắt buộc cho mọi đơn: shop gửi xác nhận, và là cách liên lạc với khách vãng lai
            string email = (form.Email ?? "").Trim();
            if (email.Length == 0) return Loi("Vui lòng nhập email.");

            string tenNhan, sdt, diaChi;
            // Khách vãng lai không có sổ địa chỉ: bỏ qua AddressId nếu ai đó cố gửi lên
            if (daDangNhap && form.AddressId.HasValue)
            {
                var dc = await _addresses.LayTheoIdAsync(form.AddressId.Value, userId);
                if (dc == null) return Loi("Địa chỉ đã chọn không tồn tại, vui lòng chọn lại.");
                tenNhan = dc.ReceiverName;
                sdt = dc.Phone;
                diaChi = dc.Detail;
            }
            else
            {
                tenNhan = (form.ReceiverName ?? "").Trim();
                sdt = (form.Phone ?? "").Trim();
                diaChi = (form.AddressDetail ?? "").Trim();
                if (tenNhan.Length < 2 || sdt.Length == 0 || diaChi.Length == 0)
                    return Loi("Vui lòng nhập đầy đủ người nhận, số điện thoại và địa chỉ.");
            }

            // ===== 3. Tiền hàng và mã giảm giá =====
            // Giá đọc lại từ biến thể ngay lúc này, không tin giá nào gửi lên từ trình duyệt
            decimal tamTinh = gio.Sum(c => c.ProductVariant!.Price * c.Quantity);

            Coupon? coupon = null;
            decimal tienGiam = 0;
            if (!string.IsNullOrWhiteSpace(form.CouponCode))
            {
                var kq = await _coupons.KiemTraAsync(form.CouponCode, tamTinh);
                if (!kq.Ok) return Loi(kq.Loi ?? "Mã giảm giá không hợp lệ.");
                coupon = kq.Coupon;
                tienGiam = kq.SoTienGiam;
            }

            // ===== 4. Ghi tất cả trong một transaction =====
            await using var tx = await _uow.BeginTransactionAsync();
            try
            {
                // 4a. Trừ tồn kho. Mỗi dòng là một câu UPDATE có điều kiện, hết hàng thì dừng.
                foreach (var c in gio)
                {
                    bool truDuoc = await _variants.TruTonKhoAsync(c.ProductVariantId, c.Quantity);
                    if (!truDuoc)
                    {
                        await tx.RollbackAsync();
                        var v = c.ProductVariant!;
                        return Loi($"{v.Product!.Name} ({PhanLoai.Ten(v.Storage, v.Color)}) vừa hết hàng hoặc không đủ số lượng. "
                                 + "Vui lòng kiểm tra lại giỏ hàng.");
                    }
                }

                // 4b. Cộng lượt dùng mã. Hai người cùng dùng lượt cuối thì chỉ một người được.
                if (coupon != null && !await _coupons.TangLuotDungAsync(coupon.Id))
                {
                    await tx.RollbackAsync();
                    return Loi("Mã giảm giá vừa hết lượt sử dụng, vui lòng bỏ mã hoặc dùng mã khác.");
                }

                // 4c. Tạo đơn, chốt giá và tên tại thời điểm mua
                var don = new Order
                {
                    CustomerName = tenNhan,
                    Phone = sdt,
                    Address = diaChi,
                    Email = email,
                    Note = string.IsNullOrWhiteSpace(form.Note) ? null : form.Note.Trim(),
                    PaymentMethod = form.PaymentMethod,
                    Status = OrderStatus.ChoXacNhan,
                    Subtotal = tamTinh,
                    DiscountAmount = tienGiam,
                    Total = tamTinh - tienGiam,
                    CouponId = coupon?.Id,
                    CouponCode = coupon?.Code,
                    IsPaid = false,
                    CreatedAt = DateTime.Now,
                    // Khách vãng lai thì UserId null, admin thấy nhãn "khách lẻ"
                    UserId = daDangNhap ? userId : null
                };

                foreach (var c in gio)
                {
                    var v = c.ProductVariant!;
                    don.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = v.ProductId,
                        ProductVariantId = v.Id,
                        ProductName = v.Product!.Name,
                        VariantName = PhanLoai.Ten(v.Storage, v.Color),
                        Quantity = c.Quantity,
                        UnitPrice = v.Price
                    });
                }

                _orders.Them(don);
                await LuuVoiMaDonMoiAsync(don);

                // 4d. Làm trống giỏ trong CSDL. Giỏ vãng lai là CartItem tạm, không có trong CSDL,
                //     controller sẽ xoá giỏ Session sau khi đặt thành công.
                if (daDangNhap)
                {
                    _cart.XoaNhieu(gio);
                    await _uow.SaveChangesAsync();
                }

                // 4e. Địa chỉ mới thì lưu vào sổ nếu khách đã đăng nhập và muốn lưu
                if (daDangNhap && !form.AddressId.HasValue && form.LuuDiaChi)
                {
                    await _addresses.LuuAsync(new Address
                    {
                        ReceiverName = tenNhan,
                        Phone = sdt,
                        Detail = diaChi
                    }, userId);
                }

                await tx.CommitAsync();
                return new DatHangResult(true, null, don);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Đặt hàng thất bại (user {UserId}, 0 là khách vãng lai)", userId);
                await tx.RollbackAsync();
                return Loi("Có lỗi khi tạo đơn hàng, vui lòng thử lại.");
            }
        }

        /// <summary>
        /// Mã đơn dạng DH20261104-0001, đếm lại từ 1 mỗi ngày. Hai đơn cùng lúc có thể
        /// lấy trùng số; chỉ mục unique trên OrderCode chặn lại, khi đó lấy số tiếp và thử lại.
        /// </summary>
        private async Task LuuVoiMaDonMoiAsync(Order don)
        {
            string tienTo = $"DH{DateTime.Now:yyyyMMdd}-";

            for (int lan = 0; ; lan++)
            {
                string? lonNhat = await _orders.LayMaLonNhatAsync(tienTo);
                int so = 0;
                if (lonNhat != null) int.TryParse(lonNhat[tienTo.Length..], out so);
                don.OrderCode = $"{tienTo}{so + 1 + lan:0000}";

                try
                {
                    await _uow.SaveChangesAsync();
                    return;
                }
                catch (DbUpdateException) when (lan < 4)
                {
                    // Trùng mã với đơn vừa được tạo cùng lúc, thử số tiếp theo
                }
            }
        }

        /// <summary>
        /// Các dòng giỏ kèm biến thể và sản phẩm. Khách đã đăng nhập đọc từ CartItems (có theo dõi,
        /// để còn xoá sau khi đặt). Khách vãng lai dựng CartItem tạm từ danh sách trong Session.
        /// </summary>
        private async Task<List<CartItem>> LayDongGioAsync(int userId, IReadOnlyList<GuestCartLine> gioKhach)
        {
            if (userId > 0)
                return (await _cart.LayCuaUserAsync(userId))
                    .Where(c => c.ProductVariant?.Product != null)
                    .ToList();

            var bienThe = (await _variants.LayNhieuKemSanPhamAsync(gioKhach.Select(x => x.VariantId)))
                .ToDictionary(v => v.Id);

            return gioKhach
                .Where(x => x.Quantity > 0 && bienThe.TryGetValue(x.VariantId, out var v) && v.Product != null)
                .Select(x => new CartItem
                {
                    Id = x.VariantId,
                    ProductVariantId = x.VariantId,
                    ProductVariant = bienThe[x.VariantId],
                    Quantity = x.Quantity
                })
                .ToList();
        }

        private static DatHangResult Loi(string loi) => new(false, loi, null);
    }
}
