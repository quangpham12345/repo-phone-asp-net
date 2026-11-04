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

        public async Task<CheckoutViewModel> ChuanBiAsync(int userId, CheckoutForm? form = null)
        {
            var dong = await _cart.LayCuaUserAsync(userId);
            var vm = new CheckoutViewModel
            {
                Gio = new CartViewModel { Lines = dong.Where(c => c.ProductVariant?.Product != null).Select(CartService.TaoDong).ToList() },
                CacDiaChi = await _addresses.LayCuaUserAsync(userId)
            };

            if (form != null)
            {
                // Trang được vẽ lại sau lỗi: giữ nguyên những gì khách đã nhập
                vm.Form = form;
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

        public async Task<ApMaResult> ApMaAsync(int userId, string? code)
        {
            var dong = await _cart.LayCuaUserAsync(userId);
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

        public async Task<DatHangResult> DatHangAsync(int userId, CheckoutForm form)
        {
            // ===== 1. Đọc giỏ và kiểm tra từng dòng =====
            var gio = await _cart.LayCuaUserAsync(userId);
            gio = gio.Where(c => c.ProductVariant?.Product != null).ToList();
            if (gio.Count == 0) return Loi("Giỏ hàng đang trống.");

            foreach (var c in gio)
            {
                var canhBao = CartService.TaoDong(c).CanhBao;
                if (canhBao != null) return Loi($"{c.ProductVariant!.Product!.Name}: {canhBao}");
            }

            // ===== 2. Địa chỉ nhận hàng =====
            string tenNhan, sdt, diaChi;
            if (form.AddressId.HasValue)
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
                    Email = string.IsNullOrWhiteSpace(form.Email) ? null : form.Email.Trim(),
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
                    UserId = userId
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

                // 4d. Làm trống giỏ
                _cart.XoaNhieu(gio);
                await _uow.SaveChangesAsync();

                // 4e. Địa chỉ mới thì lưu vào sổ nếu khách muốn
                if (!form.AddressId.HasValue && form.LuuDiaChi)
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
                _log.LogError(ex, "Đặt hàng thất bại cho user {UserId}", userId);
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

        private static DatHangResult Loi(string loi) => new(false, loi, null);
    }
}
