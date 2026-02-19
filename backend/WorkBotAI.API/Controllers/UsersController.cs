using WorkBotAI.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using WorkbotAI.Models;
using WorkBotAI.API.Services;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _auditService;

    public UsersController(IUserRepository userRepository, IPasswordHasher passwordHasher, IAuditService auditService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _auditService = auditService;
    }

    // GET: api/Users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserListDto>>> GetUsers([FromQuery] Guid? tenantId = null)
    {
        try
        {
            var users = await _userRepository.GetUsersAsync(tenantId);
            if (users == null || !users.Any())
            {
                await _auditService.LogErrorAsync("Users - GetUsers", $"No users found for tenant: {tenantId}", null, tenantId);
                return NotFound(new { success = false, error = "Nessun utente trovato" });
            }
            var userDtos = users.Select(u => new UserListDto
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
                RoleName = u.Role?.Name,
                TenantId = u.TenantId,
                TenantName = u.Tenant?.Name,
                CreationTime = u.CreationTime,
                LastLoginTime = u.LastLoginTime
            });

            return Ok(new { success = true, data = userDtos });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Users", "Error retrieving users", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetailDto>> GetUser(int id)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                await _auditService.LogErrorAsync("Users - GetUser(Id)", $"User not found: {id}", null, null, null);
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
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Users", $"Error retrieving user {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/Users
    [HttpPost]
    public async Task<ActionResult> CreateUser(CreateUserDto dto)
    {
        try
        {
            // Verifica username univoco
            var existingUser = await _userRepository.GetUserByUsernameAsync(dto.UserName);
            if (existingUser != null)
            {
                await _auditService.LogErrorAsync("Users - CreateUser", $"Username already exists: {dto.UserName}", null, null, null);
                return Conflict(new { success = false, error = "Username già esistente" });
            }

            // Verifica email univoca
            var existingEmail = await _userRepository.GetUserByEmailAsync(dto.Mail);
            if (existingEmail != null)
            {
                await _auditService.LogErrorAsync("Users - CreateUser", $"Email already exists: {dto.Mail}", null, null, null);
                return Conflict(new { success = false, error = "Email già esistente" });
            }

            var (isPasswordValid, passwordError) = PasswordValidator.Validate(dto.Password);
            if (!isPasswordValid)
            {
                await _auditService.LogErrorAsync("Users - CreateUser", $"Invalid password for user: {dto.UserName}", null, null, null);
                return BadRequest(new { success = false, error = passwordError });
            }

            var user = new User
            {
                UserName = dto.UserName,
                Password = _passwordHasher.HashPassword(dto.Password),
                Mail = dto.Mail,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                RoleId = dto.RoleId,
                TenantId = dto.TenantId,
                IsActive = dto.IsActive,
                IsSuperAdmin = dto.IsSuperAdmin,
                IsDeleted = false,
                CreationTime = DateTime.UtcNow,
                StatusId = 1 // Active by default
            };

            await _userRepository.CreateUserAsync(user);
            await _auditService.LogActionAsync("Users", "CreateUser", $"Created user {user.UserName}", null, user.TenantId, user.Id);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new
            {
                success = true,
                data = new { id = user.Id, userName = user.UserName }
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Users", "Error creating user", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                await _auditService.LogErrorAsync("Users - UpdateUser", $"User not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Utente non trovato" });
            }

            // Verifica username univoco
            var existingUser = await _userRepository.GetUserByUsernameAsync(dto.UserName);
            if (existingUser != null && existingUser.Id != id)
            {
                await _auditService.LogErrorAsync("Users - UpdateUser", $"Username already exists: {dto.UserName}", null, null, null);
                return Conflict(new { success = false, error = "Username già esistente" });
            }

            // Verifica email univoca
            var existingEmail = await _userRepository.GetUserByEmailAsync(dto.Mail);
            if (existingEmail != null && existingEmail.Id != id)
            {
                await _auditService.LogErrorAsync("Users - UpdateUser", $"Email already exists: {dto.Mail}", null, null, null);
                return Conflict(new { success = false, error = "Email già esistente" });
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

            await _userRepository.UpdateUserAsync(user);
            await _auditService.LogActionAsync("Users", "UpdateUser", $"Updated user {user.UserName}", null, user.TenantId, user.Id);

            return Ok(new { success = true, message = "Utente aggiornato con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Users", $"Error updating user {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/Users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                await _auditService.LogErrorAsync("Users - DeleteUser", $"User not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Utente non trovato" });
            }

            if (user.IsSuperAdmin == true)
            {
                await _auditService.LogErrorAsync("Users - DeleteUser", $"Attempted to delete SuperAdmin: {id}", null, null, null);
                return BadRequest(new { success = false, error = "Non è possibile eliminare un SuperAdmin" });
            }

            await _userRepository.DeleteUserAsync(id);
            await _auditService.LogActionAsync("Users", "DeleteUser", $"Deleted user {user.UserName}", null, user.TenantId, user.Id);

            return Ok(new { success = true, message = "Utente eliminato con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Users", $"Error deleting user {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Users/{id}/toggle-status
    [HttpPut("{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                await _auditService.LogErrorAsync("Users - ToggleUserStatus", $"User not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Utente non trovato" });
            }

            user.IsActive = !(user.IsActive ?? false);
            user.LastModificationTime = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
            await _auditService.LogActionAsync("Users", "ToggleStatus", $"Toggled status for {user.UserName} to {user.IsActive}", null, user.TenantId, user.Id);

            return Ok(new {
                success = true,
                message = user.IsActive == true ? "Utente attivato" : "Utente disattivato",
                isActive = user.IsActive
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Users", $"Error toggling status for user {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Users/{id}/change-password
    [HttpPut("{id}/change-password")]
    public async Task<IActionResult> ChangePassword(int id, ChangePasswordDto dto)
    {
        try
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null)
            {
                await _auditService.LogErrorAsync("Users - ChangePassword", $"User not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Utente non trovato" });
            }

            var (isPasswordValid, passwordError) = PasswordValidator.Validate(dto.NewPassword);
            if (!isPasswordValid)
            {
                await _auditService.LogErrorAsync("Users - ChangePassword", $"Invalid password for user: {user.UserName}", null, user.TenantId, user.Id);
                return BadRequest(new { success = false, error = passwordError });
            }

            user.Password = _passwordHasher.HashPassword(dto.NewPassword);
            user.LastModificationTime = DateTime.UtcNow;

            await _userRepository.UpdateUserAsync(user);
            await _auditService.LogActionAsync("Users", "ChangePassword", $"Changed password for user {user.UserName}", null, user.TenantId, user.Id);

            return Ok(new { success = true, message = "Password aggiornata con successo" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Users", $"Error changing password for user {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Users/roles
    [HttpGet("roles")]
    public async Task<ActionResult> GetRoles()
    {
        try
        {
            var roles = await _userRepository.GetRolesAsync();
            return Ok(new { success = true, data = roles.Select(r => new { r.Id, r.Name }) });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Users", "Error retrieving roles", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
