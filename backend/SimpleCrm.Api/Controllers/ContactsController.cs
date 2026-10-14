using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleCrm.Api.Data;
using SimpleCrm.Api.Dtos;
using SimpleCrm.Api.Models;

namespace SimpleCrm.Api.Controllers;

[ApiController]
[Route("api/contacts")]
[Authorize]
public class ContactsController : ControllerBase
{
    private readonly CrmDbContext _db;

    public ContactsController(CrmDbContext db)
    {
        _db = db;
    }

    // GET api/contacts?search=anna&customerId=1
    [HttpGet]
    public async Task<ActionResult<List<ContactDto>>> GetAll([FromQuery] string? search, [FromQuery] int? customerId)
    {
        var query = _db.Contacts.AsQueryable();

        if (customerId.HasValue)
        {
            query = query.Where(c => c.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c =>
                c.FirstName.Contains(search) ||
                c.LastName.Contains(search) ||
                (c.Email != null && c.Email.Contains(search)) ||
                c.Customer!.Name.Contains(search));
        }

        var contacts = await query
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Select(c => new ContactDto(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.JobTitle, c.CustomerId, c.Customer!.Name))
            .ToListAsync();

        return Ok(contacts);
    }

    // GET api/contacts/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ContactDto>> GetById(int id)
    {
        var contact = await _db.Contacts
            .Where(c => c.Id == id)
            .Select(c => new ContactDto(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.JobTitle, c.CustomerId, c.Customer!.Name))
            .FirstOrDefaultAsync();

        return contact is null ? NotFound() : Ok(contact);
    }

    // POST api/contacts
    [HttpPost]
    public async Task<ActionResult<ContactDto>> Create(SaveContactDto dto)
    {
        if (!await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId))
        {
            return BadRequest("Kunden finns inte.");
        }

        var contact = new Contact();
        Apply(dto, contact);

        _db.Contacts.Add(contact);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = contact.Id }, await GetDto(contact.Id));
    }

    // PUT api/contacts/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveContactDto dto)
    {
        var contact = await _db.Contacts.FindAsync(id);
        if (contact is null)
        {
            return NotFound();
        }

        if (!await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId))
        {
            return BadRequest("Kunden finns inte.");
        }

        Apply(dto, contact);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/contacts/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var contact = await _db.Contacts.FindAsync(id);
        if (contact is null)
        {
            return NotFound();
        }

        _db.Contacts.Remove(contact);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<ContactDto?> GetDto(int id) =>
        await _db.Contacts
            .Where(c => c.Id == id)
            .Select(c => new ContactDto(c.Id, c.FirstName, c.LastName, c.Email, c.Phone, c.JobTitle, c.CustomerId, c.Customer!.Name))
            .FirstOrDefaultAsync();

    private static void Apply(SaveContactDto dto, Contact contact)
    {
        contact.FirstName = dto.FirstName.Trim();
        contact.LastName = dto.LastName.Trim();
        contact.Email = dto.Email;
        contact.Phone = dto.Phone;
        contact.JobTitle = dto.JobTitle;
        contact.CustomerId = dto.CustomerId;
    }
}
