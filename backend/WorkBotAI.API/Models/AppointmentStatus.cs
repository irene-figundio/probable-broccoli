using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class AppointmentStatus
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
