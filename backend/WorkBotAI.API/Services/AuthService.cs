using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using WorkBotAI.API.DTOs;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AuthService(IUserRepository userRepository, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
    {
        try
        {
            // Cerca l'utente nel database
            var user = await _userRepository.GetUserByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Error = "Utente non trovato"
                };
            }

            // Verifica password (confronto semplice per ora)
            if (user.Password != loginDto.Password)
            {
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

    private string GenerateJwtToken(Models.User user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "WorkBotAI_SuperSecretKey_2024_MinLength32Characters!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var isSuperAdmin = user.IsSuperAdmin == true || user.RoleId == 1;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Mail ?? string.Empty),
            new Claim(ClaimTypes.Role, user.Role?.Name ?? "User"),
            new Claim("TenantId", user.TenantId?.ToString() ?? "0"),
            new Claim("IsSuperAdmin", isSuperAdmin.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "WorkBotAI",
            audience: _configuration["Jwt:Audience"] ?? "WorkBotAI",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}