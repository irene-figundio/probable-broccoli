using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class AppointmentService
{
    public int Id { get; set; }

    public int AppointmentId { get; set; }

    public int ServiceId { get; set; }

    public string? Note { get; set; }

    public virtual Appointment Appointment { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
