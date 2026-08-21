using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models.Abstractions;

namespace PaydayBackend.Models;

[Table("Payments", Schema = "Contract")]
public class Payment : Entity
{
    [ForeignKey(nameof(Owner))]
    public int OwnerId { get; set; }

    [Required]
    public Payer Owner { get; set; } = default!;

    [ForeignKey(nameof(Contract))]
    public int ContractId { get; set; }

    [Required]
    public Contract Contract { get; set; } = default!;

    [Required]
    public DateTime CreationDate { get; set; }

    [Required]
    [Precision(18, 2)]
    public decimal Price { get; set; } = default;

    [Required]
    [MaxLength(200)]
    public string Label { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public bool Paid { get; set; } = false;
}
