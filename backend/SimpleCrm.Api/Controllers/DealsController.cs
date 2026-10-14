using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimpleCrm.Api.Data;
using SimpleCrm.Api.Dtos;
using SimpleCrm.Api.Models;

namespace SimpleCrm.Api.Controllers;

[ApiController]
[Route("api/deals")]
[Authorize]
public class DealsController : ControllerBase
{
    private readonly CrmDbContext _db;

    public DealsController(CrmDbContext db)
    {
        _db = db;
    }

    // GET api/deals?search=webb&stage=Lead&customerId=2
    [HttpGet]
    public async Task<ActionResult<List<DealDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] DealStage? stage,
        [FromQuery] int? customerId)
    {
        var query = _db.Deals.AsQueryable();

        if (stage.HasValue)
        {
            query = query.Where(d => d.Stage == stage.Value);
        }

        if (customerId.HasValue)
        {
            query = query.Where(d => d.CustomerId == customerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.Title.Contains(search) || d.Customer!.Name.Contains(search));
        }

        var deals = await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DealDto(d.Id, d.Title, d.Amount, d.Stage, d.ExpectedCloseDate, d.CreatedAt, d.CustomerId, d.Customer!.Name))
            .ToListAsync();

        return Ok(deals);
    }

    // GET api/deals/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DealDto>> GetById(int id)
    {
        var deal = await GetDto(id);
        return deal is null ? NotFound() : Ok(deal);
    }

    // POST api/deals
    [HttpPost]
    public async Task<ActionResult<DealDto>> Create(SaveDealDto dto)
    {
        if (!await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId))
        {
            return BadRequest("Kunden finns inte.");
        }

        var deal = new Deal();
        Apply(dto, deal);

        _db.Deals.Add(deal);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = deal.Id }, await GetDto(deal.Id));
    }

    // PUT api/deals/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, SaveDealDto dto)
    {
        var deal = await _db.Deals.FindAsync(id);
        if (deal is null)
        {
            return NotFound();
        }

        if (!await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId))
        {
            return BadRequest("Kunden finns inte.");
        }

        Apply(dto, deal);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE api/deals/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deal = await _db.Deals.FindAsync(id);
        if (deal is null)
        {
            return NotFound();
        }

        _db.Deals.Remove(deal);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<DealDto?> GetDto(int id) =>
        await _db.Deals
            .Where(d => d.Id == id)
            .Select(d => new DealDto(d.Id, d.Title, d.Amount, d.Stage, d.ExpectedCloseDate, d.CreatedAt, d.CustomerId, d.Customer!.Name))
            .FirstOrDefaultAsync();

    private static void Apply(SaveDealDto dto, Deal deal)
    {
        deal.Title = dto.Title.Trim();
        deal.Amount = dto.Amount;
        deal.Stage = dto.Stage;
        deal.ExpectedCloseDate = dto.ExpectedCloseDate;
        deal.CustomerId = dto.CustomerId;
    }
}
