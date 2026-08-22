using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Models;

[Table("Payers", Schema = "Contract")]
public class Payer : Entity
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    [Column(TypeName = "citext")]
    public string Email { get; set; } = default!;

    public List<Payment> Payments { get; set; } = [];
    public List<Contract> Contracts { get; set; } = [];
    public List<Signature> Signatures { get; set; } = [];
}
