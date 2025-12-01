namespace  WorkbotAI.Repositories.DataAccess.Abstraction
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}
