namespace WorkbotAI.Models.Abstractions
{
    public interface IHasModificationAudit
    {
        DateTimeOffset? LastModificationTime { get; set; }
        string? LastModificationUserId { get; set; }
    }
}
