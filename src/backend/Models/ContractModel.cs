using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Models;

public enum ContractState
{
    Open, // Can insert new payments and can't be paid
    AwaitingClosure, // Can't insert new payments and need to be paid and signed
    Closed, // Was paid and signed
}

[Table("Contracts", Schema = "Contract")]
public class Contract : Entity
{
    public ContractState State { get; set; } = ContractState.Open;

    public List<Payer> Participants { get; set; } = [];

    [NotMapped]
    public IEnumerable<Payment> Payments =>
        Participants
            .SelectMany(p => p.Payments)
            .Where(p => p.Signature is null && p.CreationDate <= PaymentDate);

    [Required]
    public DateTime CreationDate { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    public DateTime? CloseDate { get; set; }
    public List<Signature>? CloseSignatures { get; set; }

    [Required]
    [MaxLength(200)]
    public string Label { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }
}
