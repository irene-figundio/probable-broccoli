using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class Module
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public int? PlaneId { get; set; }

    public virtual Plane? Plane { get; set; }

    public virtual ICollection<Submodule> Submodules { get; set; } = new List<Submodule>();
}
