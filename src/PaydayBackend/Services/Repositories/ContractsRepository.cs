using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PaydayBackend.Exceptions;
using PaydayBackend.Models;

namespace PaydayBackend.Services.Repositories;

public class ContractsRepository(ContractContext context, ILogger<ContractsRepository> logger)
    : RepositoryBase<Contract>(context),
        IContractsRepository
{
    protected override IQueryable<Contract> AddIncludes(IQueryable<Contract> query)
    {
        return base
            .AddIncludes(query)
            .Include(c => c.Participants)
            .Include(c => c.CloseSignatures)
            .Include(c => c.Payments);
    }

    // Reads go through `_query`/`AddIncludes`, which is `AsNoTracking` (see
    // RepositoryBase). Mutating a detached entity's navigation collections and then
    // calling `SaveChangesAsync` is a silent no-op, since the change tracker never
    // sees the detached instance. Methods that need to mutate and persist a Contract
    // must go through this tracked query instead.
    private IQueryable<Contract> TrackedQuery =>
        context
            .Contracts.Include(c => c.Participants)
            .Include(c => c.CloseSignatures)
            .Include(c => c.Payments);

    public override async Task CreateAsync(Contract contract, CancellationToken cancel = default)
    {
        if (contract.CreationDate > contract.PaymentDate)
        {
            throw new ValidationException("PaymentDate cannot be earlier than CreationDate");
        }

        if (
            contract.CloseDate is not null
            || contract.CloseSignatures.Any()
            || contract.State != ContractState.Open
        )
        {
            throw new ValidationException("Newly created contracts cannot be already closed");
        }

        await base.CreateAsync(contract, cancel);
    }

    public async Task AddPayersAsync(
        int id,
        IReadOnlyList<int> payerIds,
        CancellationToken cancel = default
    )
    {
        var contract = await TrackedQuery.FirstOrDefaultAsync(c => c.Id == id, cancel);
        if (contract is null)
        {
            logger.LogError("Contract not found");
            throw new EntityNotFoundException(typeof(Contract), id);
        }

        var participantsIds = contract.Participants.Select(p => p.Id).ToHashSet();
        var newParticipantsIds = payerIds.Where(id => !participantsIds.Contains(id)).ToHashSet();

        var payers = await context
            .Payers.Where(p => newParticipantsIds.Contains(p.Id))
            .ToListAsync(cancel);

        if (payers.Count == 0)
        {
            logger.LogInformation("There is no new participants to add");
            return;
        }

        contract.Participants.AddRange(payers);

        await context.SaveChangesAsync(cancel);
    }

    public async Task RemovePayerAsync(int id, int payerId, CancellationToken cancel = default)
    {
        var contract = await TrackedQuery.FirstOrDefaultAsync(c => c.Id == id, cancel);
        if (contract is null)
            throw new EntityNotFoundException(typeof(Contract), id);

        var participant = contract.Participants.FirstOrDefault(p => p.Id == payerId);
        if (participant is null)
            throw new EntityNotFoundException(
                $"Payer #{payerId} is not a participant of contract #{id}"
            );

        contract.Participants.Remove(participant);
        await context.SaveChangesAsync(cancel);
    }

    public async Task AddCloseSignature(
        int id,
        int participantId,
        CancellationToken cancel = default
    )
    {
        var contract = await TrackedQuery.FirstOrDefaultAsync(c => c.Id == id, cancel);
        if (contract is null)
            throw new EntityNotFoundException(typeof(Contract), id);

        var payer = contract.Participants.Find(p => p.Id == participantId);
        if (payer is null)
            throw new EntityNotFoundException(
                $"Payer #{participantId} is not a participant of contract #{id}"
            );

        ContractStateMachine.EnsureCanSign(contract);

        if (contract.CloseSignatures.Any(s => s.OwnerId == participantId))
            throw new DuplicateEntityException(
                $"Payer #{participantId} already signed the closure of contract #{id}"
            );

        var signature = new Signature
        {
            OwnerId = participantId,
            ContractId = id,
            CreationDate = DateTime.UtcNow,
        };

        contract.CloseSignatures.Add(signature);

        ContractStateMachine.TryClose(contract);

        await context.SaveChangesAsync(cancel);
    }
}
