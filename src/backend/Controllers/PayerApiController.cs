using Microsoft.AspNetCore.Mvc;
using PaydayBackend.Models;
using PaydayBackend.Requests;
using PaydayBackend.Services;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Controllers;

[ApiController]
[Route("/api/payers")]
public class PayerApiController(IPayersRespository payers, CursorService cursorService) : Controller
{
    private readonly IPayersRespository _payers = payers;

    [HttpGet("{id?}")]
    public async Task<IActionResult> GetPayers(
        int? id = null,
        int size = 100,
        string? cursor = null,
        CancellationToken cancel = default
    )
    {
        if (id.HasValue)
        {
            var payer = await _payers.GetByIdAsync(id.Value, cancel);
            if (payer is null)
                return NotFound();
            else
                return Ok(payer);
        }

        var after = 0;

        if (!string.IsNullOrEmpty(cursor))
        {
            var decodedCursor = cursorService.DecodeCursor(cursor);
            if (decodedCursor is null)
                return BadRequest();

            after = decodedCursor.Id;
        }

        var payers = await _payers.GetAllAsync(after, size + 1, cancel);
        if (payers is null)
            return BadRequest();

        var nextCursor =
            payers.Count > size ? cursorService.EncodeCursor(payers.ElementAt(size - 1).Id) : "";

        return Ok(new { Data = payers.Take(size), NextCursor = nextCursor });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayer(
        CreatePayerRequest request,
        CancellationToken cancel = default
    )
    {
        var payer = (Payer)request;
        if (payer is null)
            return BadRequest();

        var ok = await _payers.CreateAsync(payer, cancel);
        if (!ok)
            return BadRequest();

        return CreatedAtAction(nameof(GetPayers), new { id = payer.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePayer(
        int id,
        UpdatePayerRequest request,
        CancellationToken cancel = default
    )
    {
        var ok = await _payers.UpdateAsync(id, patch: request, cancel);
        if (!ok)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayer(int id, CancellationToken cancel = default)
    {
        var ok = await _payers.DeleteAsync(id, cancel);
        if (!ok)
            return NotFound();

        return NoContent();
    }
}
