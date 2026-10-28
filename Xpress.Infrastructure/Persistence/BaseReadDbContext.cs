using Microsoft.EntityFrameworkCore;

namespace Xpress.Infrastructure.Persistence;

public abstract class BaseReadDbContext : BaseDbContext
{
    protected BaseReadDbContext(DbContextOptions options) : base(options) { }

    public override int SaveChanges() =>
        throw new InvalidOperationException(
            $"This is a read-only context. Use {nameof(ApplicationDbContext)} for writes.");

    public override int SaveChanges(bool acceptAllChangesOnSuccess) =>
        throw new InvalidOperationException(
            $"This is a read-only context. Use {nameof(ApplicationDbContext)} for writes.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            $"This is a read-only context. Use {nameof(ApplicationDbContext)} for writes.");

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException(
            $"This is a read-only context. Use {nameof(ApplicationDbContext)} for writes.");
}
