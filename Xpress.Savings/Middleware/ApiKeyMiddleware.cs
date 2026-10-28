using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Xpress.Savings.Options;

namespace Xpress.Savings.Middleware;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiKeyOptions _options;

    public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // API Key
        if (!context.Request.Headers.TryGetValue(_options.HeaderName, out var extractedKey))
        {
            await RejectAsync(context, 401, $"'{_options.HeaderName}' header is missing.");
            return;
        }

        if (!string.Equals(extractedKey, _options.Key, StringComparison.Ordinal))
        {
            await RejectAsync(context, 403, "Invalid API Key.");
            return;
        }

        //Service whitelist
        if (_options.AllowedServices.Count > 0)
        {
            if (!context.Request.Headers.TryGetValue(_options.ServiceNameHeader, out var serviceName) ||
                string.IsNullOrWhiteSpace(serviceName))
            {
                await RejectAsync(context, 403,
                    $"'{_options.ServiceNameHeader}' header is missing. Service must identify itself.");
                return;
            }

            if (!_options.AllowedServices.Contains(serviceName.ToString(), StringComparer.OrdinalIgnoreCase))
            {
                await RejectAsync(context, 403,
                    $"Service '{serviceName}' is not whitelisted. Contact the platform team to request access.");
                return;
            }
        }

        //Timestamp
        if (!context.Request.Headers.TryGetValue(_options.TimestampHeader, out var timestampRaw) ||
            !long.TryParse(timestampRaw, out var timestamp))
        {
            await RejectAsync(context, 403,
                $"'{_options.TimestampHeader}' header is missing or invalid.");
            return;
        }

        var ageSeconds = Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp);
        if (ageSeconds > _options.MaxRequestAgeSeconds)
        {
            await RejectAsync(context, 403,
                $"Request is too old ({ageSeconds}s). Maximum allowed age is {_options.MaxRequestAgeSeconds}s. Possible replay attack.");
            return;
        }

        //signature
        if (!context.Request.Headers.TryGetValue(_options.SignatureHeader, out var providedSignature))
        {
            await RejectAsync(context, 403, $"'{_options.SignatureHeader}' header is missing.");
            return;
        }

        context.Request.EnableBuffering();

        string body;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, leaveOpen: true))
            body = await reader.ReadToEndAsync();

        context.Request.Body.Position = 0;

        var fullPath = context.Request.Path + context.Request.QueryString;
        var payload = $"{context.Request.Method}\n{fullPath}\n{timestampRaw}\n{body}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.Key));
        var expectedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var expectedSignature = Convert.ToHexString(expectedHash).ToLowerInvariant();

        var providedBytes = Encoding.UTF8.GetBytes(providedSignature.ToString());
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSignature);

        if (providedBytes.Length != expectedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes))
        {
            await RejectAsync(context, 403, "Invalid request signature.");
            return;
        }

        await _next(context);
    }

    private static Task RejectAsync(HttpContext context, int status, string detail)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7807",
            title = status == 401 ? "Unauthorized" : "Forbidden",
            status,
            detail,
        });
    }
}
