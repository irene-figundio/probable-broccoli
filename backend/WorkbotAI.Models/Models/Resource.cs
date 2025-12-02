using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class Resource
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public int? NumeroPosti { get; set; }

    public bool? IsAvailable { get; set; }

    public int? TypeId { get; set; }

    public Guid TenantId { get; set; }

    public DateTime? CreationTime { get; set; }

    public int? CreationUserId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public int? LastModificationUserId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletionTime { get; set; }

    public int? DeletionUserId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ResourceType? Type { get; set; }
}
