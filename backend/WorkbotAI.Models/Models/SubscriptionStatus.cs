using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class SubscriptionStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
