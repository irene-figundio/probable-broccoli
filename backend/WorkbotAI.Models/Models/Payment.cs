using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class Payment
{
    public int Id { get; set; }

    public int? SubscriptionId { get; set; }

    public DateTime? DatePayment { get; set; }

    public int? PaymentTypeId { get; set; }

    public decimal? ImportValue { get; set; }

    public decimal? IvaValue { get; set; }

    public int? StatusPaymentId { get; set; }

    public string? Notes { get; set; }

    public virtual PaymentType? PaymentType { get; set; }

    public virtual PaymentStatus? StatusPayment { get; set; }

    public virtual Subscription? Subscription { get; set; }
}
