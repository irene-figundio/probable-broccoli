using System;
using System.Collections.Generic;

namespace WorkbotAI.Models;

public partial class User
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Mail { get; set; } = null!;

    public int? StatusId { get; set; }

    public DateTime? StatusTime { get; set; }

    public DateTime? LastLoginTime { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreationTime { get; set; }

    public int? CreationUserId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public int? LastModificationUserId { get; set; }

    public string? AvatarImage { get; set; }

    public int? RoleId { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletionTime { get; set; }

    public int? DeletionUserId { get; set; }

    public Guid? TenantId { get; set; }

    public bool? IsSuperAdmin { get; set; }

    public string? VerificationToken { get; set; }

    public string? ResetPasswordCode { get; set; }

    public string? FirstLoginOtp { get; set; }

    public DateTime? FirstLoginExpire { get; set; }

    public string? FirstLoginToken { get; set; }

    public DateTime? ResetPasswordExpiration { get; set; }

    public virtual Role? Role { get; set; }

    public virtual UserStatus? Status { get; set; }

    public virtual Tenant? Tenant { get; set; }
}
