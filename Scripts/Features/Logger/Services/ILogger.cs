namespace GameFoundation.Scripts.Features.Logger.Services
{
    using System.Runtime.CompilerServices;

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    public interface ILogger
    {
        void Log(
            string message,
            LogLevel level = LogLevel.Info,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);

        void Debug(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);

        void Info(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);

        void Warning(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);

        void Error(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0);

        void LogFormat(LogLevel level, string format, params object[] args);
        void SetMinLogLevel(LogLevel level);
        void SetEnabled(bool enabled);
        bool IsEnabled { get; }
        LogLevel MinLogLevel { get; }
    }
}
