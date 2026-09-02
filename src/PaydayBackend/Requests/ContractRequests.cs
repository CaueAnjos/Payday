using System.ComponentModel.DataAnnotations;
using PaydayBackend.Models;

namespace PaydayBackend.Requests;

public record ParticipantResponse(
    int Id,
    [Required, MaxLength(255)] string Name,
    [Required, EmailAddress, MaxLength(254)] string Email
)
{
    public static explicit operator ParticipantResponse(Payer payer)
    {
        return new ParticipantResponse(payer.Id, payer.Name, payer.Email);
    }
};

public record DefaultContractResponse(
    int Id,
    [Required, MaxLength(200)] string Label,
    [MaxLength(500)] string? Description,
    ContractState State,
    [Required] DateTime CreationDate,
    [Required] DateTime PaymentDate,
    IReadOnlyList<ParticipantResponse> Participants,
    IReadOnlyList<Payment> Payments,
    DateTime? CloseDate,
    IReadOnlyList<Signature> CloseSignatures
)
{
    public static explicit operator DefaultContractResponse(Contract contract)
    {
        return new DefaultContractResponse(
            contract.Id,
            contract.Label,
            contract.Description,
            contract.State,
            contract.CreationDate,
            contract.PaymentDate,
            contract.Participants.Select(p => (ParticipantResponse)p).ToList(),
            contract.Payments.ToList(),
            contract.CloseDate,
            contract.CloseSignatures
        );
    }
};

public record CreateContractRequest(
    [Required, MaxLength(200)] string Label,
    DateTime? PaymentDate = null,
    [MaxLength(500)] string? Description = null
)
{
    public static explicit operator Contract(CreateContractRequest request)
    {
        return new Contract
        {
            CreationDate = DateTime.UtcNow,
            PaymentDate = request.PaymentDate ?? DateTime.UtcNow.AddMonths(1),
            Label = request.Label,
            Description = request.Description,
        };
    }
}

public record UpdateContractRequest(
    [Required] DateTime PaymentDate,
    [Required, MaxLength(200)] string Label,
    [MaxLength(500)] string? Description = null
)
{
    public static explicit operator Contract(UpdateContractRequest request)
    {
        return new Contract
        {
            CreationDate = DateTime.UtcNow,
            PaymentDate = request.PaymentDate,
            Label = request.Label,
            Description = request.Description,
        };
    }
}

public record AddParticipantsRequest([Required] IReadOnlyList<int> PayerIds);
