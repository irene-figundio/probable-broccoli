using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class Service
{
    public int Id { get; set; }

    public Guid TenantId { get; set; }

    public int? CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? DurationMin { get; set; }

    public decimal? BasePrice { get; set; }

    public decimal? BafferTime { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreationTime { get; set; }

    public int? CreationUserId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public int? LastModificationUserId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletionTime { get; set; }

    public int? DeletionUserId { get; set; }

    public virtual ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>();

    public virtual Category? Category { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
