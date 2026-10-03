using Microsoft.EntityFrameworkCore;
using PaydayBackend.Models;

namespace PaydayBackend.Services.Repositories;

public class PayersRepository(ContractContext context)
    : RepositoryBase<Payer>(context),
        IPayersRepository
{
    protected override IQueryable<Payer> AddIncludes(IQueryable<Payer> query)
    {
        return query
            .Include(p => p.Payments)
            .ThenInclude(payment => payment.Signature)
            .Include(p => p.Contracts)
            .ThenInclude(c => c.Participants)
            .Include(p => p.Contracts)
            .ThenInclude(c => c.Payments)
            .Include(p => p.Contracts)
            .ThenInclude(c => c.CloseSignatures);
    }
}
