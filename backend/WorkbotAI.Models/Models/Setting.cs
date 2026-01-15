using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class Setting
{
    public int Id { get; set; }

    public Guid TenantId { get; set; }

    public int? SettingTypeId { get; set; }

    public string? Value { get; set; }

    public virtual SettingType? SettingType { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
