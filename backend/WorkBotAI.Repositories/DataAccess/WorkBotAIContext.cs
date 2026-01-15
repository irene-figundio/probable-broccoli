using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;

namespace WorkBotAI.Repositories.DataAccess;

public partial class WorkBotAIContext : DbContext
{
    public WorkBotAIContext()
    {
    }

    public WorkBotAIContext(DbContextOptions<WorkBotAIContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentPayment> AppointmentPayments { get; set; }

    public virtual DbSet<AppointmentService> AppointmentServices { get; set; }

    public virtual DbSet<AppointmentStatus> AppointmentStatuses { get; set; }

    public virtual DbSet<Availability> Availabilities { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ContactInfo> ContactInfos { get; set; }

    public virtual DbSet<ContactInfoType> ContactInfoTypes { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<JobType> JobTypes { get; set; }

    public virtual DbSet<Module> Modules { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<PaymentStatus> PaymentStatuses { get; set; }

    public virtual DbSet<PaymentType> PaymentTypes { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Plane> Planes { get; set; }

    public virtual DbSet<Resource> Resources { get; set; }

    public virtual DbSet<ResourceType> ResourceTypes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<SettingType> SettingTypes { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Submodule> Submodules { get; set; }

    public virtual DbSet<Subscription> Subscriptions { get; set; }

    public virtual DbSet<SubscriptionStatus> SubscriptionStatuses { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<TenantFaq> TenantFaqs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserStatus> UserStatuses { get; set; }

    public virtual DbSet<SystemLog> SystemLogs { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=Hellboy\\SQLEXPRESS;Database=WorkBotAI;User Id=sa;Password=sa1;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC27EDF0AC45");

            entity.HasIndex(e => e.CustomerId, "IX_Appointments_Customer");

            entity.HasIndex(e => e.TenantId, "IX_Appointments_Tenant");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.DeletionUserId).HasColumnName("DeletionUserID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LastModificationUserId).HasColumnName("LastModificationUserID");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.ResourceId).HasColumnName("ResourceID");
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.TenantId).HasColumnName("TenantID");

            entity.HasOne(d => d.Customer).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_Appointments_Customers");

            entity.HasOne(d => d.Resource).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.ResourceId)
                .HasConstraintName("FK_Appointments_Resources");

            entity.HasOne(d => d.Staff).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_Appointments_Staff");

            entity.HasOne(d => d.Status).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Appointments_Status");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Appointments_Tenants");
        });

        modelBuilder.Entity<AppointmentPayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC272AD08F43");

            entity.HasIndex(e => e.AppointmentId, "IX_AppointmentPayments_Appointment");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.ImportValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IvaValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PaymentTypeId).HasColumnName("PaymentTypeID");
            entity.Property(e => e.StatusPaymentId).HasColumnName("StatusPaymentID");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentPayments)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentPayments_Appointments");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.AppointmentPayments)
                .HasForeignKey(d => d.PaymentTypeId)
                .HasConstraintName("FK_AppointmentPayments_PaymentTypes");

            entity.HasOne(d => d.StatusPayment).WithMany(p => p.AppointmentPayments)
                .HasForeignKey(d => d.StatusPaymentId)
                .HasConstraintName("FK_AppointmentPayments_Status");
        });

        modelBuilder.Entity<AppointmentService>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC27D5E34162");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AppointmentId).HasColumnName("AppointmentID");
            entity.Property(e => e.Note).HasMaxLength(300);
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

            entity.HasOne(d => d.Appointment).WithMany(p => p.AppointmentServices)
                .HasForeignKey(d => d.AppointmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentServices_Appointments");

            entity.HasOne(d => d.Service).WithMany(p => p.AppointmentServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AppointmentServices_Services");
        });

        modelBuilder.Entity<AppointmentStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Appointm__3214EC275B07E2FA");

            entity.ToTable("AppointmentStatus");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Availabi__3214EC2765A16212");

            entity.HasIndex(e => e.StaffId, "IX_Availabilities_Staff");

            entity.HasIndex(e => e.TenantId, "IX_Availabilities_Tenant");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.StaffId).HasColumnName("StaffID");
            entity.Property(e => e.TenantId).HasColumnName("TenantID");

            entity.HasOne(d => d.Staff).WithMany(p => p.Availabilities)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK_Availabilities_Staff");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Availabilities)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Availabilities_Tenants");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Categori__3214EC27850F52DE");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<ContactInfo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ContactI__3214EC2768EEF820");

            entity.ToTable("ContactInfo");

            entity.HasIndex(e => e.TenantId, "IX_ContactInfo_Tenant");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.TenantId).HasColumnName("TenantID");
            entity.Property(e => e.TypeId).HasColumnName("TypeID");
            entity.Property(e => e.Value).HasMaxLength(255);

            entity.HasOne(d => d.Tenant).WithMany(p => p.ContactInfos)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ContactInfo_Tenants");

            entity.HasOne(d => d.Type).WithMany(p => p.ContactInfos)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK_ContactInfo_Types");
        });

        modelBuilder.Entity<ContactInfoType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ContactI__3214EC27ACCA9EFA");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC274457752A");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.DeletionUserId).HasColumnName("DeletionUserID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LastModificationUserId).HasColumnName("LastModificationUserID");
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.TenantId).HasColumnName("TenantID");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Customers)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Customers_Tenants");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Faqs__3214EC270B999701");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Question).HasMaxLength(500);

            entity.HasOne(d => d.Category).WithMany(p => p.Faqs)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Faqs_Categories");
        });

        modelBuilder.Entity<JobType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__JobTypes__3214EC271BB384AF");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.Category).WithMany(p => p.JobTypes)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_JobTypes_Categories");
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Modules__3214EC2790DDF088");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PlaneId).HasColumnName("PlaneID");

            entity.HasOne(d => d.Plane).WithMany(p => p.Modules)
                .HasForeignKey(d => d.PlaneId)
                .HasConstraintName("FK_Modules_Planes");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Payments__3214EC2734EF2FB5");

            entity.HasIndex(e => e.SubscriptionId, "IX_Payments_Subscription");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DatePayment)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImportValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IvaValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Notes).HasMaxLength(255);
            entity.Property(e => e.PaymentTypeId).HasColumnName("PaymentTypeID");
            entity.Property(e => e.StatusPaymentId).HasColumnName("StatusPaymentID");
            entity.Property(e => e.SubscriptionId).HasColumnName("SubscriptionID");

            entity.HasOne(d => d.PaymentType).WithMany(p => p.Payments)
                .HasForeignKey(d => d.PaymentTypeId)
                .HasConstraintName("FK_Payments_PaymentTypes");

            entity.HasOne(d => d.StatusPayment).WithMany(p => p.Payments)
                .HasForeignKey(d => d.StatusPaymentId)
                .HasConstraintName("FK_Payments_Status");

            entity.HasOne(d => d.Subscription).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SubscriptionId)
                .HasConstraintName("FK_Payments_Subscriptions");
        });

        modelBuilder.Entity<PaymentStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentS__3214EC2705855E8E");

            entity.ToTable("PaymentStatus");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<PaymentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PaymentT__3214EC270FD5DF8F");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BaseGateway).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Permissi__3214EC27BBA1D3FD");

            entity.HasIndex(e => e.SubmoduleId, "IX_Permissions_Submodule");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsMinimum).HasDefaultValue(false);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PlaneId).HasColumnName("PlaneID");
            entity.Property(e => e.SubmoduleId).HasColumnName("SubmoduleID");

            entity.HasOne(d => d.Plane).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.PlaneId)
                .HasConstraintName("FK_Permissions_Planes");

            entity.HasOne(d => d.Submodule).WithMany(p => p.Permissions)
                .HasForeignKey(d => d.SubmoduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Permissions_Submodules");
        });

        modelBuilder.Entity<Plane>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Planes__3214EC271483F805");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Resource>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Resource__3214EC2706900EE6");

            entity.HasIndex(e => e.TenantId, "IX_Resources_Tenant");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.DeletionUserId).HasColumnName("DeletionUserID");
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LastModificationUserId).HasColumnName("LastModificationUserID");
            entity.Property(e => e.NumeroPosti).HasDefaultValue(1);
            entity.Property(e => e.TenantId).HasColumnName("TenantID");
            entity.Property(e => e.TypeId).HasColumnName("TypeID");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Resources)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resources_Tenants");

            entity.HasOne(d => d.Type).WithMany(p => p.Resources)
                .HasForeignKey(d => d.TypeId)
                .HasConstraintName("FK_Resources_Types");
        });

        modelBuilder.Entity<ResourceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Resource__3214EC276F3561E4");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.Category).WithMany(p => p.ResourceTypes)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_ResourceTypes_Categories");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC27D5A95318");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.DeletionUserId).HasColumnName("DeletionUserID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LastModificationUserId).HasColumnName("LastModificationUserID");
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__RolePerm__3214EC274A1104CB");

            entity.HasIndex(e => e.PermissionId, "IX_RolePermissions_Permission");

            entity.HasIndex(e => e.RoleId, "IX_RolePermissions_Role");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.PermissionId).HasColumnName("PermissionID");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Permission).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.PermissionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolePermissions_Permissions");

            entity.HasOne(d => d.Role).WithMany(p => p.RolePermissions)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RolePermissions_Roles");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Services__3214EC278F3A8A15");

            entity.HasIndex(e => e.TenantId, "IX_Services_Tenant");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BafferTime).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.BasePrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.DeletionUserId).HasColumnName("DeletionUserID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LastModificationUserId).HasColumnName("LastModificationUserID");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.TenantId).HasColumnName("TenantID");

            entity.HasOne(d => d.Category).WithMany(p => p.Services)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Services_Categories");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Services)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Services_Tenants");
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Settings__3214EC2723EF934D");

            entity.HasIndex(e => e.TenantId, "IX_Settings_Tenant");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.SettingTypeId).HasColumnName("SettingTypeID");
            entity.Property(e => e.TenantId).HasColumnName("TenantID");

            entity.HasOne(d => d.SettingType).WithMany(p => p.Settings)
                .HasForeignKey(d => d.SettingTypeId)
                .HasConstraintName("FK_Settings_SettingTypes");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Settings)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Settings_Tenants");
        });

        modelBuilder.Entity<SettingType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SettingT__3214EC27E7E75681");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Staff__3214EC2748A1F73D");

            entity.HasIndex(e => e.TenantId, "IX_Staff_Tenant");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.DeletionUserId).HasColumnName("DeletionUserID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.JobTypeId).HasColumnName("JobTypeID");
            entity.Property(e => e.LastModificationUserId).HasColumnName("LastModificationUserID");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.TenantId).HasColumnName("TenantID");

            entity.HasOne(d => d.JobType).WithMany(p => p.Staff)
                .HasForeignKey(d => d.JobTypeId)
                .HasConstraintName("FK_Staff_JobTypes");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Staff)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Staff_Tenants");
        });

        modelBuilder.Entity<Submodule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Submodul__3214EC272AEBE2EB");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModuleId).HasColumnName("ModuleID");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PlaneId).HasColumnName("PlaneID");

            entity.HasOne(d => d.Module).WithMany(p => p.Submodules)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Submodules_Modules");

            entity.HasOne(d => d.Plane).WithMany(p => p.Submodules)
                .HasForeignKey(d => d.PlaneId)
                .HasConstraintName("FK_Submodules_Planes");
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC27DC031D7D");

            entity.HasIndex(e => e.PlaneId, "IX_Subscriptions_Plane");

            entity.HasIndex(e => e.TenantId, "IX_Subscriptions_Tenant");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.PlaneId).HasColumnName("PlaneID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.TenantId).HasColumnName("TenantID");

            entity.HasOne(d => d.Plane).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.PlaneId)
                .HasConstraintName("FK_Subscriptions_Planes");

            entity.HasOne(d => d.Status).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Subscriptions_Status");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subscriptions_Tenants");
        });

        modelBuilder.Entity<SubscriptionStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Subscrip__3214EC27F412DC25");

            entity.ToTable("SubscriptionStatus");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("SystemLogs");

            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.Level);
            entity.HasIndex(e => e.TenantId);

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Timestamp).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Level).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Source).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Message).IsRequired();
            entity.Property(e => e.Context).HasColumnType("nvarchar(max)");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.TenantId).HasColumnName("TenantID");
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.ToTable("SystemSettings");

            entity.HasIndex(e => new { e.Category, e.Key }).IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Category).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasColumnType("nvarchar(max)");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tenants__3214EC27FF446EA9");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("ID");
            entity.Property(e => e.Acronym).HasMaxLength(50);
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CreationDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DeletionUserId).HasColumnName("DeletionUserID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LogoImage).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Category).WithMany(p => p.Tenants)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Tenants_Categories");
        });

        modelBuilder.Entity<TenantFaq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TenantFa__3214EC2714D181B8");

            entity.HasIndex(e => e.FaqId, "IX_TenantFaqs_Faq");

            entity.HasIndex(e => e.TenantId, "IX_TenantFaqs_Tenant");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.DeletionUserId).HasColumnName("DeletionUserID");
            entity.Property(e => e.FaqId).HasColumnName("FaqID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.LastModificationUserId).HasColumnName("LastModificationUserID");
            entity.Property(e => e.TenantId).HasColumnName("TenantID");

            entity.HasOne(d => d.Faq).WithMany(p => p.TenantFaqs)
                .HasForeignKey(d => d.FaqId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TenantFaqs_Faqs");

            entity.HasOne(d => d.Tenant).WithMany(p => p.TenantFaqs)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TenantFaqs_Tenants");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC27ED99C781");

            entity.HasIndex(e => e.LastLoginTime, "IX_Users_LastLoginTime");

            entity.HasIndex(e => e.Mail, "IX_Users_Mail");

            entity.HasIndex(e => e.StatusId, "IX_Users_Status");

            entity.HasIndex(e => e.TenantId, "IX_Users_Tenant");

            entity.HasIndex(e => e.UserName, "IX_Users_UserName");

            entity.HasIndex(e => e.Mail, "UQ__Users__2724B2D1C0C21909").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F2845650DF537B").IsUnique();

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CreationTime).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreationUserId).HasColumnName("CreationUserID");
            entity.Property(e => e.DeletionUserId).HasColumnName("DeletionUserID");
            entity.Property(e => e.FirstLoginOtp).HasMaxLength(64);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasDefaultValue(false);
            entity.Property(e => e.IsSuperAdmin).HasDefaultValue(false);
            entity.Property(e => e.LastModificationUserId).HasColumnName("LastModificationUserID");
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Mail).HasMaxLength(256);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.ResetPasswordCode).HasMaxLength(2500);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.StatusId).HasColumnName("StatusID");
            entity.Property(e => e.TenantId).HasColumnName("TenantID");
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_Users_Roles");

            entity.HasOne(d => d.Status).WithMany(p => p.Users)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_Users_Status");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Users)
                .HasForeignKey(d => d.TenantId)
                .HasConstraintName("FK_Users_Tenant");
        });

        modelBuilder.Entity<UserStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserStat__3214EC27868C2791");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}