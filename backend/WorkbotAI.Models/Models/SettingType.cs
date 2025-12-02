using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class SettingType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Setting> Settings { get; set; } = new List<Setting>();
}
