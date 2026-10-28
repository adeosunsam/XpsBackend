using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Xpress.Infrastructure.Logger;

public interface ILog<T>
{
    void Log(string message, LogLevel logType = LogLevel.Information, [CallerMemberName] string methodName = "");
    void Log(object data, string message = "", LogLevel logType = LogLevel.Information, [CallerMemberName] string methodName = "");
    void Log(string message, LogLevel logType = LogLevel.Information, [CallerMemberName] string methodName = "", params object[] args);
    void Log(Exception ex, LogLevel logType = LogLevel.Error, [CallerMemberName] string methodName = "");
}
