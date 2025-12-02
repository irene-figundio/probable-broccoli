using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkBotAI.API.DTOs;
using WorkbotAI.Models;
using WorkBotAI.Repositories.DataAccess.Repositories.Interfaces;
using System.Linq;

namespace WorkBotAI.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _customerRepository;

    public CustomersController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    // GET: api/Customers
    [HttpGet]
    public async Task<ActionResult> GetCustomers([FromQuery] Guid? tenantId, [FromQuery] string? search)
    {
        var customers = await _customerRepository.GetCustomersAsync(tenantId, search);
        return Ok(new { success = true, data = customers, count = customers.Count() });
    }

    // GET: api/Customers/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCustomer(int id)
    {
        var customer = await _customerRepository.GetCustomerByIdAsync(id);
        if (customer == null)
            return NotFound(new { success = false, error = "Cliente non trovato" });

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

    // POST: api/Customers
    [HttpPost]
    public async Task<ActionResult> CreateCustomer([FromBody] CreateCustomerDto dto)
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

    // PUT: api/Customers/{id}
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
    {
        var customer = await _customerRepository.GetCustomerByIdAsync(id);
        if (customer == null)
            return NotFound(new { success = false, error = "Cliente non trovato" });

        customer.FullName = dto.FullName;
        customer.Phone = dto.Phone;
        customer.Email = dto.Email;
        customer.Note = dto.Note;

        await _customerRepository.UpdateCustomerAsync(customer);
        return Ok(new { success = true, message = "Cliente aggiornato" });
    }

    // DELETE: api/Customers/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCustomer(int id)
    {
        await _customerRepository.DeleteCustomerAsync(id);
        return Ok(new { success = true, message = "Cliente eliminato" });
    }

    // GET: api/Customers/tenant/{tenantId}/stats
    [HttpGet("tenant/{tenantId}/stats")]
    public async Task<ActionResult> GetTenantCustomerStats(Guid tenantId)
    {
        var totalCustomers = await _customerRepository.GetTotalCustomersAsync(tenantId);
        var newThisMonth = await _customerRepository.GetNewCustomersThisMonthAsync(tenantId);
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
}