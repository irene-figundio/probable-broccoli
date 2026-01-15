using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class Availability
{
    public int Id { get; set; }

    public Guid TenantId { get; set; }

    public int? StaffId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public string? Note { get; set; }

    public virtual Staff? Staff { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
    public string StaffName { get; set; }
}
