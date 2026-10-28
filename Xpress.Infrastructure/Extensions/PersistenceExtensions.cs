using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xpress.Application.Interfaces;
using Xpress.Infrastructure.Persistence;

namespace Xpress.Infrastructure.Extensions;

public static class PersistenceExtensions
{
    public static IServiceCollection AddXpressDbContext(
        this IServiceCollection services,
        DatabaseOptions options)
    {
        services.AddDbContext<ApplicationDbContext>(db =>
            db.UseSqlServer(
                options.ConnectionString,
                sql => sql.MigrationsAssembly(
                    options.MigrationsAssembly
                    ?? typeof(ApplicationDbContext).Assembly.GetName().Name)));

        services.AddScoped<IApplicationDbContext>(
            sp => sp.GetRequiredService<ApplicationDbContext>());

        return services;
    }

    public static IServiceCollection AddXpressReadDbContext(
        this IServiceCollection services,
        DatabaseOptions options)
    {
        var connectionString = options.ReadConnectionString ?? options.ConnectionString;

        services.AddDbContext<ReadApplicationDbContext>(db =>
            db.UseSqlServer(connectionString)
              .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddScoped<IReadApplicationDbContext>(
            sp => sp.GetRequiredService<ReadApplicationDbContext>());

        return services;
    }
}
