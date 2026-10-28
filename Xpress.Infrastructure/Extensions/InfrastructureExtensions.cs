using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xpress.Application.Interfaces;
using Xpress.Infrastructure.BackgroundJobs;
using Xpress.Infrastructure.ExternalServices;
using Xpress.Infrastructure.Identity;
using Xpress.Infrastructure.Logger;
using Xpress.Infrastructure.Options;
using Xpress.Infrastructure.Persistence;
using Xpress.Infrastructure.Services;

namespace Xpress.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var dbOptions = configuration
            .GetSection("DatabaseOptions")
            .Get<DatabaseOptions>()
            ?? throw new InvalidOperationException("DatabaseOptions section is missing.");

        services
            .AddXpressDbContext(dbOptions)
            .AddXpressReadDbContext(dbOptions);

        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));

        services
            .AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequireDigit = true;
                o.Password.RequiredLength = 8;
                o.Password.RequireUppercase = true;
                o.Password.RequireNonAlphanumeric = true;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddTransient(typeof(ILog<>), typeof(LogService<>));

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<TransactionReportJob>();

        services.AddHangfire(cfg => cfg
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(dbOptions.ConnectionString));
        services.AddHangfireServer();

        services.Configure<SavingsServiceOptions>(configuration.GetSection("SavingsServiceOptions"));
        services.AddTransient<HmacSigningHandler>();

        var savingsOpts = configuration.GetSection("SavingsServiceOptions").Get<SavingsServiceOptions>()
            ?? new SavingsServiceOptions();

        services.AddHttpClient<ISavingsServiceClient, SavingsServiceClient>(client =>
        {
            client.BaseAddress = new Uri(savingsOpts.BaseUrl ?? "https://localhost:7267");
            client.DefaultRequestHeaders.Add("X-Api-Key", savingsOpts.ApiKey);
            client.DefaultRequestHeaders.Add("X-Service-Name", savingsOpts.ServiceName);
            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddHttpMessageHandler<HmacSigningHandler>();

        return services;
    }
}
