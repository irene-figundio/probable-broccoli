using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class ContactInfoType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<ContactInfo> ContactInfos { get; set; } = new List<ContactInfo>();
}
