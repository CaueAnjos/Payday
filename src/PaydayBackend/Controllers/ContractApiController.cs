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
    IPaymentsRepository payments,
    IPaginatorService<Contract> paginator
) : Controller
{
    private readonly IContractsRepository _contracts = contracts;
    private readonly IPaymentsRepository _payments = payments;
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
        await _contracts.AddPayersAsync(id, request.PayerIds, cancel);
        return Ok();
    }

    [HttpDelete("{id}/participants/{participantId}")]
    public async Task<IActionResult> RemoveParticipant(
        int id,
        int participantId,
        CancellationToken cancel = default
    )
    {
        await _contracts.RemovePayerAsync(id, participantId, cancel);
        return NoContent();
    }

    [HttpPost("{id}/signatures/participant/{participantId}")]
    public async Task<IActionResult> AddCloseSignature(
        int id,
        int participantId,
        CancellationToken cancel = default
    )
    {
        await _contracts.AddCloseSignature(id, participantId, cancel);
        return NoContent();
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

    [HttpGet("{id}/payments")]
    public async Task<IActionResult> GetContractPayments(
        int id,
        CancellationToken cancel = default
    )
    {
        var contractPayments = await _payments.GetByContractIdAsync(id, cancel);
        return Ok(contractPayments.Select(p => (DefaultPaymentResponse)p).ToList());
    }

    [HttpGet("{id}/payments/participant/{participantId}")]
    public async Task<IActionResult> GetContractParticipantPayments(
        int id,
        int participantId,
        CancellationToken cancel = default
    )
    {
        var participantPayments = await _payments.GetByContractAndParticipantIdAsync(
            id,
            participantId,
            cancel
        );
        return Ok(participantPayments.Select(p => (DefaultPaymentResponse)p).ToList());
    }

    [HttpPost("{id}/payments/participant/{participantId}")]
    public async Task<IActionResult> CreateContractParticipantPayments(
        int id,
        int participantId,
        List<CreatePaymentRequest> request,
        CancellationToken cancel = default
    )
    {
        var newPayments = request.Select(r => r.ToPayment(participantId, id)).ToList();
        var created = await _payments.CreateContractParticipantPaymentsAsync(
            id,
            participantId,
            newPayments,
            cancel
        );

        return CreatedAtAction(
            nameof(GetContractParticipantPayments),
            new { id, participantId },
            created.Select(p => (DefaultPaymentResponse)p).ToList()
        );
    }

    [HttpDelete("{id}/payments/participant/{participantId}/{paymentId}")]
    public async Task<IActionResult> DeleteContractParticipantPayment(
        int id,
        int participantId,
        int paymentId,
        CancellationToken cancel = default
    )
    {
        var payment = await _payments.GetByIdAsync(paymentId, cancel);
        if (payment is null || payment.ContractId != id || payment.OwnerId != participantId)
            return NotFound();

        await _payments.DeleteAsync(paymentId, cancel);
        return NoContent();
    }
}
