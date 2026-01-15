

// DTO per la lista abbonamenti
public class SubscriptionListDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantAcronym { get; set; }
    public int? PlaneId { get; set; }
    public string? PlaneName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsExpired => EndDate.HasValue && EndDate.Value < DateOnly.FromDateTime(DateTime.Today);
    public int DaysRemaining => EndDate.HasValue 
        ? (EndDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days 
        : 0;
}

// DTO per dettaglio singolo abbonamento
public class SubscriptionDetailDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string? TenantAcronym { get; set; }
    public int? PlaneId { get; set; }
    public string? PlaneName { get; set; }
    public string? PlaneDescription { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public bool IsExpired => EndDate.HasValue && EndDate.Value < DateOnly.FromDateTime(DateTime.Today);
    public int DaysRemaining => EndDate.HasValue 
        ? (EndDate.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Today).Days 
        : 0;
    public int TotalPayments { get; set; }
    public decimal TotalPaid { get; set; }
}

// DTO per creare un nuovo abbonamento
public class CreateSubscriptionDto
{
    public Guid TenantId { get; set; }
    public int PlaneId { get; set; }
    public int StatusId { get; set; } = 1; // Default: active
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

// DTO per aggiornare un abbonamento
public class UpdateSubscriptionDto
{
    public int PlaneId { get; set; }
    public int StatusId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

// DTO per i piani disponibili
public class PlaneDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

// DTO per gli stati abbonamento
public class SubscriptionStatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}