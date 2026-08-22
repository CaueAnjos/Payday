using Microsoft.AspNetCore.Mvc;
using PaydayBackend.Models;
using PaydayBackend.Requests;
using PaydayBackend.Services.Pagination;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Controllers;

[ApiController]
[Route("/api/contracts")]
public class ContractApiController(
    IContractsRepository contracts,
    IPaginatorService<Contract> paginator
) : Controller
{
    private readonly IContractsRepository _contracts = contracts;
    private readonly IPaginatorService<Contract> _paginator = paginator;

    [HttpGet("{id?}")]
    public async Task<IActionResult> GetContracts(
        int? id = null,
        int size = 100,
        string? cursor = null,
        CancellationToken cancel = default
    )
    {
        if (id.HasValue)
        {
            var payer = await _contracts.GetByIdAsync(id.Value, cancel);
            if (payer is null)
                return NotFound();
            else
                return Ok((DefaultContractResponse)payer);
        }

        Page<Contract> page;
        if (cursor is null)
            page = await _paginator.MakePageAsync(new Cursor(0), size, cancel);
        else
            page = await _paginator.MakePageAsync(cursor, size, cancel);

        var contractPage = new Page<DefaultContractResponse>(
            page.Size,
            page.Items.Select(c => (DefaultContractResponse)c).ToList(),
            page.PreviousCursor,
            page.NextCursor
        );

        return Ok(contractPage);
    }

    [HttpPost("{id}")]
    public async Task<IActionResult> AddPayerToContract(
        int id,
        AddPayerToContractRequest request,
        CancellationToken cancel = default
    )
    {
        await _contracts.AddPayersAsync(id, request.PayerIds);
        return Ok();
    }

    [HttpPost("{id}/signatures/participant/{participantId}")]
    public async Task<IActionResult> AddCloseSignature(
        int id,
        int participantId,
        CancellationToken cancel = default
    )
    {
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> CreateContract(
        CreateContractRequest request,
        CancellationToken cancel = default
    )
    {
        var contract = (Contract)request;

        await _contracts.CreateAsync(contract, cancel);
        return CreatedAtAction(nameof(GetContracts), new { id = contract.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContract(int id, CancellationToken cancel = default)
    {
        await _contracts.DeleteAsync(id, cancel);
        return NoContent();
    }
}
