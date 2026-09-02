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

        Page page;
        var mapperFunc = (Contract c) => (DefaultContractResponse)c;
        if (cursor is null)
            page = await _paginator.MakePageAsync(new Cursor(0), size, mapperFunc, cancel);
        else
            page = await _paginator.MakePageAsync(cursor, size, mapperFunc, cancel);

        return Ok(page);
    }

    [HttpPost("{id}/participants")]
    public async Task<IActionResult> AddPayerToContract(
        int id,
        AddParticipantsRequest request,
        CancellationToken cancel = default
    )
    {
        await _contracts.AddPayersAsync(id, request.PayerIds);
        return Ok();
    }

    [HttpDelete("{id}/participants/{participantId}")]
    public async Task<IActionResult> RemoveParticipant(
        int id,
        int participantId,
        CancellationToken cancel = default
    )
    {
        await _contracts.RemovePayerAsync(id, participantId);
        return NoContent();
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
