using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Xpress.Savings.Data;
using Xpress.Savings.Middleware;
using Xpress.Savings.Options;
using Xpress.Savings.Services;

namespace Xpress.Savings;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<ApiKeyOptions>(builder.Configuration.GetSection("ApiKeyOptions"));

        builder.Services.AddDbContext<SavingsDbContext>(opt =>
            opt.UseSqlServer(
                builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection is missing.")));

        builder.Services.AddScoped<ISavingsAccountService, SavingsAccountService>();

        builder.Services.AddValidatorsFromAssemblyContaining<Program>();
        builder.Services.AddFluentValidationAutoValidation();

        builder.Services.AddControllers();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHealthChecks();

        var app = builder.Build();

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
                InvalidOperationException => (409, "Conflict"),
                KeyNotFoundException => (404, "Not Found"),
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

        app.UseMiddleware<ApiKeyMiddleware>();
        app.UseHttpsRedirection();
        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
