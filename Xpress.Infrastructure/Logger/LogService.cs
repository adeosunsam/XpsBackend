using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Xpress.Infrastructure.Logger;

public sealed class LogService<T> : ILog<T>
{
    private readonly ILogger<T> _logger;
    private readonly string _className;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    public LogService(ILogger<T> logger)
    {
        _logger = logger;
        _className = typeof(T).Name;
    }

    public void Log(string message, LogLevel logType = LogLevel.Information, [CallerMemberName] string methodName = "")
    {
        _logger.Log(logType, "[{ClassName}.{MethodName}] {Message}", _className, methodName, message);
    }

    public void Log(object data, string message = "", LogLevel logType = LogLevel.Information, [CallerMemberName] string methodName = "")
    {
        var serialized = JsonSerializer.Serialize(data, SerializerOptions);
        _logger.Log(logType, "[{ClassName}.{MethodName}] {Message} {Data}", _className, methodName, message, serialized);
    }

    public void Log(string message, LogLevel logType = LogLevel.Information, [CallerMemberName] string methodName = "", params object[] args)
    {
        _logger.Log(logType, "[{ClassName}.{MethodName}] {Message}", _className, methodName, string.Format(message, args));
    }

    public void Log(Exception ex, LogLevel logType = LogLevel.Error, [CallerMemberName] string methodName = "")
    {
        _logger.Log(logType, ex,
            "[{ClassName}.{MethodName}] Exception: {ExceptionMessage} | InnerException: {InnerExceptionMessage}",
            _className, methodName, ex?.Message, ex?.InnerException?.Message);
    }
}
