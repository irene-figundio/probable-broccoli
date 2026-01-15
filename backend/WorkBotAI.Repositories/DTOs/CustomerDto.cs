
    // Lista clienti
    public class CustomerListDto
    {
        public int Id { get; set; }
        public Guid TenantId { get; set; }
        public string? TenantName { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Note { get; set; }
        public DateTime? CreationTime { get; set; }
        public int AppointmentsCount { get; set; }
    }

    // Dettaglio cliente
    public class CustomerDetailDto
    {
        public int Id { get; set; }
        public Guid TenantId { get; set; }
        public string? TenantName { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Note { get; set; }
        public DateTime? CreationTime { get; set; }
        public bool IsDeleted { get; set; }
        public List<CustomerAppointmentDto> RecentAppointments { get; set; } = new();
    }

    // Appuntamento del cliente
    public class CustomerAppointmentDto
    {
        public int Id { get; set; }
        public DateTime? StartTime { get; set; }
        public string? StaffName { get; set; }
        public string? StatusName { get; set; }
        public string? Note { get; set; }
    }

    // Creazione cliente
    public class CreateCustomerDto
    {
        public Guid TenantId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Note { get; set; }
    }

    // Aggiornamento cliente
    public class UpdateCustomerDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Note { get; set; }
    }

