using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class RegisterController : ControllerBase
{
    private readonly WorkBotAIContext _context;
    private readonly IConfiguration _configuration;

    public RegisterController(WorkBotAIContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    // POST: api/Register
    [HttpPost]
    public async Task<ActionResult> RegisterTenant([FromBody] RegisterTenantDto dto)
    {
        // Validazione
        if (string.IsNullOrEmpty(dto.BusinessName))
            return BadRequest(new RegisterResponseDto { Success = false, Error = "Nome attività obbligatorio" });
        
        if (string.IsNullOrEmpty(dto.OwnerEmail))
            return BadRequest(new RegisterResponseDto { Success = false, Error = "Email obbligatoria" });
        
        if (string.IsNullOrEmpty(dto.OwnerPassword) || dto.OwnerPassword.Length < 6)
            return BadRequest(new RegisterResponseDto { Success = false, Error = "Password deve essere almeno 6 caratteri" });

        // Verifica email già esistente
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Mail == dto.OwnerEmail && u.IsDeleted != true);
        
        if (existingUser != null)
            return BadRequest(new RegisterResponseDto { Success = false, Error = "Email già registrata" });

        using var transaction = await _context.Database.BeginTransactionAsync();
        
        try
        {
            // 1. Crea il Tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = dto.BusinessName,
                CategoryId = dto.CategoryId,
                IsActive = true,
                CreationDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Tenants.Add(tenant);
            await _context.SaveChangesAsync();

            // 2. Crea la Subscription (piano base gratuito per 30 giorni)
            var subscription = new Subscription
            {
                TenantId = tenant.Id,
                PlaneId = 1, // Piano Base
                StatusId = 1, // Attivo
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30))
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            // 3. Crea l'Utente Owner
            var user = new User
            {
                TenantId = tenant.Id,
                FirstName = dto.OwnerFirstName,
                LastName = dto.OwnerLastName,
                Mail = dto.OwnerEmail,
                UserName = dto.OwnerEmail, // UserName = Email
                Password = dto.OwnerPassword, // TODO: Hash password in produzione
                StatusId = 1, // Attivo
                RoleId = 2, // Owner
                IsActive = true,
                IsSuperAdmin = false,
                CreationTime = DateTime.UtcNow,
                IsDeleted = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            // 4. Genera JWT Token
            var token = GenerateJwtToken(user, tenant);

            return Ok(new RegisterResponseDto
            {
                Success = true,
                Token = token,
                TenantId = tenant.Id,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Mail ?? string.Empty,
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    Role = "Owner",
                    IsSuperAdmin = false,
                    TenantId = tenant.Id.ToString()
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new RegisterResponseDto 
            { 
                Success = false, 
                Error = $"Errore durante la registrazione: {ex.Message}" 
            });
        }
    }

    // GET: api/Register/categories
    [HttpGet("categories")]
    public async Task<ActionResult> GetCategories()
    {
        var categories = await _context.Categories
            .Select(c => new { c.Id, c.Name })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(new { success = true, data = categories });
    }

    private string GenerateJwtToken(User user, Tenant tenant)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "WorkBotAI_SuperSecretKey_2024_MinLength32Characters!";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("email", user.Mail ?? ""),
            new Claim("role", "Owner"),
            new Claim("isSuperAdmin", "false"),
            new Claim("tenantId", tenant.Id.ToString()),
            new Claim("tenantName", tenant.Name ?? "")
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