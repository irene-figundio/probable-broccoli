namespace WorkBotAI.API.DTOs;

// Lista impostazioni
public class SettingListDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public int? SettingTypeId { get; set; }
    public string? SettingTypeName { get; set; }
    public string? Value { get; set; }
}

// Dettaglio impostazione
public class SettingDetailDto
{
    public int Id { get; set; }
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public int? SettingTypeId { get; set; }
    public string? SettingTypeName { get; set; }
    public string? Value { get; set; }
}

// Creazione impostazione
public class CreateSettingDto
{
    public Guid TenantId { get; set; }
    public int SettingTypeId { get; set; }
    public string? Value { get; set; }
}

// Aggiornamento impostazione
public class UpdateSettingDto
{
    public string? Value { get; set; }
}

// Aggiornamento batch impostazioni
public class UpdateSettingsDto
{
    public List<SettingItemDto> Settings { get; set; } = new();
}

public class SettingItemDto
{
    public int SettingTypeId { get; set; }
    public string? Value { get; set; }
}

// Tipo impostazione
public class SettingTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Impostazioni tenant complete
public class TenantSettingsDto
{
    public Guid TenantId { get; set; }
    public string? TenantName { get; set; }
    public string? BusinessName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? OpeningHours { get; set; }
    public string? ClosingHours { get; set; }
    public string? WorkingDays { get; set; }
    public string? BookingAdvanceDays { get; set; }
    public string? CancellationPolicy { get; set; }
    public string? ReminderHours { get; set; }
    public string? WelcomeMessage { get; set; }
    public bool? SmsNotifications { get; set; }
    public bool? EmailNotifications { get; set; }
    public bool? WhatsappNotifications { get; set; }
}