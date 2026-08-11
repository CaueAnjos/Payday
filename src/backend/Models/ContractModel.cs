using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Models;

[Table("Contracts", Schema = "Contract")]
public class Contract : Entity
{
    public List<Payer> Participants { get; set; } = [];
    public List<Payment> Payments { get; set; } = [];

    [Required]
    public DateTime CreationDate { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; }

    [Required]
    [MaxLength(200)]
    public string Label { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }
}
