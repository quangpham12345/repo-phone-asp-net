namespace ShopBanHang.Models.Dtos
{
    /// <summary>
    /// Hình dạng dữ liệu tài khoản khi trả ra API.
    ///
    /// Không trả trực tiếp entity User vì sẽ lộ PasswordHash, và navigation
    /// property Orders - User - Orders làm bộ chuyển JSON lặp vô tận.
    /// </summary>
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>Một trang dữ liệu trả ra API, kèm thông tin phân trang.</summary>
    public class PagedDto<T>
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; } = new();
    }
}
