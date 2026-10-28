using Hangfire.Dashboard;

namespace Xpress.Api.Middleware;

public sealed class HangfireAdminAuthFilter : IDashboardAuthorizationFilter
{
    private readonly IWebHostEnvironment _env;

    public HangfireAdminAuthFilter(IWebHostEnvironment env) => _env = env;

    public bool Authorize(DashboardContext context)
    {
        if (_env.IsDevelopment())
            return true;

        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true
            && httpContext.User.IsInRole("Admin");
    }
}
