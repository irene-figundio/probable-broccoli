

// Lista disponibilità
public class AvailabilityListDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public int? StaffId { get; set; }
    public string? StaffName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Note { get; set; }
}

// Dettaglio disponibilità
public class AvailabilityDetailDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public int? StaffId { get; set; }
    public string? StaffName { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Note { get; set; }
}

// Creazione disponibilità
public class CreateAvailabilityDto
{
    public Guid TenantId { get; set; }
    public int? StaffId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Note { get; set; }
}

// Aggiornamento disponibilità
public class UpdateAvailabilityDto
{
    public int? StaffId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? Note { get; set; }
}

// Slot disponibile per prenotazione
public class AvailableSlotDto
{
    public int StaffId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}