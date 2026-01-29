using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkBotAI.API.Data;
using WorkBotAI.API.DTOs;
using WorkBotAI.API.Models;

namespace WorkBotAI.API.Controllers;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly WorkBotAIContext _context;

    public CustomersController(WorkBotAIContext context)
    {
        _context = context;
    }

    // GET: api/Customers
    [HttpGet]
    public async Task<ActionResult> GetCustomers([FromQuery] Guid? tenantId, [FromQuery] string? search)
    {
        var query = _context.Customers
            .Where(c => c.IsDeleted != true)
            .Include(c => c.Tenant)
            .AsQueryable();

        // Filtro per tenant
        if (tenantId.HasValue)
        {
            query = query.Where(c => c.TenantId == tenantId.Value);
        }

        // Filtro ricerca
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => 
                (c.FullName != null && c.FullName.Contains(search)) ||
                (c.Phone != null && c.Phone.Contains(search)) ||
                (c.Email != null && c.Email.Contains(search))
            );
        }

        var customersRaw = await query
            .OrderBy(c => c.FullName)
            .ToListAsync();

        var customers = new List<CustomerListDto>();
        foreach (var c in customersRaw)
        {
            var appointmentsCount = await _context.Appointments
                .CountAsync(a => a.CustomerId == c.Id && a.IsDeleted != true);
            
            customers.Add(new CustomerListDto
            {
                Id = c.Id,
                TenantId = c.TenantId,
                TenantName = c.Tenant != null ? c.Tenant.Name : null,
                FullName = c.FullName,
                Phone = c.Phone,
                Email = c.Email,
                Note = c.Note,
                CreationTime = c.CreationTime,
                AppointmentsCount = appointmentsCount
            });
        }

        return Ok(new { success = true, data = customers, count = customers.Count });
    }

    // GET: api/Customers/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCustomer(int id)
    {
        var customer = await _context.Customers
            .Where(c => c.Id == id && c.IsDeleted != true)
            .Include(c => c.Tenant)
            .FirstOrDefaultAsync();

        if (customer == null)
        {
            return NotFound(new { success = false, error = "Cliente non trovato" });
        }

        // Ultimi appuntamenti
        var recentAppointments = await _context.Appointments
            .Where(a => a.CustomerId == id && a.IsDeleted != true)
            .Include(a => a.Staff)
            .Include(a => a.Status)
            .OrderByDescending(a => a.StartTime)
            .Take(10)
            .Select(a => new CustomerAppointmentDto
            {
                Id = a.Id,
                StartTime = a.StartTime,
                StaffName = a.Staff != null ? a.Staff.Name : null,
                StatusName = a.Status != null ? a.Status.Name : null,
                Note = a.Note
            })
            .ToListAsync();

        var dto = new CustomerDetailDto
        {
            Id = customer.Id,
            TenantId = customer.TenantId,
            TenantName = customer.Tenant?.Name,
            FullName = customer.FullName,
            Phone = customer.Phone,
            Email = customer.Email,
            Note = customer.Note,
            CreationTime = customer.CreationTime,
            IsDeleted = customer.IsDeleted ?? false,
            RecentAppointments = recentAppointments
        };

        return Ok(new { success = true, data = dto });
    }

    // POST: api/Customers
    [HttpPost]
    public async Task<ActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        // Verifica tenant
        var tenant = await _context.Tenants.FindAsync(dto.TenantId);
        if (tenant == null)
        {
            return BadRequest(new { success = false, error = "Tenant non trovato" });
        }

        var customer = new Customer
        {
            TenantId = dto.TenantId,
            FullName = dto.FullName,
            Phone = dto.Phone,
            Email = dto.Email,
            Note = dto.Note,
            CreationTime = DateTime.UtcNow,
            IsDeleted = false
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, new { 
            success = true, 
            data = new CustomerListDto
            {
                Id = customer.Id,
                TenantId = customer.TenantId,
                TenantName = tenant.Name,
                FullName = customer.FullName,
                Phone = customer.Phone,
                Email = customer.Email,
                Note = customer.Note,
                CreationTime = customer.CreationTime,
                AppointmentsCount = 0
            }
        });
    }

    // PUT: api/Customers/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null || customer.IsDeleted == true)
        {
            return NotFound(new { success = false, error = "Cliente non trovato" });
        }

        customer.FullName = dto.FullName;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        customer.Note = dto.Note;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Cliente aggiornato" });
    }

    // DELETE: api/Customers/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCustomer(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
        {
            return NotFound(new { success = false, error = "Cliente non trovato" });
        }

        // Soft delete
        customer.IsDeleted = true;
        customer.DeletionTime = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { success = true, message = "Cliente eliminato" });
    }

    // GET: api/Customers/tenant/{tenantId}/stats
    [HttpGet("tenant/{tenantId}/stats")]
    public async Task<ActionResult> GetTenantCustomerStats(Guid tenantId)
    {
        var totalCustomers = await _context.Customers
            .Where(c => c.TenantId == tenantId && c.IsDeleted != true)
            .CountAsync();

        var thisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var newThisMonth = await _context.Customers
            .Where(c => c.TenantId == tenantId && c.IsDeleted != true && c.CreationTime >= thisMonth)
            .CountAsync();

        return Ok(new { 
            success = true, 
            data = new {
                totalCustomers,
                newThisMonth
            }
        });
    }
}