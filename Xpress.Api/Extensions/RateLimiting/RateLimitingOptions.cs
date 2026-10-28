namespace Xpress.Api.Extensions.RateLimiting;

public sealed class RateLimitingOptions
{
    public bool Enabled { get; set; }

    public FixedWindowPolicyOptions User { get; set; }
    public FixedWindowPolicyOptions Ip { get; set; }
    public FixedWindowPolicyOptions Auth { get; set; }
}
