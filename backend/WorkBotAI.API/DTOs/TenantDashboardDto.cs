namespace WorkBotAI.API.DTOs;

// DTO principale per la dashboard del tenant
public class TenantDashboardDto
{
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public object? Stats { get; set; }
    public List<WorkBotAI.API.DTOs.AppointmentListDto> UpcomingAppointments { get; set; } = new();
    public List<WorkBotAI.API.DTOs.AppointmentListDto> RecentAppointments { get; set; } = new();
}

// DTO per le statistiche dashboard del tenant
public class TenantDashboardStatsDto
{
    public int AppointmentsToday { get; set; }
    public int AppointmentsWeek { get; set; }
    public int AppointmentsMonth { get; set; }
    public int CustomersTotal { get; set; }
    public int CompletedAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int CancelledAppointments { get; set; }
}

// DTO per appuntamenti recenti del tenant
public class TenantAppointmentDto
{
    public int Id { get; set; }
    public string? CustomerName { get; set; }
    public string? StaffName { get; set; }
    public string? StatusName { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Note { get; set; }
}
