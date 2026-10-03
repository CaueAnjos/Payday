using Microsoft.EntityFrameworkCore;
using PaydayBackend.Exceptions;
using PaydayBackend.Models;

namespace PaydayBackend.Services.Repositories;

public class PaymentsRepository(ContractContext context)
    : RepositoryBase<Payment>(context),
        IPaymentsRepository
{
    protected override IQueryable<Payment> AddIncludes(IQueryable<Payment> query)
    {
        return base.AddIncludes(query).Include(p => p.Signature);
    }

    public async Task<IReadOnlyList<Payment>> GetByPayerIdAsync(
        int payerId,
        CancellationToken cancel = default
    )
    {
        if (payerId < 0)
            throw new ArgumentOutOfRangeException("Only positive `payerId` are allowed");

        return await _query
            .Where(p => p.OwnerId == payerId)
            .OrderBy(p => p.Id)
            .ToListAsync(cancel);
    }

    public async Task<IReadOnlyList<Payment>> GetByContractIdAsync(
        int contractId,
        CancellationToken cancel = default
    )
    {
        if (contractId < 0)
            throw new ArgumentOutOfRangeException("Only positive `contractId` are allowed");

        return await _query
            .Where(p => p.ContractId == contractId)
            .OrderBy(p => p.Id)
            .ToListAsync(cancel);
    }

    public async Task<IReadOnlyList<Payment>> GetByContractAndParticipantIdAsync(
        int contractId,
        int participantId,
        CancellationToken cancel = default
    )
    {
        if (contractId < 0)
            throw new ArgumentOutOfRangeException("Only positive `contractId` are allowed");

        if (participantId < 0)
            throw new ArgumentOutOfRangeException("Only positive `participantId` are allowed");

        return await _query
            .Where(p => p.ContractId == contractId && p.OwnerId == participantId)
            .OrderBy(p => p.Id)
            .ToListAsync(cancel);
    }

    public async Task<IReadOnlyList<Payment>> CreatePayerPaymentsAsync(
        int payerId,
        IReadOnlyList<Payment> payments,
        CancellationToken cancel = default
    )
    {
        var payerExists = await context.Payers.AnyAsync(p => p.Id == payerId, cancel);
        if (!payerExists)
            throw new EntityNotFoundException(typeof(Payer), payerId);

        if (payments.Count == 0)
            return payments;

        await context.Payments.AddRangeAsync(payments, cancel);
        await context.SaveChangesAsync(cancel);

        return payments;
    }

    public async Task<IReadOnlyList<Payment>> CreateContractParticipantPaymentsAsync(
        int contractId,
        int participantId,
        IReadOnlyList<Payment> payments,
        CancellationToken cancel = default
    )
    {
        var contract = await context
            .Contracts.Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancel);

        if (contract is null)
            throw new EntityNotFoundException(typeof(Contract), contractId);

        if (!contract.Participants.Any(p => p.Id == participantId))
            throw new EntityNotFoundException(
                $"Payer #{participantId} is not a participant of contract #{contractId}"
            );

        // Mutates contract.State in place if the Open -> AwaitingClosure transition
        // is due; `contract` is tracked, so that change is persisted below alongside
        // the new payments.
        ContractStateMachine.EnsureCanAddPayment(contract);

        if (payments.Count > 0)
            await context.Payments.AddRangeAsync(payments, cancel);

        await context.SaveChangesAsync(cancel);

        return payments;
    }

    public override async Task DeleteAsync(int id, CancellationToken cancel = default)
    {
        if (id < 0)
            throw new ArgumentOutOfRangeException("Only positive `id` are allowed");

        var payment = await context
            .Payments.Include(p => p.Signature)
            .FirstOrDefaultAsync(p => p.Id == id, cancel);

        if (payment is null)
            throw new EntityNotFoundException(typeof(Payment), id);

        if (payment.Signature is not null)
            throw new ImmutableEntityException(
                $"Payment #{id} has already been signed and cannot be deleted."
            );

        context.Payments.Remove(payment);
        await context.SaveChangesAsync(cancel);
    }

    public override Task<bool> CreateOrReplaceAsync(
        Payment entity,
        CancellationToken cancel = default
    )
    {
        throw new ImmutableEntityException(
            "Payments are immutable; use CreateAsync (or the batch create endpoints) instead of CreateOrReplaceAsync."
        );
    }

    public override Task UpdateAsync(int id, object patch, CancellationToken cancel = default)
    {
        throw new ImmutableEntityException("Payments are immutable and cannot be updated.");
    }
}
