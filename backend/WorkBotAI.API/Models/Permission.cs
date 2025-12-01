using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class Permission
{
    public int Id { get; set; }

    public int SubmoduleId { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public bool? IsMinimum { get; set; }

    public int? PlaneId { get; set; }

    public virtual Plane? Plane { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual Submodule Submodule { get; set; } = null!;
}
