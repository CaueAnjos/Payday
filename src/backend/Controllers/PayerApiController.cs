using Microsoft.AspNetCore.Mvc;
using PaydayBackend.Models;
using PaydayBackend.Requests;
using PaydayBackend.Services.Pagination;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Controllers;

[ApiController]
[Route("/api/payers")]
public class PayerApiController(IPayersRepository payers, IPaginatorService<Payer> paginator)
    : Controller
{
    private readonly IPayersRepository _payers = payers;
    private readonly IPaginatorService<Payer> _paginator = paginator;

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

        Page<Payer> page;
        if (cursor is null)
            page = await _paginator.MakePageAsync(new Cursor(0), size, cancel);
        else
            page = await _paginator.MakePageAsync(cursor, size, cancel);

        return Ok(page);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayer(
        CreatePayerRequest request,
        CancellationToken cancel = default
    )
    {
        var payer = (Payer)request;

        await _payers.CreateAsync(payer, cancel);
        return CreatedAtAction(nameof(GetPayers), new { id = payer.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutPayer(
        int id,
        UpdatePayerRequest request,
        CancellationToken cancel = default
    )
    {
        var payer = (Payer)request;
        payer.Id = id;

        var created = await _payers.CreateOrReplaceAsync(payer, cancel);
        if (created)
            return CreatedAtAction(nameof(GetPayers), new { id });

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePayer(int id, CancellationToken cancel = default)
    {
        await _payers.DeleteAsync(id, cancel);
        return NoContent();
    }
}
