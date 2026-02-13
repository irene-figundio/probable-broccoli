using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WorkbotAI.Models;
using WorkBotAI.API.DTOs;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public AuthService(IUserRepository userRepository, IConfiguration configuration, IPasswordHasher passwordHasher, IAuditService auditService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            // Cerca l'utente nel database
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

            if (user == null)
            {
                await _auditService.LogActionAsync("Auth", "Login", $"Attempted login for non-existent user: {loginDto.Email}");
                return new LoginResponseDto
                {
                    Success = false,
                    Error = "Utente non trovato"
                };
            }

            // Verifica password
            if (!_passwordHasher.VerifyPassword(loginDto.Password, user.Password))
            {
                await _auditService.LogActionAsync("Auth", "Login", $"Failed login attempt for user: {loginDto.Email}", null, user.TenantId, user.Id);
                return new LoginResponseDto
                {
                    Success = false,
                    Error = "Password non valida"
                };
            }

            // Verifica se l'utente è attivo
            if (user.IsActive != true)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Error = "Utente non attivo"
                };
            }

            // Genera il token JWT
            var token = GenerateJwtToken(user);

            // Determina se è super admin
            var isSuperAdmin = user.IsSuperAdmin == true || user.RoleId == 1;

            await _auditService.LogActionAsync("Auth", "Login", $"Successful login for user: {loginDto.Email}", null, user.TenantId, user.Id);

            return new LoginResponseDto
            {
                Success = true,
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Mail ?? string.Empty,
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    Role = user.Role?.Name ?? "User",
                    IsSuperAdmin = isSuperAdmin,
                    TenantId = user.TenantId?.ToString(),
                    TenantName = user.Tenant?.Name ?? ""
                }
            };
        }
        catch (Exception ex)
        {
            return new LoginResponseDto
            {
                Success = false,
                Error = $"Errore durante il login: {ex.Message}"
            };
        }
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "aee30e2c-73b5-4b87-9d9d-e28c7a326626A!wkr8vHmpUrJ784";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var isSuperAdmin = user.IsSuperAdmin == true || user.RoleId == 1;

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Mail ?? string.Empty),
            new Claim("TenantId", user.TenantId?.ToString() ?? "0"),
            new Claim("IsSuperAdmin", isSuperAdmin.ToString())
        };

        // Aggiungi il ruolo specifico dell'utente
        var userRole = user.Role?.Name ?? "User";
        claims.Add(new Claim(ClaimTypes.Role, userRole));

        // Se è SuperAdmin e il ruolo non è già SuperAdmin, aggiungilo come ruolo aggiuntivo
        if (isSuperAdmin && userRole != "SuperAdmin")
        {
            claims.Add(new Claim(ClaimTypes.Role, "SuperAdmin"));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "WorkBotAI",
            audience: _configuration["Jwt:Audience"] ?? "WorkBotAI",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}