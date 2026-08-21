using PaydayBackend.Models;

namespace PaydayBackend.Services.Repositories;

public class PayersRepository(ContractContext context)
    : RepositoryBase<Payer>(context),
        IPayersRepository;
