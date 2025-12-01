using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class ResourceType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
