using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Models;

[Table("Contracts", Schema = "Signatures")]
public class Signature : Entity
{
    [ForeignKey(nameof(Owner))]
    public int OwnerId { get; set; } = default!;

    [Required]
    public Payer Owner { get; set; } = default!;

    [ForeignKey(nameof(Contract))]
    public int ContractId { get; set; }

    [Required]
    public Contract Contract { get; set; } = default!;

    [Required]
    public DateTime CreationDate { get; set; }
}
