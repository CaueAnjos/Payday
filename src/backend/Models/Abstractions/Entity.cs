using System.ComponentModel.DataAnnotations;

namespace PaydayBackend.Models.Abstractions;

public abstract class Entity
{
    [Key]
    public int Id { get; set; }
};
