using Microsoft.EntityFrameworkCore;
using Xpress.Savings.Domain;

namespace Xpress.Savings.Data;

public sealed class SavingsDbContext : DbContext
{
    public SavingsDbContext(DbContextOptions<SavingsDbContext> options) : base(options) { }

    public DbSet<SavingsAccount> SavingsAccounts { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(SavingsDbContext).Assembly);
    }
}
