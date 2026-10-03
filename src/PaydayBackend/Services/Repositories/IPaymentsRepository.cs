using PaydayBackend.Models;

namespace PaydayBackend.Services.Repositories;

public interface IPaymentsRepository : IRepository<Payment>
{
    Task<IReadOnlyList<Payment>> GetByPayerIdAsync(int payerId, CancellationToken cancel = default);

    Task<IReadOnlyList<Payment>> GetByContractIdAsync(
        int contractId,
        CancellationToken cancel = default
    );

    Task<IReadOnlyList<Payment>> GetByContractAndParticipantIdAsync(
        int contractId,
        int participantId,
        CancellationToken cancel = default
    );

    /// <summary>
    /// Creates a batch of payments owned directly by a payer, not tied to any contract.
    /// </summary>
    Task<IReadOnlyList<Payment>> CreatePayerPaymentsAsync(
        int payerId,
        IReadOnlyList<Payment> payments,
        CancellationToken cancel = default
    );

    /// <summary>
    /// Creates a batch of payments for a contract participant. Throws
    /// InvalidContractStateException unless the contract currently accepts new
    /// payments.
    /// </summary>
    Task<IReadOnlyList<Payment>> CreateContractParticipantPaymentsAsync(
        int contractId,
        int participantId,
        IReadOnlyList<Payment> payments,
        CancellationToken cancel = default
    );
}
