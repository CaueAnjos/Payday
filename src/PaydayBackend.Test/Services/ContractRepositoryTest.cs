using FakeItEasy;
using Microsoft.Extensions.Logging;
using PaydayBackend.Models;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Test.Services;

public static class ContractsRepositoryTestUtility
{
    public static Contract CreateFakeEntity(int id)
    {
        return new Contract
        {
            Id = id,
            CreationDate = DateTime.Now,
            PaymentDate = DateTime.Now,
            Label = "Generic Contract",
        };
    }

    public static ILogger<ContractsRepository> CreateFakeLogger()
    {
        var logger = A.Fake<ILogger<ContractsRepository>>();
        return logger;
    }
}

public class ContractsRepositoryReaderTest : RepositoryReaderTest<ContractsRepository, Contract>
{
    public ContractsRepositoryReaderTest()
        : base(
            (ContractContext context) =>
                new ContractsRepository(context, ContractsRepositoryTestUtility.CreateFakeLogger())
        ) { }

    protected override Contract CreateFakeEntity(int id)
    {
        return ContractsRepositoryTestUtility.CreateFakeEntity(id);
    }
}

public class ContractsRepositoryWriterTest : RepositoryWriterTest<ContractsRepository, Contract>
{
    public ContractsRepositoryWriterTest()
        : base(
            (ContractContext context) =>
                new ContractsRepository(context, ContractsRepositoryTestUtility.CreateFakeLogger())
        ) { }

    protected override Contract CreateFakeEntity(int id)
    {
        return ContractsRepositoryTestUtility.CreateFakeEntity(id);
    }
}
