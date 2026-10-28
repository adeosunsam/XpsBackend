using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Xpress.Savings.Data;

public sealed class SavingsDbContextFactory : IDesignTimeDbContextFactory<SavingsDbContext>
{
    public SavingsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<SavingsDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=XpressSavingsDb;Trusted_Connection=True;MultipleActiveResultSets=true")
            .Options;

        return new SavingsDbContext(options);
    }
}
