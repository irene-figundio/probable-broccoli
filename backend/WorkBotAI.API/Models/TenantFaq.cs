using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class TenantFaq
{
    public int Id { get; set; }

    public Guid TenantId { get; set; }

    public int FaqId { get; set; }

    public string? Value { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreationTime { get; set; }

    public int? CreationUserId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public int? LastModificationUserId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletionTime { get; set; }

    public int? DeletionUserId { get; set; }

    public virtual Faq Faq { get; set; } = null!;

    public virtual Tenant Tenant { get; set; } = null!;
}
