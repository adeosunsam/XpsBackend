using Microsoft.EntityFrameworkCore;
using Xpress.Domain.Entities;

namespace Xpress.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Transaction> Transactions { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Loan> Loans { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
