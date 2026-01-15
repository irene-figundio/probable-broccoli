using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Acronym { get; set; }

    public string? LogoImage { get; set; }

    public DateTime? CreationDate { get; set; }

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletionTime { get; set; }

    public int? DeletionUserId { get; set; }

    public int? CategoryId { get; set; }

    public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public virtual ICollection<Availability> Availabilities { get; set; } = new List<Availability>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<ContactInfo> ContactInfos { get; set; } = new List<ContactInfo>();

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Resource> Resources { get; set; } = new List<Resource>();

    public virtual ICollection<Service> Services { get; set; } = new List<Service>();

    public virtual ICollection<Setting> Settings { get; set; } = new List<Setting>();

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();

    public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

    public virtual ICollection<TenantFaq> TenantFaqs { get; set; } = new List<TenantFaq>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
