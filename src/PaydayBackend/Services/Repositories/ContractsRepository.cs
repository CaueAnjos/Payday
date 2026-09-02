using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models;

namespace PaydayBackend.Services.Repositories;

public class ContractsRepository(ContractContext context, ILogger<ContractsRepository> logger)
    : RepositoryBase<Contract>(context),
        IContractsRepository
{
    protected override IQueryable<Contract> AddIncludes(IQueryable<Contract> query)
    {
        return base.AddIncludes(query).Include(c => c.Participants);
    }

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
        var contract = await GetByIdAsync(id);
        if (contract is null)
        {
            logger.LogError("Contract not found");
            throw new NullReferenceException("Contract not found");
        }

        var participantsIds = contract.Participants.Select(p => p.Id).ToHashSet();
        var newParticipantsIds = payerIds.Where(id => !participantsIds.Contains(id)).ToHashSet();

        var payers = context.Payers.Where(p => newParticipantsIds.Contains(p.Id)).ToList();

        if (payers is null || !payers.Any())
        {
            logger.LogInformation("There is no new participants to add");
            return;
        }

        contract.Participants.AddRange(payers);

        await context.SaveChangesAsync();
    }

    public async Task RemovePayerAsync(int id, int payerId, CancellationToken cancel = default)
    {
        var contract = await GetByIdAsync(id);

        var participant = contract?.Participants.FirstOrDefault(p => p.Id == payerId);
        if (participant is null)
            throw new NullReferenceException("Participant not found");

        contract?.Participants.Remove(participant);
        await context.SaveChangesAsync();
    }

    public async Task AddCloseSignature(
        int id,
        int participantId,
        CancellationToken cancel = default
    )
    {
        var contract = await GetByIdAsync(id);
        if (contract is null)
        {
            throw new Exception("Contract with id " + id + " not found");
        }

        var payer = contract.Participants.Find(p => p.Id == participantId);
        if (payer is null)
        {
            throw new Exception("Participant with id " + id + " not found");
        }

        var signature = new Signature
        {
            OwnerId = participantId,
            ContractId = id,
            CreationDate = DateTime.UtcNow,
        };

        contract?.CloseSignatures.Add(signature);

        await context.SaveChangesAsync();
    }
}
