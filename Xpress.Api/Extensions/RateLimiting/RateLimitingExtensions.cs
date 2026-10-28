using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Xpress.Api.Extensions.RateLimiting;

public static class RateLimitingExtensions
{
    private static readonly string[] HealthPaths =
        ["/health", "/ready", "/live"];

    public static IServiceCollection AddXpressRateLimiting(this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection("RateLimitingOptions")
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        if (!options.Enabled)
            return services;

        services.AddRateLimiter(rl =>
        {
            rl.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                BuildUserLimiter(options.User),
                BuildIpLimiter(options.Ip));

            rl.OnRejected = async (ctx, token) =>
            {
                ctx.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                ctx.HttpContext.Response.ContentType = "application/problem+json";

                if (ctx.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    ctx.HttpContext.Response.Headers.RetryAfter =
                        ((int)retryAfter.TotalSeconds).ToString();

                await ctx.HttpContext.Response.WriteAsJsonAsync(new
                {
                    type = "https://tools.ietf.org/html/rfc6585#section-4",
                    title = "Too Many Requests",
                    status = 429,
                    detail = "Rate limit exceeded. Please slow down and retry after the indicated interval.",
                    traceId = ctx.HttpContext.TraceIdentifier,
                }, token);
            };
        });

        return services;
    }

    public static IApplicationBuilder UseXpressRateLimiting(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices
            .GetService<IConfiguration>()
            ?.GetSection("RateLimitingOptions")
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        if (options.Enabled)
            app.UseRateLimiter();

        return app;
    }

    private static PartitionedRateLimiter<HttpContext> BuildUserLimiter(FixedWindowPolicyOptions opts)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        {
            if (IsHealthPath(ctx))
                return RateLimitPartition.GetNoLimiter("health");

            var userId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RateLimitPartition.GetNoLimiter("anonymous");

            return RateLimitPartition.GetFixedWindowLimiter(
                $"user:{userId}", _ => ToFixedWindow(opts));
        });
    }

    private static PartitionedRateLimiter<HttpContext> BuildIpLimiter(
        FixedWindowPolicyOptions opts)
    {
        return PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        {
            if (IsHealthPath(ctx))
                return RateLimitPartition.GetNoLimiter("health");

            var ip = ctx.Connection.RemoteIpAddress;
            if (ip is null)
                return RateLimitPartition.GetNoLimiter("ip:no-ip");

            return RateLimitPartition.GetFixedWindowLimiter(
                $"ip:{ip}", _ => ToFixedWindow(opts));
        });
    }


    private static FixedWindowRateLimiterOptions ToFixedWindow(FixedWindowPolicyOptions o) =>
        new()
        {
            PermitLimit = o.PermitLimit,
            Window = TimeSpan.FromSeconds(o.WindowSeconds),
            QueueLimit = o.QueueLimit,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            AutoReplenishment = true,
        };

    private static bool IsHealthPath(HttpContext ctx) =>
        HealthPaths.Contains(ctx.Request.Path.Value, StringComparer.OrdinalIgnoreCase);
}
