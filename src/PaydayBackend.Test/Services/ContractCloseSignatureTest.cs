using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaydayBackend.Exceptions;
using PaydayBackend.Models;
using PaydayBackend.Services.Repositories;

namespace PaydayBackend.Test.Services;

public class ContractCloseSignatureTest : IDisposable
{
    private readonly ContractContext _context;
    private readonly ContractsRepository _repository;

    public ContractCloseSignatureTest()
    {
        var options = new DbContextOptionsBuilder<ContractContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ContractContext(options);
        _repository = new ContractsRepository(
            _context,
            ContractsRepositoryTestUtility.CreateFakeLogger()
        );
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

    [Fact]
    public async Task AddCloseSignature_ContractDoesNotExist_ThrowsEntityNotFoundException()
    {
        var act = async () =>
            await _repository.AddCloseSignature(
                999,
                1,
                TestContext.Current.CancellationToken
            );

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddCloseSignature_PayerIsNotAParticipant_ThrowsEntityNotFoundException()
    {
        var payer = AddPayer(1);
        AddContract(1, ContractState.AwaitingClosure, DateTime.UtcNow.AddDays(-1));
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var act = async () =>
            await _repository.AddCloseSignature(1, 1, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task AddCloseSignature_ContractStillOpen_ThrowsInvalidContractStateException()
    {
        var payer = AddPayer(1);
        AddContract(1, ContractState.Open, DateTime.UtcNow.AddDays(1), payer);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        var act = async () =>
            await _repository.AddCloseSignature(1, 1, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<InvalidContractStateException>();
    }

    [Fact]
    public async Task AddCloseSignature_PastPaymentDateButStillOpen_TransitionsLazilyAndSucceeds()
    {
        // The contract's State is still (stale) Open, but its PaymentDate is already
        // in the past: ContractStateMachine should bring it up to date (to
        // AwaitingClosure) before validating, instead of rejecting the signature.
        var payer = AddPayer(1);
        AddContract(1, ContractState.Open, DateTime.UtcNow.AddSeconds(-1), payer);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _repository.AddCloseSignature(1, 1, TestContext.Current.CancellationToken);

        var contract = await _context.Contracts.FindAsync([1], TestContext.Current.CancellationToken);
        contract.Should().NotBeNull();
        contract!.State.Should().Be(ContractState.Closed); // only participant, now fully signed
    }

    [Fact]
    public async Task AddCloseSignature_ParticipantAlreadySigned_ThrowsDuplicateEntityException()
    {
        var payer1 = AddPayer(1);
        var payer2 = AddPayer(2);
        // Two participants, so the contract doesn't auto-close after the first
        // signature - otherwise the second attempt would (correctly) fail with
        // InvalidContractStateException instead of the duplicate-signature check.
        AddContract(1, ContractState.AwaitingClosure, DateTime.UtcNow.AddDays(-1), payer1, payer2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _repository.AddCloseSignature(1, 1, TestContext.Current.CancellationToken);

        var act = async () =>
            await _repository.AddCloseSignature(1, 1, TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task AddCloseSignature_NotEveryoneSignedYet_ContractStaysAwaitingClosure()
    {
        var payer1 = AddPayer(1);
        var payer2 = AddPayer(2);
        AddContract(1, ContractState.AwaitingClosure, DateTime.UtcNow.AddDays(-1), payer1, payer2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _repository.AddCloseSignature(1, 1, TestContext.Current.CancellationToken);

        var contract = await _context
            .Contracts.Include(c => c.CloseSignatures)
            .FirstAsync(c => c.Id == 1, TestContext.Current.CancellationToken);
        contract.State.Should().Be(ContractState.AwaitingClosure);
        contract.CloseSignatures.Should().ContainSingle(s => s.OwnerId == 1);
    }

    [Fact]
    public async Task AddCloseSignature_EveryoneSigned_ClosesContract()
    {
        var payer1 = AddPayer(1);
        var payer2 = AddPayer(2);
        AddContract(1, ContractState.AwaitingClosure, DateTime.UtcNow.AddDays(-1), payer1, payer2);
        await _context.SaveChangesAsync(TestContext.Current.CancellationToken);

        await _repository.AddCloseSignature(1, 1, TestContext.Current.CancellationToken);
        await _repository.AddCloseSignature(1, 2, TestContext.Current.CancellationToken);

        var contract = await _context.Contracts.FindAsync([1], TestContext.Current.CancellationToken);
        contract.Should().NotBeNull();
        contract!.State.Should().Be(ContractState.Closed);
        contract.CloseDate.Should().NotBeNull();
    }
}
