using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class ContactInfo
{
    public int Id { get; set; }

    public Guid TenantId { get; set; }

    public int? TypeId { get; set; }

    public string? Value { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;

    public virtual ContactInfoType? Type { get; set; }
}
