using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ShopBanHang.Data;
using ShopBanHang.Models.Entities;
using ShopBanHang.Models.Enums;
using ShopBanHang.Models.ViewModels;
using ShopBanHang.Services.Interfaces;

namespace ShopBanHang.Services
{
    public class CouponService : ICouponService
    {
        private static readonly CultureInfo Vi = new("vi-VN");

        private readonly ShopContext _db;

        public CouponService(ShopContext db) => _db = db;

        // ====================== Khu quản trị ======================

        public async Task<PagedResult<Coupon>> LayDanhSachAsync(
            string? tuKhoa, CouponStatus? trangThai, int page, int pageSize)
        {
            var now = DateTime.Now;
            IQueryable<Coupon> q = _db.Coupons.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string k = tuKhoa.Trim();
                q = q.Where(c => c.Code.Contains(k) || (c.Description != null && c.Description.Contains(k)));
            }

            // Các điều kiện này phải khớp với Coupon.LayTrangThai
            q = trangThai switch
            {
                CouponStatus.DaTat => q.Where(c => !c.IsActive),
                CouponStatus.HetHan => q.Where(c => c.IsActive && c.EndDate < now),
                CouponStatus.SapDienRa => q.Where(c => c.IsActive && c.EndDate >= now && c.StartDate > now),
                CouponStatus.HetLuot => q.Where(c => c.IsActive && c.EndDate >= now && c.StartDate <= now
                                                     && c.UsageLimit != null && c.UsedCount >= c.UsageLimit),
                CouponStatus.DangChay => q.Where(c => c.IsActive && c.EndDate >= now && c.StartDate <= now
                                                      && (c.UsageLimit == null || c.UsedCount < c.UsageLimit)),
                _ => q
            };

            return await PagedResult<Coupon>.TaoAsync(
                q.OrderByDescending(c => c.CreatedAt).ThenByDescending(c => c.Id),
                page, pageSize);
        }

        public async Task<Coupon?> LayTheoIdAsync(int id)
            => await _db.Coupons.FindAsync(id);

