using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess;
using WorkBotAI.API.Services;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public UsersController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsers([FromQuery] Guid? tenantId = null)
    {
        var query = _context.Users
            .Where(u => u.IsDeleted != true)
            .Include(u => u.Role)
            .Include(u => u.Tenant)
            .AsQueryable();

        // Filtro per tenant (opzionale)
        if (tenantId.HasValue)
        {
            query = query.Where(u => u.TenantId == tenantId.Value);
        }

        var users = await query
            .OrderByDescending(u => u.CreationTime)
            .Select(u => new UserListDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Mail = u.Mail,
                FirstName = u.FirstName,
                LastName = u.LastName,
                AvatarImage = u.AvatarImage,
                IsActive = u.IsActive ?? false,
                IsSuperAdmin = u.IsSuperAdmin ?? false,
                RoleId = u.RoleId,
                RoleName = u.Role != null ? u.Role.Name : null,
                TenantId = u.TenantId,
                TenantName = u.Tenant != null ? u.Tenant.Name : null,
                CreationTime = u.CreationTime,
                LastLoginTime = u.LastLoginTime
            })
            .ToListAsync();

        return Ok(new { success = true, data = users });
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id && u.IsDeleted != true)
            .Include(u => u.Role)
            .Include(u => u.Status)
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { success = false, error = "Utente non trovato" });
        }

        var result = new UserDetailDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Mail = user.Mail,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AvatarImage = user.AvatarImage,
            IsActive = user.IsActive ?? false,
            IsSuperAdmin = user.IsSuperAdmin ?? false,
            RoleId = user.RoleId,
            RoleName = user.Role?.Name,
            StatusId = user.StatusId,
            StatusName = user.Status?.Name,
            TenantId = user.TenantId,
            TenantName = user.Tenant?.Name,
            CreationTime = user.CreationTime,
            LastLoginTime = user.LastLoginTime,
            LastModificationTime = user.LastModificationTime
        };

        return Ok(new { success = true, data = result });
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult> CreateUser(CreateUserDto dto)
    {
        // Verifica username univoco
        var existingUser = await _context.Users
            .Where(u => u.UserName == dto.UserName && u.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            return BadRequest(new { success = false, error = "Username già esistente" });
        }

        // Verifica email univoca
        var existingEmail = await _context.Users
            .Where(u => u.Mail == dto.Mail && u.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (existingEmail != null)
        {
            return BadRequest(new { success = false, error = "Email già esistente" });
        }

        var (isPasswordValid, passwordError) = PasswordValidator.Validate(dto.Password);
        if (!isPasswordValid)
        {
            return BadRequest(new { success = false, error = passwordError });
        }

        var user = new User
        {
            UserName = dto.UserName,
            Password = dto.Password, // In produzione: hash della password
            Mail = dto.Mail,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            RoleId = dto.RoleId,
            TenantId = dto.TenantId,
            IsActive = dto.IsActive,
            IsSuperAdmin = dto.IsSuperAdmin,
            IsDeleted = false,
            CreationTime = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
        {
            success = true,
            data = new UserListDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Mail = user.Mail,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive ?? false,
                IsSuperAdmin = user.IsSuperAdmin ?? false,
                CreationTime = user.CreationTime
            }
        });
    }

    // PUT: api/Users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        var user = await _context.Users
            .Where(u => u.Id == id && u.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { success = false, error = "Utente non trovato" });
        }

        // Verifica username univoco (escludendo l'utente corrente)
        var existingUser = await _context.Users
            .Where(u => u.UserName == dto.UserName && u.Id != id && u.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (existingUser != null)
        {
            return BadRequest(new { success = false, error = "Username già esistente" });
        }

        // Verifica email univoca (escludendo l'utente corrente)
        var existingEmail = await _context.Users
            .Where(u => u.Mail == dto.Mail && u.Id != id && u.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (existingEmail != null)
        {
            return BadRequest(new { success = false, error = "Email già esistente" });
        }

        user.UserName = dto.UserName;
        user.Mail = dto.Mail;
        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.RoleId = dto.RoleId;
        user.TenantId = dto.TenantId;
        user.IsActive = dto.IsActive;
        user.IsSuperAdmin = dto.IsSuperAdmin;
        user.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Utente aggiornato con successo" });
    }

    // DELETE: api/Users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id && u.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { success = false, error = "Utente non trovato" });
        }

        // Impedisci eliminazione SuperAdmin
        if (user.IsSuperAdmin == true)
        {
            return BadRequest(new { success = false, error = "Non è possibile eliminare un SuperAdmin" });
        }

        // Soft delete
        user.IsDeleted = true;
        user.DeletionTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Utente eliminato con successo" });
    }

    // PUT: api/Users/{id}/toggle-status
    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var user = await _context.Users
            .Where(u => u.Id == id && u.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { success = false, error = "Utente non trovato" });
        }

        user.IsActive = !(user.IsActive ?? false);
        user.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { 
            success = true, 
            message = user.IsActive == true ? "Utente attivato" : "Utente disattivato",
            isActive = user.IsActive
        });
    }

    // PUT: api/Users/{id}/change-password
    [HttpPut("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto dto)
    {
        var user = await _context.Users
            .Where(u => u.Id == id && u.IsDeleted != true)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { success = false, error = "Utente non trovato" });
        }

        var (isPasswordValid, passwordError) = PasswordValidator.Validate(dto.NewPassword);
        if (!isPasswordValid)
        {
            return BadRequest(new { success = false, error = passwordError });
        }

        // In produzione: hash della password
        user.Password = dto.NewPassword;
        user.LastModificationTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Password aggiornata con successo" });
    }

    // GET: api/Users/roles
    [HttpGet("roles")]
    public async Task<ActionResult> GetRoles()
    {
        var roles = await _context.Roles
            .Where(r => r.IsActive == true)
            .Select(r => new { r.Id, r.Name })
            .ToListAsync();

        return Ok(new { success = true, data = roles });
    }
}