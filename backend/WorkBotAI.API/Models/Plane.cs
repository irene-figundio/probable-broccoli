using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class Plane
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Module> Modules { get; set; } = new List<Module>();

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    public virtual ICollection<Submodule> Submodules { get; set; } = new List<Submodule>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
