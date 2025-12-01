using System;

namespace WorkBotAI.API.Models;

public class SystemLog
{
    public int Id { get; set; }
    
    public DateTime Timestamp { get; set; }
    
    public string Level { get; set; } = string.Empty; // info, warning, error, debug
    
    public string Source { get; set; } = string.Empty; // Controller/Service che ha generato il log
    
    public string Message { get; set; } = string.Empty;
    
    public string? Context { get; set; } // JSON con dati aggiuntivi
    
    public int? UserId { get; set; }
    
    public Guid? TenantId { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? UserAgent { get; set; }
}