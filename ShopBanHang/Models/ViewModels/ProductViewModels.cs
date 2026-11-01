using System.ComponentModel.DataAnnotations;
using ShopBanHang.Models.Entities;

namespace ShopBanHang.Models.ViewModels
{
    /// <summary>Cách sắp xếp danh sách sản phẩm.</summary>
    public enum ProductSort
    {
        [Display(Name = "Mới nhất")]
        MoiNhat = 0,

        [Display(Name = "Giá thấp đến cao")]
        GiaTang = 1,

        [Display(Name = "Giá cao đến thấp")]
        GiaGiam = 2,

        [Display(Name = "Bán chạy")]
        BanChay = 3
    }

    /// <summary>
    /// Điều kiện lọc danh sách sản phẩm. Gom thành một đối tượng để khỏi
    /// truyền sáu tham số rời qua các tầng.
    /// </summary>
    public class ProductFilter
    {
        [Display(Name = "Từ khoá")]
        public string? TuKhoa { get; set; }

        [Display(Name = "Danh mục")]
        public string? DanhMuc { get; set; }        // theo slug để URL đọc được

        [Display(Name = "Thương hiệu")]
        public string? ThuongHieu { get; set; }     // theo slug

        [Display(Name = "Giá từ")]
        public decimal? GiaTu { get; set; }

        [Display(Name = "Giá đến")]
        public decimal? GiaDen { get; set; }

        [Display(Name = "RAM")]
        public string? Ram { get; set; }

        [Display(Name = "Dung lượng")]
        public string? Storage { get; set; }

        [Display(Name = "Hệ điều hành")]
        public string? Os { get; set; }

        [Display(Name = "Sắp xếp")]
        public ProductSort SapXep { get; set; } = ProductSort.MoiNhat;

        /// <summary>Chỉ lấy hàng đang bán. Phía admin đặt false để xem cả hàng đã ẩn.</summary>
        public bool ChiHangDangBan { get; set; } = true;
    }

    /// <summary>Dữ liệu cho trang danh sách sản phẩm phía khách.</summary>
    public class ProductListViewModel
    {
        public PagedResult<Product> KetQua { get; set; } = new();
        public ProductFilter Loc { get; set; } = new();

        public List<Category> CacDanhMuc { get; set; } = new();
        public List<Brand> CacThuongHieu { get; set; } = new();
        public List<string> CacRam { get; set; } = new();
        public List<string> CacStorage { get; set; } = new();
        public List<string> CacOs { get; set; } = new();
        /// <summary>Tiêu đề trang, đổi theo danh mục đang xem.</summary>
        public string TieuDe { get; set; } = "Tất cả sản phẩm";
        /// <summary>Id sản phẩm đang có biến thể (hoặc chính nó) sắp hết hàng, để đánh dấu trong danh sách admin.</summary>
        public HashSet<int> SanPhamCanhBao { get; set; } = new();
    }

    /// <summary>Dữ liệu trang chi tiết sản phẩm.</summary>
    public class ProductDetailViewModel
    {
        public Product SanPham { get; set; } = null!;
        public List<Product> LienQuan { get; set; } = new();
        public List<Product> PhienBanKhac { get; set; } = new();
    }

    public class ProductSuggestItem
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
    }

    public class ProductCompareViewModel
    {
        public List<Product> SanPham { get; set; } = new();
    }

    /// <summary>Một dòng cảnh báo tồn kho: một biến thể, hoặc một sản phẩm không có biến thể.</summary>
    public class CanhBaoTonKhoItem
    {
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? Anh { get; set; }

        /// <summary>Ví dụ "512GB · Titan Đen". Null nếu sản phẩm không có biến thể.</summary>
        public string? BienThe { get; set; }

        public int Ton { get; set; }
        public bool DaHet => Ton <= 0;
    }
}