        public async Task<(bool ok, string? loi)> LuuAsync(Coupon coupon)
        {
            coupon.Code = (coupon.Code ?? string.Empty).Trim().ToUpperInvariant();
            coupon.Description = string.IsNullOrWhiteSpace(coupon.Description) ? null : coupon.Description.Trim();

            string? loiHopLe = KiemTraHopLe(coupon);
            if (loiHopLe != null) return (false, loiHopLe);

            bool trung = await _db.Coupons.AnyAsync(c => c.Code == coupon.Code && c.Id != coupon.Id);
            if (trung) return (false, "Mã giảm giá này đã tồn tại.");

            // Ngày bắt đầu tính từ 00:00, ngày kết thúc tính đến 23:59:59 để mã dùng được trọn ngày cuối
            var batDau = coupon.StartDate.Date;
            var ketThuc = coupon.EndDate.Date.AddDays(1).AddSeconds(-1);

            // Giảm số tiền cố định thì không có trần
            decimal? tran = coupon.DiscountType == DiscountType.PhanTram ? coupon.MaxDiscount : null;

            if (coupon.Id == 0)
            {
                coupon.StartDate = batDau;
                coupon.EndDate = ketThuc;
                coupon.MaxDiscount = tran;
                coupon.UsedCount = 0;
                coupon.CreatedAt = DateTime.Now;
                _db.Coupons.Add(coupon);
            }
            else
            {
                var cu = await _db.Coupons.FindAsync(coupon.Id);
                if (cu == null) return (false, "Không tìm thấy mã giảm giá cần sửa.");

                // Mã đã có đơn dùng thì giữ nguyên chữ mã để đối chiếu đơn cũ
                if (cu.UsedCount > 0 && cu.Code != coupon.Code)
                    return (false, "Mã này đã được sử dụng nên không đổi được chữ mã. Hãy tắt mã và tạo mã mới.");

                if (coupon.UsageLimit != null && coupon.UsageLimit < cu.UsedCount)
                    return (false, $"Mã đã được dùng {cu.UsedCount} lượt, số lượt tối đa không được nhỏ hơn con số này.");

                cu.Code = coupon.Code;
                cu.Description = coupon.Description;
                cu.DiscountType = coupon.DiscountType;
                cu.DiscountValue = coupon.DiscountValue;
                cu.MaxDiscount = tran;
                cu.MinOrderValue = coupon.MinOrderValue;
                cu.UsageLimit = coupon.UsageLimit;
                cu.StartDate = batDau;
                cu.EndDate = ketThuc;
                cu.IsActive = coupon.IsActive;
            }

            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> DoiTrangThaiAsync(int id)
        {
            var c = await _db.Coupons.FindAsync(id);
            if (c == null) return (false, "Không tìm thấy mã giảm giá.");

            c.IsActive = !c.IsActive;
            await _db.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool ok, string? loi)> XoaAsync(int id)
        {
            var c = await _db.Coupons.FindAsync(id);
            if (c == null) return (false, "Không tìm thấy mã giảm giá.");

            if (c.UsedCount > 0)
                return (false, "Mã này đã được dùng trong đơn hàng nên không xoá được. Hãy tắt mã thay vì xoá.");

            _db.Coupons.Remove(c);
            await _db.SaveChangesAsync();
            return (true, null);
        }

        // ====================== Dùng ở thanh toán ======================

        public async Task<CouponKetQua> KiemTraAsync(string? code, decimal tongTien)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Loi("Vui lòng nhập mã giảm giá.");

            string ma = code.Trim().ToUpperInvariant();
            var c = await _db.Coupons.AsNoTracking().FirstOrDefaultAsync(x => x.Code == ma);
            if (c == null) return Loi("Mã giảm giá không tồn tại.");

            var now = DateTime.Now;
            if (!c.IsActive) return Loi("Mã giảm giá này hiện không còn hiệu lực.");
            if (c.StartDate > now) return Loi($"Mã giảm giá áp dụng từ ngày {c.StartDate:dd/MM/yyyy}.");
            if (c.EndDate < now) return Loi("Mã giảm giá đã hết hạn.");
            if (!c.ConLuot) return Loi("Mã giảm giá đã hết lượt sử dụng.");

            if (tongTien < c.MinOrderValue)
                return Loi($"Đơn hàng tối thiểu {c.MinOrderValue.ToString("N0", Vi)}đ mới dùng được mã này.");

            return new CouponKetQua(true, null, c, TinhGiam(c, tongTien));
        }

        public decimal TinhGiam(Coupon coupon, decimal tongTien)
        {
            decimal giam = coupon.DiscountType == DiscountType.PhanTram
                ? Math.Round(tongTien * coupon.DiscountValue / 100m, 0)
                : coupon.DiscountValue;

            if (coupon.DiscountType == DiscountType.PhanTram && coupon.MaxDiscount.HasValue)
                giam = Math.Min(giam, coupon.MaxDiscount.Value);

            // Không bao giờ giảm quá tổng tiền hàng
            return Math.Max(0, Math.Min(giam, tongTien));
        }

        public async Task<bool> TangLuotDungAsync(int id)
        {
            // Một câu UPDATE có điều kiện: hai người cùng dùng lượt cuối thì chỉ một người thành công
            int dong = await _db.Coupons
                .Where(c => c.Id == id && (c.UsageLimit == null || c.UsedCount < c.UsageLimit))
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.UsedCount, c => c.UsedCount + 1));

            return dong > 0;
        }

        // ====================== Nội bộ ======================

        private static CouponKetQua Loi(string loi) => new(false, loi, null, 0);

        private static string? KiemTraHopLe(Coupon c)
        {
            if (string.IsNullOrEmpty(c.Code)) return "Vui lòng nhập mã giảm giá.";

            if (c.DiscountType == DiscountType.PhanTram && (c.DiscountValue < 1 || c.DiscountValue > 100))
                return "Giảm theo % thì giá trị phải từ 1 đến 100.";

            if (c.DiscountType == DiscountType.SoTien && c.DiscountValue <= 0)
                return "Số tiền giảm phải lớn hơn 0.";

            if (c.EndDate.Date < c.StartDate.Date)
                return "Ngày kết thúc phải sau hoặc bằng ngày bắt đầu.";

            return null;
        }
    }
}