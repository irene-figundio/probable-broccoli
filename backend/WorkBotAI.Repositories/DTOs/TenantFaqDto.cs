

// ============================================
// FAQ DTOs (Domande di sistema)
// ============================================

public class FaqListDto
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class FaqDetailDto
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<TenantFaqListDto> TenantAnswers { get; set; } = new();
}

public class CreateFaqDto
{
    public int? CategoryId { get; set; }
    public string Question { get; set; } = string.Empty;
}

public class UpdateFaqDto
{
    public int? CategoryId { get; set; }
    public string Question { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// ============================================
// TENANT FAQ DTOs (Risposte personalizzate)
// ============================================

public class TenantFaqListDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public int FaqId { get; set; }
    public string? Question { get; set; }
    public string? CategoryName { get; set; }
    public string? Value { get; set; }
    public bool IsActive { get; set; }
}

public class TenantFaqDetailDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public int FaqId { get; set; }
    public string? Question { get; set; }
    public string? CategoryName { get; set; }
    public string? Value { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

public class CreateTenantFaqDto
{
    public Guid TenantId { get; set; }
    public int FaqId { get; set; }
    public string? Value { get; set; }
}

public class UpdateTenantFaqDto
{
    public string? Value { get; set; }
    public bool IsActive { get; set; }
}

public class UpsertTenantFaqDto
{
    public int FaqId { get; set; }
    public string? Value { get; set; }
}

public class ChatbotFaqDto
{
    public string Question { get; set; } = string.Empty;
    public string? Answer { get; set; }
    public string? Category { get; set; }
}

// ============================================
// SIMPLE FAQ DTO (per gestione diretta tenant)
// ============================================

public class SimpleFaqDto
{
    public Guid TenantId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}