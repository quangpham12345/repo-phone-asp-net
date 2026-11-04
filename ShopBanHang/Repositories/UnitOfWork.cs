using Microsoft.EntityFrameworkCore.Storage;
using ShopBanHang.Data;
using ShopBanHang.Repositories.Interfaces;

namespace ShopBanHang.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ShopContext _db;

        public UnitOfWork(ShopContext db) => _db = db;

        public Task<int> SaveChangesAsync() => _db.SaveChangesAsync();

        public Task<IDbContextTransaction> BeginTransactionAsync() => _db.Database.BeginTransactionAsync();
    }
}
