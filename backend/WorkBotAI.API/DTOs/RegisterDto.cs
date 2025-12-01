namespace WorkBotAI.API.DTOs;

// Registrazione nuovo tenant
public class RegisterTenantDto
{
    // Dati Tenant
    public string BusinessName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    
    // Dati Utente Owner
    public string OwnerFirstName { get; set; } = string.Empty;
    public string OwnerLastName { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public string OwnerPassword { get; set; } = string.Empty;
}

// Risposta registrazione
public class RegisterResponseDto
{
    public bool Success { get; set; }
    public string? Error { get; set; }
    public string? Token { get; set; }
    public UserDto? User { get; set; }
    public Guid? TenantId { get; set; }
}