using Microsoft.EntityFrameworkCore;
using Xpress.Application.Interfaces;
using Xpress.Domain.Entities;

namespace Xpress.Infrastructure.Persistence;

public sealed class ReadApplicationDbContext
    : BaseReadDbContext, IReadApplicationDbContext
{
    public ReadApplicationDbContext(DbContextOptions<ReadApplicationDbContext> options)
        : base(options) { }

    public DbSet<Transaction> Transactions {  get; set; }
    public DbSet<Customer> Customers {  get; set; }
    public DbSet<Loan> Loans {  get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
