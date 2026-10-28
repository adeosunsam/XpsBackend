namespace Xpress.Savings.Options;

public sealed class ApiKeyOptions
{
    public string HeaderName { get; set; }
    public string Key { get; set; }
    public string ServiceNameHeader { get; set; }
    public List<string> AllowedServices { get; set; }
    public string TimestampHeader { get; set; }
    public string SignatureHeader { get; set; }
    public int MaxRequestAgeSeconds { get; set; }
}
