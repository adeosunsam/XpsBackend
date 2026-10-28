using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Xpress.Api.Extensions.Auth;
using Xpress.Api.Extensions.RateLimiting;
using Xpress.Api.Middleware;
using Xpress.Application.Validators;
using Xpress.Infrastructure.Extensions;
using Xpress.Infrastructure.Identity;
using Xpress.Infrastructure.Persistence;

namespace Xpress.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddXpressRateLimiting(builder.Configuration);
        builder.Services.AddHealthChecks();
        builder.Services.AddControllers();
        builder.Services.AddFluentValidationAutoValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        builder.Services.AddXpressSwagger();

        var app = builder.Build();

        await SeedRolesAsync(app);
        await SeedDataAsync(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
        {
            var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;

            var (status, title) = ex switch
            {
                ArgumentException => (400, "Bad Request"),
                InvalidOperationException e when e.Message.Contains("already registered") => (409, "Conflict"),
                InvalidOperationException e when e.Message.Contains("service") => (502, "Bad Gateway"),
                InvalidOperationException => (400, "Bad Request"),
                KeyNotFoundException => (404, "Not Found"),
                UnauthorizedAccessException => (401, "Unauthorized"),
                TimeoutException => (504, "Gateway Timeout"),
                _ => (500, "Internal Server Error"),
            };

            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json";
            await ctx.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7807",
                title,
                status,
                detail = ex?.Message,
                traceId = ctx.TraceIdentifier,
            });
        }));

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
        });

        app.UseHttpsRedirection();
        app.UseXpressRateLimiting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            Authorization = [new HangfireAdminAuthFilter(app.Environment)],
        });

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }

    private static async Task SeedRolesAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await RoleSeeder.SeedAsync(roleManager);
    }

    private static async Task SeedDataAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DataSeeder.SeedAsync(userManager, context);
    }
}
