using System;
using System.Collections.Concurrent;

namespace  WorkbotAI.Repositories.DataAccess.Abstraction
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _db;
        public UnitOfWork(ApplicationDbContext db) => _db = db;

        public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

        public ValueTask DisposeAsync() => _db.DisposeAsync();
    }
}
