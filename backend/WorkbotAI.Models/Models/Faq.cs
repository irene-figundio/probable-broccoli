using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class Faq
{
    public int Id { get; set; }

    public int? CategoryId { get; set; }

    public string Question { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<TenantFaq> TenantFaqs { get; set; } = new List<TenantFaq>();
}
