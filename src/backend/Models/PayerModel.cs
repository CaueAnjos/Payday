using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaydayBackend.Models;

[Table("Payers", Schema = "Contract")]
public class Payer
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = default!;

    [Required]
    [EmailAddress]
    [MaxLength(254)]
    [Column(TypeName = "citext")]
    public string Email { get; set; } = default!;

    public List<Payment> Payments { get; set; } = [];
}
