using Microsoft.AspNetCore.Mvc;
using PaydayBackend.Models;
using PaydayBackend.Requests;
using PaydayBackend.Services.Pagination;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Controllers;

[ApiController]
[Route("/api/payers")]
public class PayerApiController(
    IPayersRepository payers,
    IPaymentsRepository payments,
    IPaginatorService<Payer> paginator
) : Controller
{
    private readonly IPayersRepository _payers = payers;
    private readonly IPaymentsRepository _payments = payments;
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
                return Ok((DefaultPayerResponse)payer);
        }

        Page page;
        var mapperFunc = (Payer p) => (DefaultPayerResponse)p;
        if (cursor is null)
            page = await _paginator.MakePageAsync(new Cursor(0), size, mapperFunc, cancel);
        else
            page = await _paginator.MakePageAsync(cursor, size, mapperFunc, cancel);

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

    [HttpGet("{id}/payments")]
    public async Task<IActionResult> GetPayerPayments(int id, CancellationToken cancel = default)
    {
        var payerPayments = await _payments.GetByPayerIdAsync(id, cancel);
        return Ok(payerPayments.Select(p => (DefaultPaymentResponse)p).ToList());
    }

    [HttpPost("{id}/payments")]
    public async Task<IActionResult> CreatePayerPayments(
        int id,
        List<CreatePaymentRequest> request,
        CancellationToken cancel = default
    )
    {
        var newPayments = request.Select(r => r.ToPayment(id)).ToList();
        var created = await _payments.CreatePayerPaymentsAsync(id, newPayments, cancel);

        return CreatedAtAction(
            nameof(GetPayerPayments),
            new { id },
            created.Select(p => (DefaultPaymentResponse)p).ToList()
        );
    }

    [HttpDelete("{id}/payments/{paymentId}")]
    public async Task<IActionResult> DeletePayerPayment(
        int id,
        int paymentId,
        CancellationToken cancel = default
    )
    {
        var payment = await _payments.GetByIdAsync(paymentId, cancel);
        if (payment is null || payment.OwnerId != id)
            return NotFound();

        await _payments.DeleteAsync(paymentId, cancel);
        return NoContent();
    }
}
