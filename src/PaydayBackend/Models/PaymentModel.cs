using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Models;

// Payments are immutable: once created they can only be deleted (never edited), so
// every business field is `init`-only.
[Table("Payments", Schema = "Contract")]
public class Payment : Entity
{
    [ForeignKey(nameof(Owner))]
    public int OwnerId { get; init; }

    [Required]
    public Payer Owner { get; init; } = default!;

    // Payments created directly for a Payer (not tied to any contract) leave this null.
    [ForeignKey(nameof(Contract))]
    public int? ContractId { get; init; }

    public Contract? Contract { get; init; }

    [Required]
    public DateTime CreationDate { get; init; }

    [NotMapped]
    public DateTime? SignedDate => Signature?.CreationDate;

    [Required]
    [Precision(18, 2)]
    public decimal Price { get; init; } = default;

    [Required]
    [MaxLength(200)]
    public string Label { get; init; } = default!;

    [MaxLength(500)]
    public string? Description { get; init; }

    public Signature? Signature { get; init; }
}
