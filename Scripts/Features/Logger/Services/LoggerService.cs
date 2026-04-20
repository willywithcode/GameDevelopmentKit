namespace GameFoundation.Scripts.Features.Logger.Services
{
    using UnityEngine;
    using Debug = UnityEngine.Debug;

    public class LoggerService : ILogger
    {
        private const string TagFormat = "[{0}] {1}";
        private const string DebugTag = "DEBUG";
        private const string InfoTag = "INFO";
        private const string WarningTag = "WARNING";
        private const string ErrorTag = "ERROR";

        private bool isEnabled = true;
        private LogLevel minLogLevel = LogLevel.Debug;

        public bool IsEnabled => this.isEnabled;
        public LogLevel MinLogLevel => this.minLogLevel;

        public void SetEnabled(bool enabled)
        {
            this.isEnabled = enabled;
        }

        public void SetMinLogLevel(LogLevel level)
        {
            this.minLogLevel = level;
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (!this.isEnabled || level < this.minLogLevel) return;

            switch (level)
            {
                case LogLevel.Debug:
                    this.Debug(message);
                    break;
                case LogLevel.Info:
                    this.Info(message);
                    break;
                case LogLevel.Warning:
                    this.Warning(message);
                    break;
                case LogLevel.Error:
                    this.Error(message);
                    break;
            }
        }

        public void Debug(string message)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!this.isEnabled || this.minLogLevel > LogLevel.Debug) return;
            UnityEngine.Debug.Log(string.Format(TagFormat, DebugTag, message));
#endif
        }

        public void Info(string message)
        {
            if (!this.isEnabled || this.minLogLevel > LogLevel.Info) return;
            UnityEngine.Debug.Log(string.Format(TagFormat, InfoTag, message));
        }

        public void Warning(string message)
        {
            if (!this.isEnabled || this.minLogLevel > LogLevel.Warning) return;
            UnityEngine.Debug.LogWarning(string.Format(TagFormat, WarningTag, message));
        }

        public void Error(string message)
        {
            if (!this.isEnabled || this.minLogLevel > LogLevel.Error) return;
            UnityEngine.Debug.LogError(string.Format(TagFormat, ErrorTag, message));
        }

        public void LogFormat(LogLevel level, string format, params object[] args)
        {
            if (!this.isEnabled || level < this.minLogLevel) return;

            var message = string.Format(format, args);
            this.Log(message, level);
        }
    }
}
