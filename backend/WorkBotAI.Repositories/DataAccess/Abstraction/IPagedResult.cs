namespace  WorkbotAI.Repositories.DataAccess.Abstraction
{
    public interface IPagedResult<T>
    {
        int Page { get; }
        int PageSize { get; }
        int TotalCount { get; }
        IReadOnlyList<T> Items { get; }
    }

    public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount) : IPagedResult<T>;
}
