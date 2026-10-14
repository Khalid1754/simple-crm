using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleCrm.Api.Data;
using SimpleCrm.Api.Dtos;
using SimpleCrm.Api.Models;

namespace SimpleCrm.Api.Controllers;

[ApiController]
[Route("api/customers")]
[Authorize] // Man måste vara inloggad (skicka med en giltig token) för att nå dessa endpoints.
public class CustomersController : ControllerBase
{
    private readonly CrmDbContext _db;

    public CustomersController(CrmDbContext db)
    {
        _db = db;
    }

    // GET api/customers?search=acme
    [HttpGet]
    public async Task<ActionResult<List<CustomerDto>>> GetAll([FromQuery] string? search)
    {
        var query = _db.Customers.AsQueryable();

        // Enkel sökning i namn, e-post, ort och org.nr.
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.Name.Contains(search) ||
                (c.Email != null && c.Email.Contains(search)) ||
                (c.City != null && c.City.Contains(search)) ||
                (c.OrgNumber != null && c.OrgNumber.Contains(search)));
        }

        var customers = await query
            .OrderBy(c => c.Name)
            .Select(c => new CustomerDto(c.Id, c.Name, c.OrgNumber, c.Email, c.Phone, c.City, c.CreatedAt, c.Contacts.Count, c.Deals.Count))
            .ToListAsync();

        return Ok(customers);
    }

    // GET api/customers/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerDto>> GetById(int id)
    {
        var customer = await _db.Customers
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto(c.Id, c.Name, c.OrgNumber, c.Email, c.Phone, c.City, c.CreatedAt, c.Contacts.Count, c.Deals.Count))
            .FirstOrDefaultAsync();

        return customer is null ? NotFound() : Ok(customer);
    }

    // POST api/customers
    [HttpPost]
    public async Task<ActionResult<CustomerDto>> Create(SaveCustomerDto dto)
    {
        var customer = new Customer();
        Apply(dto, customer);

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        // 201 Created + adressen till den nya kunden.
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, ToDto(customer));
    }

    // PUT api/customers/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveCustomerDto dto)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        Apply(dto, customer);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/customers/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _db.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        // Kundens kontakter och affärer tas bort automatiskt (cascade delete).
        _db.Customers.Remove(customer);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // Hjälpmetoder för att översätta mellan entitet och DTO.
    // OBS: i databasfrågorna ovan skriver vi "new CustomerDto(...)" direkt i Select,
    // så att EF Core kan räkna kontakter/affärer med SQL (COUNT) i stället för i minnet.
    private static CustomerDto ToDto(Customer c) =>
        new(c.Id, c.Name, c.OrgNumber, c.Email, c.Phone, c.City, c.CreatedAt, c.Contacts.Count, c.Deals.Count);

    private static void Apply(SaveCustomerDto dto, Customer customer)
    {
        customer.Name = dto.Name.Trim();
        customer.OrgNumber = dto.OrgNumber;
        customer.Email = dto.Email;
        customer.Phone = dto.Phone;
        customer.City = dto.City;
    }
}
