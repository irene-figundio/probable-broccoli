using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class PaymentStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<AppointmentPayment> AppointmentPayments { get; set; } = new List<AppointmentPayment>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
