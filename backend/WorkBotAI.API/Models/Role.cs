using System;
using System.Collections.Generic;

namespace WorkBotAI.API.Models;

public partial class Role
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public bool? IsActive { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletionDate { get; set; }

    public int? DeletionUserId { get; set; }

    public DateTime? CreationTime { get; set; }

    public int? CreationUserId { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public int? LastModificationUserId { get; set; }

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
