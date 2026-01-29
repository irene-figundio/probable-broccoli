using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class JobType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
