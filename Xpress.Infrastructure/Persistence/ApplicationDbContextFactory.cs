using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Xpress.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=XpressBackendDb;Trusted_Connection=True;MultipleActiveResultSets=true",
                sql => sql.MigrationsAssembly("Xpress.Infrastructure"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
