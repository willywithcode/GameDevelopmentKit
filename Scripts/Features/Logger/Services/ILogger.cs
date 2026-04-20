namespace GameFoundation.Scripts.Features.Logger.Services
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public interface ILogger
    {
        void Log(string message, LogLevel level = LogLevel.Info);
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void LogFormat(LogLevel level, string format, params object[] args);
        void SetMinLogLevel(LogLevel level);
        void SetEnabled(bool enabled);
        bool IsEnabled { get; }
        LogLevel MinLogLevel { get; }
    }
}
