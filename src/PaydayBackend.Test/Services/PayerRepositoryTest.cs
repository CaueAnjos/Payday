using PaydayBackend.Models;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Test.Services;

public class PayersRepositoryReaderTest : RepositoryReaderTest<PayersRepository, Payer>
{
    public PayersRepositoryReaderTest()
        : base((ContractContext context) => new PayersRepository(context)) { }

    protected override Payer CreateFakeEntity(int id)
    {
        return new Payer
        {
            Id = id,
            Email = "test@test.com",
            Name = "test",
        };
    }
}
