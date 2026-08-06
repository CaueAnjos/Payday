using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models;
using PaydayBackend.Requests;
using PaydayBackend.Services;

namespace PaydayBackend.Controllers;

[ApiController]
[Route("/api/payers")]
public class PayerApiController(ContractContext db, CursorService cursorService) : Controller
{
    private readonly ContractContext db = db;
    private readonly CursorService cursorService = cursorService;

    [HttpGet("{id?}")]
    public async Task<IActionResult> GetPayers(
        int? id = null,
        int size = 100,
        string? cursor = null
    )
    {
        if (size <= 0)
            return BadRequest();

        if (id is not null)
        {
            var payer = await db.Payers.FindAsync(id);
            if (payer is null)
                return NotFound();
            else
                return Ok(payer);
        }

        var after = 0;

        try
        {
            if (!string.IsNullOrEmpty(cursor))
            {
                var decodedCursor = cursorService.DecodeCursor(cursor);
                if (decodedCursor is null)
                    return BadRequest();

                after = decodedCursor.Id;
            }
        }
        catch
        {
            return BadRequest();
        }

        var payers = await db
            .Payers.Where(p => p.Id > after)
            .OrderBy(p => p.Id)
            .Take(size)
            .ToListAsync();

        var nextCursor = payers.Count == size ? cursorService.EncodeCursor(payers.Last().Id) : "";

        return Ok(new { Data = payers, NextCursor = nextCursor });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayer(CreatePayerRequest request)
    {
        var payer = (Payer)request;
        if (payer is null)
            return BadRequest();

        await db.Payers.AddAsync(payer);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPayers), new { id = payer.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayer(int id, UpdatePayerRequest request)
    {
        var payer = await db.Payers.FindAsync(id);
        if (payer is null)
            return NotFound();

        payer.Name = request.Name;
        payer.Email = request.Email;

        await db.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayer(int id)
    {
        var payer = await db.Payers.FindAsync(id);
        if (payer is null)
            return NotFound();

        db.Payers.Remove(payer);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
