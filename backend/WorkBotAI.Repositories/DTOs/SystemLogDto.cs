

public class SystemLogDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Context { get; set; }
    public int? UserId { get; set; }
    public Guid? TenantId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class CreateLogDto
{
    public string Level { get; set; } = "info";
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Context { get; set; }
    public int? UserId { get; set; }
    public Guid? TenantId { get; set; }
}

public class LogFilterDto
{
    public string? Level { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? TenantId { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class LogsResponseDto
{
    public List<SystemLogDto> Logs { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}