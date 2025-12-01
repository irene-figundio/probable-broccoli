using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class Subscription
{
    public int Id { get; set; }

    public Guid TenantId { get; set; }

    public int? StatusId { get; set; }

    public int? PlaneId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual Plane? Plane { get; set; }

    public virtual SubscriptionStatus? Status { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
