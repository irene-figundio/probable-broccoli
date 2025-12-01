namespace WorkbotAI.Models.Abstractions
{
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTimeOffset? DeletionTime { get; set; }
        string? DeletionUserId { get; set; }
    }

    public abstract class SoftDeletableEntity : ISoftDeletable
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletionTime { get; set; }
        public string? DeletionUserId { get; set; }
    }
    public interface ICurrentUserService
    {
        string? UserId { get; }
    }
}
