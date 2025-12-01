using Microsoft.EntityFrameworkCore;

namespace  WorkbotAI.Repositories.DataAccess.Abstraction
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> If<T>(this IQueryable<T> q, bool condition, Func<IQueryable<T>, IQueryable<T>> then)
            => condition ? then(q) : q;

        public static async Task<bool> AnyAsyncSafe<T>(this IQueryable<T> q, CancellationToken ct = default)
            => await q.AnyAsync(ct);
    }
}
