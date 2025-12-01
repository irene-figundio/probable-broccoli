using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Faq> Faqs { get; set; } = new List<Faq>();

    public virtual ICollection<JobType> JobTypes { get; set; } = new List<JobType>();

    public virtual ICollection<ResourceType> ResourceTypes { get; set; } = new List<ResourceType>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
}
