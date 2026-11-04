using Microsoft.EntityFrameworkCore.Storage;

namespace ShopBanHang.Repositories.Interfaces
{
    /// <summary>
    /// Gom việc lưu và mở transaction về một chỗ, để service điều phối nhiều
    /// repository trong cùng một giao dịch mà không phải cầm ShopContext.
    ///
    /// Các repository và UnitOfWork cùng dùng chung một ShopContext (đăng ký Scoped),
    /// nên một lần SaveChangesAsync ở đây lưu thay đổi của tất cả repository.
    /// </summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
