using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class Submodule
{
    public int Id { get; set; }

    public int ModuleId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public int? PlaneId { get; set; }

    public DateTime? CreationTime { get; set; }

    public int? CreationUserId { get; set; }

    public virtual Module Module { get; set; } = null!;

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    public virtual Plane? Plane { get; set; }
}
