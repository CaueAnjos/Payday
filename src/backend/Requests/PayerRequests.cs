using PaydayBackend.Models;

namespace PaydayBackend.Requests;

public record CreatePayerRequest(string Name, string Email)
{
    public static explicit operator Payer(CreatePayerRequest request)
    {
        return new Payer { Name = request.Name, Email = request.Email };
    }
};

public record UpdatePayerRequest(string Name, string Email)
{
    public static explicit operator Payer(UpdatePayerRequest request)
    {
        return new Payer { Name = request.Name, Email = request.Email };
    }
}
