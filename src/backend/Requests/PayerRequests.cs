using System.ComponentModel.DataAnnotations;
using PaydayBackend.Models;

namespace PaydayBackend.Requests;

public record CreatePayerRequest(
    [Required, MaxLength(255)] string Name,
    [Required, EmailAddress, MaxLength(254)] string Email
)
{
    public static explicit operator Payer(CreatePayerRequest request)
    {
        return new Payer { Name = request.Name, Email = request.Email };
    }
};

public record UpdatePayerRequest(
    [Required, MaxLength(255)] string Name,
    [Required, EmailAddress, MaxLength(254)] string Email
)
{
    public static explicit operator Payer(UpdatePayerRequest request)
    {
        return new Payer { Name = request.Name, Email = request.Email };
    }
}
