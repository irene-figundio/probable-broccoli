using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class PaymentType
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public string? BaseGateway { get; set; }

    public virtual ICollection<AppointmentPayment> AppointmentPayments { get; set; } = new List<AppointmentPayment>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
