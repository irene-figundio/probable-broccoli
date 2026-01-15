

// Lista staff
public class StaffListDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? JobTypeId { get; set; }
    public string? JobTypeName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreationTime { get; set; }
    public int AppointmentsCount { get; set; }
}

// Dettaglio staff
public class StaffDetailDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? JobTypeId { get; set; }
    public string? JobTypeName { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreationTime { get; set; }
    public bool IsDeleted { get; set; }
    public List<StaffAppointmentDto> RecentAppointments { get; set; } = new();
}

// Appuntamento dello staff
public class StaffAppointmentDto
{
    public int Id { get; set; }
    public DateTime? StartTime { get; set; }
    public string? CustomerName { get; set; }
    public string? StatusName { get; set; }
    public string? Note { get; set; }
}

// Creazione staff
public class CreateStaffDto
{
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int? JobTypeId { get; set; }
}

// Aggiornamento staff
public class UpdateStaffDto
{
    public string Name { get; set; } = string.Empty;
    public int? JobTypeId { get; set; }
    public bool IsActive { get; set; }
}