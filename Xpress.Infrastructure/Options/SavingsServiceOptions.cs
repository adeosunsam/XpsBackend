namespace Xpress.Infrastructure.Options;

public sealed class SavingsServiceOptions
{
    public string BaseUrl { get; set; }
    public string ApiKey { get; set; }
    public string ServiceName { get; set; }
    public string TimestampHeader { get; set; }
    public string SignatureHeader { get; set; }
}
