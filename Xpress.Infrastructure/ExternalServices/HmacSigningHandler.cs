using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Xpress.Infrastructure.Options;

namespace Xpress.Infrastructure.ExternalServices;

public sealed class HmacSigningHandler : DelegatingHandler
{
    private readonly SavingsServiceOptions _opts;

    public HmacSigningHandler(IOptions<SavingsServiceOptions> opts) => _opts = opts.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var body = string.Empty;
        if (request.Content is not null)
        {
            body = await request.Content.ReadAsStringAsync(cancellationToken);

            request.Content = new StringContent(
                body,
                Encoding.UTF8,
                request.Content.Headers.ContentType?.MediaType ?? "application/json");
        }

        var path = request.RequestUri?.PathAndQuery ?? "/";
        var payload = $"{request.Method.Method}\n{path}\n{timestamp}\n{body}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_opts.ApiKey));
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var signature = Convert.ToHexString(hashBytes).ToLowerInvariant();

        request.Headers.Add(_opts.TimestampHeader, timestamp);
        request.Headers.Add(_opts.SignatureHeader, signature);

        return await base.SendAsync(request, cancellationToken);
    }
}
