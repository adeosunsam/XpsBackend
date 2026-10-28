namespace Xpress.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public string ConnectionString { get; set; }
    public string? ReadConnectionString { get; set; }
    public string? MigrationsAssembly { get; set; }
}
