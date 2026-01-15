
// DTO per la lista utenti
public class UserListDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? AvatarImage { get; set; }
    public bool IsActive { get; set; }
    public bool IsSuperAdmin { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public DateTime? CreationTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
}

// DTO per dettaglio singolo utente
public class UserDetailDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarImage { get; set; }
    public bool IsActive { get; set; }
    public bool IsSuperAdmin { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public Guid? TenantId { get; set; }
    public string? TenantName { get; set; }
    public DateTime? CreationTime { get; set; }
    public DateTime? LastLoginTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}

// DTO per creare un nuovo utente
public class CreateUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? RoleId { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSuperAdmin { get; set; } = false;
}

// DTO per aggiornare un utente
public class UpdateUserDto
{
    public string UserName { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? RoleId { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; }
    public bool IsSuperAdmin { get; set; }
}

// DTO per cambio password
public class ChangePasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}