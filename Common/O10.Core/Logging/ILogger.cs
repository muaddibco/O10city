using System;
using O10.Core.Architecture;

namespace O10.Core.Logging
{
    [ExtensionPoint]
    public interface ILogger
    {
        void Initialize(string scopeName, string logConfigurationFile = null, LogLevel logLevel = LogLevel.Info);

        void Error(string msg, params object[] messageArgs);
        void Error(string msg, Exception ex, params object[] messageArgs);
        void Warning(string msg, params object[] messageArgs);
        void Info(string msg, params object[] messageArgs);
        void Debug(string msg, params object[] messageArgs);
        void Debug(Func<string> getMsg);
        bool IsWarnEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsDebugEnabled { get; }
        bool IsErrorEnabled { get; }
        void LogIfDebug(Func<string> messageFactory, params object[] argsToJson);
        void LogIfInfo(Func<string> messageFactory);
    }
}
