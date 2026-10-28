using Microsoft.EntityFrameworkCore;

namespace Xpress.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext
{
    protected BaseDbContext(DbContextOptions options) : base(options) { }
}
