using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class Staff
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public Guid TenantId { get; set; }

    public int? JobTypeId { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreationTime { get; set; }

    public int? CreationUserId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public int? LastModificationUserId { get; set; }

    public DateTime? DeletionTime { get; set; }

    public int? DeletionUserId { get; set; }

    public bool? IsDeleted { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public virtual JobType? JobType { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
