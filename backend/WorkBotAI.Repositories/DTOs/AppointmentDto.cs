

// DTO per la lista appuntamenti
public class AppointmentListDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? StaffId { get; set; }
    public string? StaffName { get; set; }
    public int? ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public decimal? TotalPrice { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
}

// DTO per dettaglio singolo appuntamento
public class AppointmentDetailDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
    public int? StaffId { get; set; }
    public string? StaffName { get; set; }
    public int? ResourceId { get; set; }
    public string? ResourceName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Note { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public List<AppointmentServiceDto> Services { get; set; } = new();
}

// DTO per i servizi dell'appuntamento
public class AppointmentServiceDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public int? Duration { get; set; }
}

// DTO per creare un nuovo appuntamento
public class CreateAppointmentDto
{
    public Guid TenantId { get; set; }
    public int? CustomerId { get; set; }
    public int? StaffId { get; set; }
    public int? ResourceId { get; set; }
    public int StatusId { get; set; } = 1; // Default: pending
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Note { get; set; }
}

// DTO per aggiornare un appuntamento
public class UpdateAppointmentDto
{
    public int? CustomerId { get; set; }
    public int? StaffId { get; set; }
    public int? ResourceId { get; set; }
    public int StatusId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Note { get; set; }
}

// DTO per gli stati appuntamento
public class AppointmentStatusDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}