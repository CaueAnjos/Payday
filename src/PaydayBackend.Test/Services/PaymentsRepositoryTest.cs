using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaydayBackend.Exceptions;
using PaydayBackend.Models;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Test.Services;

public class PaymentsRepositoryTest : IDisposable
{
    private readonly ContractContext _context;
    private readonly PaymentsRepository _repository;

    public PaymentsRepositoryTest()
    {
        var options = new DbContextOptionsBuilder<ContractContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContractContext(options);
        _repository = new PaymentsRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private Payer AddPayer(int id)
    {
        var payer = new Payer
        {
            Id = id,
            Name = $"Payer {id}",
            Email = $"payer{id}@test.com",
        };
        _context.Payers.Add(payer);
        return payer;
    }

    private Contract AddContract(
        int id,
        ContractState state,
        DateTime paymentDate,
        params Payer[] participants
    )
    {
        var contract = new Contract
        {
            Id = id,
            Label = "Contract",
            CreationDate = DateTime.UtcNow.AddDays(-1),
            PaymentDate = paymentDate,
            State = state,
            Participants = [.. participants],
        };
        _context.Contracts.Add(contract);
        return contract;
    }

    private static Payment MakePayment(int ownerId, int? contractId = null, int? id = null)
    {
        return new Payment
        {
            Id = id ?? default,
            OwnerId = ownerId,
            ContractId = contractId,
            Label = "Payment",
            Price = 10,
            CreationDate = DateTime.UtcNow,
        };
    }

    [Fact]
    public async Task GetByPayerIdAsync_ReturnsOnlyThatPayersPayments()
    {
        AddPayer(1);
        AddPayer(2);
        _context.Payments.Add(MakePayment(ownerId: 1, id: 1));
        _context.Payments.Add(MakePayment(ownerId: 2, id: 2));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _repository.GetByPayerIdAsync(1, TestContext.Current.CancellationToken);

        result.Should().ContainSingle(p => p.Id == 1);
    }

    [Fact]
    public async Task GetByContractIdAsync_ReturnsOnlyThatContractsPayments()
    {
        var payer = AddPayer(1);
        AddContract(1, ContractState.Open, DateTime.UtcNow.AddDays(1), payer);
        AddContract(2, ContractState.Open, DateTime.UtcNow.AddDays(1), payer);
        _context.Payments.Add(MakePayment(ownerId: 1, contractId: 1, id: 1));
        _context.Payments.Add(MakePayment(ownerId: 1, contractId: 2, id: 2));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _repository.GetByContractIdAsync(
            1,
            TestContext.Current.CancellationToken
        );

        result.Should().ContainSingle(p => p.Id == 1);
    }

    [Fact]
    public async Task GetByContractAndParticipantIdAsync_FiltersByBoth()
    {
        var payer1 = AddPayer(1);
        var payer2 = AddPayer(2);
        AddContract(1, ContractState.Open, DateTime.UtcNow.AddDays(1), payer1, payer2);
        _context.Payments.Add(MakePayment(ownerId: 1, contractId: 1, id: 1));
        _context.Payments.Add(MakePayment(ownerId: 2, contractId: 1, id: 2));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _repository.GetByContractAndParticipantIdAsync(
            1,
            1,
            TestContext.Current.CancellationToken
        );

        result.Should().ContainSingle(p => p.Id == 1);
    }

    [Fact]
    public async Task CreatePayerPaymentsAsync_PayerDoesNotExist_ThrowsEntityNotFoundException()
    {
        var act = async () =>
            await _repository.CreatePayerPaymentsAsync(
                999,
                [MakePayment(ownerId: 999)],
                TestContext.Current.CancellationToken
            );

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreatePayerPaymentsAsync_PayerExists_PersistsBatch()
    {
        AddPayer(1);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var created = await _repository.CreatePayerPaymentsAsync(
            1,
            [MakePayment(ownerId: 1), MakePayment(ownerId: 1)],
            TestContext.Current.CancellationToken
        );

        created.Should().HaveCount(2);
        _context.Payments.Count(p => p.OwnerId == 1).Should().Be(2);
    }

    [Fact]
    public async Task CreateContractParticipantPaymentsAsync_ContractDoesNotExist_ThrowsEntityNotFoundException()
    {
        var act = async () =>
            await _repository.CreateContractParticipantPaymentsAsync(
                999,
                1,
                [MakePayment(ownerId: 1, contractId: 999)],
                TestContext.Current.CancellationToken
            );

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateContractParticipantPaymentsAsync_ParticipantNotInContract_ThrowsEntityNotFoundException()
    {
        AddContract(1, ContractState.Open, DateTime.UtcNow.AddDays(1));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var act = async () =>
            await _repository.CreateContractParticipantPaymentsAsync(
                1,
                1,
                [MakePayment(ownerId: 1, contractId: 1)],
                TestContext.Current.CancellationToken
            );

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateContractParticipantPaymentsAsync_ContractNotOpen_ThrowsInvalidContractStateException()
    {
        var payer = AddPayer(1);
        AddContract(1, ContractState.AwaitingClosure, DateTime.UtcNow.AddDays(-1), payer);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var act = async () =>
            await _repository.CreateContractParticipantPaymentsAsync(
                1,
                1,
                [MakePayment(ownerId: 1, contractId: 1)],
                TestContext.Current.CancellationToken
            );

        await act.Should().ThrowAsync<InvalidContractStateException>();
    }

    [Fact]
    public async Task CreateContractParticipantPaymentsAsync_ContractOpen_PersistsBatch()
    {
        var payer = AddPayer(1);
        AddContract(1, ContractState.Open, DateTime.UtcNow.AddDays(1), payer);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var created = await _repository.CreateContractParticipantPaymentsAsync(
            1,
            1,
            [MakePayment(ownerId: 1, contractId: 1), MakePayment(ownerId: 1, contractId: 1)],
            TestContext.Current.CancellationToken
        );

        created.Should().HaveCount(2);
        _context.Payments.Count(p => p.ContractId == 1).Should().Be(2);
    }

    [Fact]
    public async Task DeleteAsync_PaymentIsSigned_ThrowsImmutableEntityException()
    {
        var payer = AddPayer(1);
        var contract = AddContract(1, ContractState.AwaitingClosure, DateTime.UtcNow.AddDays(-1));
        var signature = new Signature
        {
            Id = 1,
            OwnerId = 1,
            Owner = payer,
            ContractId = 1,
            Contract = contract,
            CreationDate = DateTime.UtcNow,
        };
        var payment = new Payment
        {
            Id = 1,
            OwnerId = 1,
            Label = "Payment",
            Price = 10,
            CreationDate = DateTime.UtcNow,
            Signature = signature,
        };
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var act = async () => await _repository.DeleteAsync(1, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ImmutableEntityException>();
    }

    [Fact]
    public async Task DeleteAsync_PaymentNotSigned_RemovesIt()
    {
        AddPayer(1);
        _context.Payments.Add(MakePayment(ownerId: 1, id: 1));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _repository.DeleteAsync(1, TestContext.Current.CancellationToken);

        (await _context.Payments.FindAsync([1], TestContext.Current.CancellationToken)).Should().BeNull();
    }

    [Fact]
    public async Task CreateOrReplaceAsync_ThrowsImmutableEntityException()
    {
        var act = async () =>
            await _repository.CreateOrReplaceAsync(
                MakePayment(ownerId: 1),
                TestContext.Current.CancellationToken
            );

        await act.Should().ThrowAsync<ImmutableEntityException>();
    }

    [Fact]
    public async Task UpdateAsync_ThrowsImmutableEntityException()
    {
        var act = async () =>
            await _repository.UpdateAsync(
                1,
                new { Label = "Y" },
                TestContext.Current.CancellationToken
            );

        await act.Should().ThrowAsync<ImmutableEntityException>();
    }
}
