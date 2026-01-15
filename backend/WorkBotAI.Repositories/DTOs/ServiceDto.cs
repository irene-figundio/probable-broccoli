

// Lista servizi
public class ServiceListDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? CategoryName { get; set; }
    public int? DurationMin { get; set; }
    public decimal? BasePrice { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreationTime { get; set; }
    public int AppointmentsCount { get; set; }
    public int? CategoryId { get; internal set; }
}

// Dettaglio servizio
public class ServiceDetailDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMin { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? BufferTime { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreationTime { get; set; }
    public bool IsDeleted { get; set; }
}

// Creazione servizio
public class CreateServiceDto
{
    public Guid TenantId { get; set; }
    public int? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMin { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? BufferTime { get; set; }
}

// Aggiornamento servizio
public class UpdateServiceDto
{
    public int? CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? DurationMin { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? BufferTime { get; set; }
    public bool IsActive { get; set; }
}