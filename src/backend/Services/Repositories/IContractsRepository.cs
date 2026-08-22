using PaydayBackend.Models;

namespace PaydayBackend.Services.Repositories;

public interface IContractsRepository : IRepository<Contract>
{
    public Task AddPayersAsync(
        int id,
        IReadOnlyList<int> payerIds,
        CancellationToken cancel = default
    );

    public Task AddCloseSignature(int id, int participantId, CancellationToken cancel = default);
}
