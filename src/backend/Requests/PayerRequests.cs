using System.ComponentModel.DataAnnotations;
using PaydayBackend.Models;

namespace PaydayBackend.Requests;

public record DefaultPaymentResponse(
    int Id,
    [Required] DateTime CreationDate,
    [Required] decimal Price, // FIX: should have data validation here!
    [Required, MaxLength(200)] string Label,
    [MaxLength(500)] string? Description,
    DateTime? SignedDate,
    Signature? Signature
)
{
    public static explicit operator DefaultPaymentResponse(Payment payment)
    {
        return new DefaultPaymentResponse(
            payment.Id,
            payment.CreationDate,
            payment.Price,
            payment.Label,
            payment.Description,
            payment.SignedDate,
            payment.Signature
        );
    }
};

public record DefaultPayerResponse(
    int Id,
    [Required, MaxLength(255)] string Name,
    [Required, EmailAddress, MaxLength(254)] string Email,
    IReadOnlyList<DefaultPaymentResponse> Payments,
    IReadOnlyList<DefaultContractResponse> Contracts
)
{
    public static explicit operator DefaultPayerResponse(Payer payer)
    {
        return new DefaultPayerResponse(
            payer.Id,
            payer.Name,
            payer.Email,
            payer.Payments.Select(p => (DefaultPaymentResponse)p).ToList(),
            payer.Contracts.Select(c => (DefaultContractResponse)c).ToList()
        );
    }
};

public record CreatePayerRequest(
    [Required, MaxLength(255)] string Name,
    [Required, EmailAddress, MaxLength(254)] string Email
)
{
    public static explicit operator Payer(CreatePayerRequest request)
    {
        return new Payer { Name = request.Name, Email = request.Email };
    }
};

public record UpdatePayerRequest(
    [Required, MaxLength(255)] string Name,
    [Required, EmailAddress, MaxLength(254)] string Email
)
{
    public static explicit operator Payer(UpdatePayerRequest request)
    {
        return new Payer { Name = request.Name, Email = request.Email };
    }
}
