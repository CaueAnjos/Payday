using PaydayBackend.Models;

namespace PaydayBackend.Services.Repositories;

public class PayersRespository(ContractContext context)
    : RepositoryBase<Payer>(context),
        IPayersRespository;
