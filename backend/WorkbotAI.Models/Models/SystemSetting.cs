namespace WorkbotAI.Models;

public class SystemSetting
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty; // general, email, security, notifications, appearance
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Description { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}