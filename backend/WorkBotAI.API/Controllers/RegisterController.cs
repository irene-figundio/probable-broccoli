using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkBotAI.API.DTOs;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using System.Linq;
using WorkBotAI.API.Services;

namespace WorkBotAI.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class RegisterController : ControllerBase
{
    private readonly IRegisterRepository _registerRepository;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public RegisterController(IRegisterRepository registerRepository, IConfiguration configuration, IPasswordHasher passwordHasher, IAuditService auditService)
    {
        _registerRepository = registerRepository;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
    }

    // POST: api/Register
    [HttpPost]
    public async Task<ActionResult> RegisterTenant([FromBody] RegisterTenantDto dto)
    {
        try
        {
            if (string.IsNullOrEmpty(dto.BusinessName)) return BadRequest(new RegisterResponseDto { Success = false, Error = "Nome attività obbligatorio" });
            if (string.IsNullOrEmpty(dto.OwnerEmail)) return BadRequest(new RegisterResponseDto { Success = false, Error = "Email obbligatoria" });

            var (isPasswordValid, passwordError) = PasswordValidator.Validate(dto.OwnerPassword);
            if (!isPasswordValid)
            {
                return BadRequest(new RegisterResponseDto { Success = false, Error = passwordError });
            }

            var existingUser = await _registerRepository.GetUserByEmailAsync(dto.OwnerEmail);
            if (existingUser != null)
            {
                await _auditService.LogActionAsync("Register", "RegisterTenant", $"Attempted registration with already registered email: {dto.OwnerEmail}");
                return Conflict(new RegisterResponseDto { Success = false, Error = "Email già registrata" });
            }

            // Hash password
            dto.OwnerPassword = _passwordHasher.HashPassword(dto.OwnerPassword);

            var result = await _registerRepository.RegisterTenantAsync(dto);
            if (!result.Success)
            {
                await _auditService.LogErrorAsync("Register", $"Registration failed for {dto.OwnerEmail}: {result.Error}");
                return StatusCode(500, result);
            }

            var user = await _registerRepository.GetUserByEmailAsync(dto.OwnerEmail);
            var token = GenerateJwtToken(user, new Tenant { Id = (Guid)result.TenantId, Name = dto.BusinessName });
            result.Token = token;

            await _auditService.LogActionAsync("Register", "RegisterTenant", $"Successfully registered tenant {dto.BusinessName} and user {dto.OwnerEmail}", null, result.TenantId, user.Id);

            return Ok(result);
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Register", "Unexpected error during registration", ex);
            return StatusCode(500, new RegisterResponseDto { Success = false, Error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Register/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _registerRepository.GetCategoriesAsync();
        return Ok(new { success = true, data = categories.Select(c => new { c.Id, c.Name }) });
    }

    private string GenerateJwtToken(User user, Tenant tenant)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "aee30e2c-73b5-4b87-9d9d-e28c7a326626A!wkr8vHmpUrJ784";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Mail ?? string.Empty),
            new Claim(ClaimTypes.Role, "Owner"),
            new Claim("TenantId", tenant.Id.ToString()),
            new Claim("TenantName", tenant.Name ?? ""),
            new Claim("IsSuperAdmin", "false")
        };

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