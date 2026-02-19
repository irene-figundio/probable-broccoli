using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkBotAI.API.DTOs;
using WorkbotAI.Models;
using WorkBotAI.API.Services;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;

namespace WorkBotAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IAuditService _auditService;

    public CustomersController(ICustomerRepository customerRepository, IAuditService auditService)
    {
        _customerRepository = customerRepository;
        _auditService = auditService;
    }

    // GET: api/Customers
    [HttpGet]
    public async Task<ActionResult> GetCustomers([FromQuery] Guid? tenantId, [FromQuery] string? search)
    {
        try
        {
            var customers = await _customerRepository.GetCustomersAsync(tenantId, search);
                if (customers == null) {
                    await _auditService.LogActionAsync("Customers", "GetCustomers", "No customers found", null, tenantId);
                    return Ok(new { success = true, data = Array.Empty<CustomerListDto>(), count = 0 });
                } else {
                    await _auditService.LogActionAsync("Customers", "GetCustomers", $"Retrieved {customers.Count()} customers", null, tenantId);
                }
            return Ok(new { success = true, data = customers, count = customers.Count() });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Customers", "Error retrieving customers", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Customers/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCustomer(int id)
    {
        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                await _auditService.LogErrorAsync("Customers - GetCustomer", $"Customer not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Cliente non trovato" });
            } else {
                await _auditService.LogActionAsync("Customers", "Get", $"Retrieved customer {id}", null, customer.TenantId);
            }

            var recentAppointments = await _customerRepository.GetRecentAppointmentsAsync(id);
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
                RecentAppointments = recentAppointments.Select(a => new CustomerAppointmentDto
                {
                    Id = a.Id,
                    StartTime = a.StartTime,
                    StaffName = a.Staff != null ? a.Staff.Name : null,
                    StatusName = a.Status != null ? a.Status.Name : null,
                    Note = a.Note
                }).ToList()
            };
            return Ok(new { success = true, data = dto });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Customers", $"Error retrieving customer {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // POST: api/Customers
    [HttpPost]
    public async Task<ActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        try
        {
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

            var createdCustomer = await _customerRepository.CreateCustomerAsync(customer);
            await _auditService.LogActionAsync("Customers", "Create", $"Created customer {createdCustomer.FullName} ({createdCustomer.Id})", null, dto.TenantId);

            return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, new
            {
                success = true,
                data = new CustomerListDto
                {
                    Id = createdCustomer.Id,
                    TenantId = createdCustomer.TenantId,
                    FullName = createdCustomer.FullName,
                    Phone = createdCustomer.Phone,
                    Email = createdCustomer.Email,
                    Note = createdCustomer.Note,
                    CreationTime = createdCustomer.CreationTime,
                    AppointmentsCount = 0
                }
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Customers", "Error creating customer", ex, dto.TenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // PUT: api/Customers/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
    {
        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                await _auditService.LogErrorAsync("Customers - UpdateCustomer", $"Customer not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Cliente non trovato" });
            } else {
                await _auditService.LogActionAsync("Customers", "Update Attempt", $"Attempting to update customer {id}", null, customer.TenantId);
            }

            customer.FullName = dto.FullName;
            customer.Phone = dto.Phone;
            customer.Email = dto.Email;
            customer.Note = dto.Note;

            await _customerRepository.UpdateCustomerAsync(customer);
            await _auditService.LogActionAsync("Customers", "Update", $"Updated customer {id}", null, customer.TenantId);

            return Ok(new { success = true, message = "Cliente aggiornato" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Customers", $"Error updating customer {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // DELETE: api/Customers/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCustomer(int id)
    {
        try
        {
            var customer = await _customerRepository.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                await _auditService.LogErrorAsync("Customers - DeleteCustomer", $"Customer not found: {id}", null, null, null);
                return NotFound(new { success = false, error = "Cliente non trovato" });
            } else {
                await _auditService.LogActionAsync("Customers", "Delete Attempt", $"Attempting to delete customer {id}", null, customer.TenantId);
            }

            await _customerRepository.DeleteCustomerAsync(id);
            await _auditService.LogActionAsync("Customers", "Delete", $"Deleted customer {id}", null, customer.TenantId);
            return Ok(new { success = true, message = "Cliente eliminato" });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Customers", $"Error deleting customer {id}", ex);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }

    // GET: api/Customers/tenant/{tenantId}/stats
    [HttpGet("tenant/{tenantId}/stats")]
    public async Task<ActionResult> GetTenantCustomerStats(Guid tenantId)
    {
        try
        {

            var totalCustomers = await _customerRepository.GetTotalCustomersAsync(tenantId);
            var newThisMonth = await _customerRepository.GetNewCustomersThisMonthAsync(tenantId);
            await _auditService.LogActionAsync("Customers", "GetTenantCustomerStats", $"Retrieved customer stats for tenant {tenantId}", null, tenantId);
            return Ok(new
            {
                success = true,
                data = new
                {
                    totalCustomers,
                    newThisMonth
                }
            });
        }
        catch (Exception ex)
        {
            await _auditService.LogErrorAsync("Customers", $"Error retrieving customer stats for tenant {tenantId}", ex, tenantId);
            return StatusCode(500, new { success = false, error = "INTERNAL_SERVER_ERROR" });
        }
    }
}
