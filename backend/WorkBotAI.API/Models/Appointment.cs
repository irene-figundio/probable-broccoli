using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class Appointment
{
    public int Id { get; set; }

    public Guid TenantId { get; set; }

    public int? CustomerId { get; set; }

    public int? StatusId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Note { get; set; }

    public bool? IsActive { get; set; }

    public int? StaffId { get; set; }

    public int? ResourceId { get; set; }

    public DateTime? CreationTime { get; set; }

    public int? CreationUserId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public int? LastModificationUserId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletionTime { get; set; }

    public int? DeletionUserId { get; set; }

    public virtual ICollection<AppointmentPayment> AppointmentPayments { get; set; } = new List<AppointmentPayment>();

    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public virtual Customer? Customer { get; set; }

    public virtual Resource? Resource { get; set; }

    public virtual Staff? Staff { get; set; }

    public virtual AppointmentStatus? Status { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
