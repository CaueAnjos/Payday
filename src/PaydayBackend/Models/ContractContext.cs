using Microsoft.EntityFrameworkCore;

namespace PaydayBackend.Models;

public class ContractContext : DbContext
{
    public DbSet<Payer> Payers { get; set; } = default!;
    public DbSet<Payment> Payments { get; set; } = default!;
    public DbSet<Contract> Contracts { get; set; } = default!;

    public ContractContext(DbContextOptions<ContractContext> config)
        : base(config) { }
}
