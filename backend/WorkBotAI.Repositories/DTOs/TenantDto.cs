
// DTO per la lista tenant
public class TenantListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Acronym { get; set; }
    public string? LogoImage { get; set; }
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreationDate { get; set; }
    public string? SubscriptionPlan { get; set; }
    public string? SubscriptionStatus { get; set; }
    public int TotalUsers { get; set; }
    public int TotalAppointments { get; set; }
}

// DTO per dettaglio singolo tenant
public class TenantDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Acronym { get; set; }
    public string? LogoImage { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreationDate { get; set; }
    public SubscriptionInfoDto? Subscription { get; set; }
    public int TotalUsers { get; set; }
    public int TotalAppointments { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalServices { get; set; }
}

// DTO per info abbonamento
public class SubscriptionInfoDto
{
    public int Id { get; set; }
    public string? PlanName { get; set; }
    public string? Status { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

// DTO per creare un nuovo tenant
public class CreateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Acronym { get; set; }
    public string? LogoImage { get; set; }
    public int? CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}

// DTO per aggiornare un tenant
public class UpdateTenantDto
{
    public string Name { get; set; } = string.Empty;
    public string? Acronym { get; set; }
    public string? LogoImage { get; set; }
    public int? CategoryId { get; set; }
    public bool IsActive { get; set; }
}