using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class AppointmentPayment
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public DateTime? DatePayment { get; set; }

    public int? PaymentTypeId { get; set; }

    public decimal? ImportValue { get; set; }

    public decimal? IvaValue { get; set; }

    public int? StatusPaymentId { get; set; }

    public string? Note { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual PaymentType? PaymentType { get; set; }

    public virtual PaymentStatus? StatusPayment { get; set; }
}
