using System.ComponentModel.DataAnnotations;
using PaydayBackend.Models;

namespace PaydayBackend.Requests;

public record CreatePaymentRequest(
    [Required, MaxLength(200)] string Label,
    [Required, Range(0.01, double.MaxValue)] decimal Price,
    [MaxLength(500)] string? Description = null
)
{
    public Payment ToPayment(int ownerId, int? contractId = null)
    {
        return new Payment
        {
            OwnerId = ownerId,
            ContractId = contractId,
            CreationDate = DateTime.UtcNow,
            Label = Label,
            Price = Price,
            Description = Description,
        };
    }
}
