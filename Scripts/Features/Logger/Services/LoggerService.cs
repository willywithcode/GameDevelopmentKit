namespace GameFoundation.Scripts.Features.Logger.Services
{
    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEngine;

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

        [HideInCallstack]
        public void SetEnabled(bool enabled)
        {
            this.isEnabled = enabled;
        }

        [HideInCallstack]
        public void SetMinLogLevel(LogLevel level)
        {
            this.minLogLevel = level;
        }

        [HideInCallstack]
        public void Log(
            string message,
            LogLevel level = LogLevel.Info,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            this.Write(level, message, new CallerInfo(callerMemberName, callerFilePath, callerLineNumber));
        }

        [HideInCallstack]
        public void Debug(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            this.Write(LogLevel.Debug, message, new CallerInfo(callerMemberName, callerFilePath, callerLineNumber));
#endif
        }

        [HideInCallstack]
        public void Info(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            this.Write(LogLevel.Info, message, new CallerInfo(callerMemberName, callerFilePath, callerLineNumber));
        }

        [HideInCallstack]
        public void Warning(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            this.Write(LogLevel.Warning, message, new CallerInfo(callerMemberName, callerFilePath, callerLineNumber));
        }

        [HideInCallstack]
        public void Error(
            string message,
            [CallerMemberName] string callerMemberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0)
        {
            this.Write(LogLevel.Error, message, new CallerInfo(callerMemberName, callerFilePath, callerLineNumber));
        }

        [HideInCallstack]
        public void LogFormat(LogLevel level, string format, params object[] args)
        {
            this.Write(level, string.Format(format, args), FindExternalCaller());
        }

        [HideInCallstack]
        private void Write(LogLevel level, string message, CallerInfo callerInfo)
        {
            if (!this.isEnabled || level < this.minLogLevel)
            {
                return;
            }

            var formattedMessage = FormatMessage(level, message, callerInfo);

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
                default:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
            }
        }

        [HideInCallstack]
        private static string FormatMessage(LogLevel level, string message, CallerInfo callerInfo)
        {
            var taggedMessage = string.Format(TagFormat, GetTag(level), message);

#if UNITY_EDITOR
            return taggedMessage;
#else
            var source = callerInfo.ToUnitySourceLocation();
            return string.IsNullOrEmpty(source)
                ? taggedMessage
                : $"{taggedMessage}\n{source}";
#endif
        }

        [HideInCallstack]
        private static string GetTag(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug   => DebugTag,
                LogLevel.Info    => InfoTag,
                LogLevel.Warning => WarningTag,
                LogLevel.Error   => ErrorTag,
                _                => InfoTag
            };
        }

        [HideInCallstack]
        private static CallerInfo FindExternalCaller()
        {
            var loggerType = typeof(LoggerService);
            var stackTrace = new StackTrace(true);

            for (var i = 0; i < stackTrace.FrameCount; i++)
            {
                var frame = stackTrace.GetFrame(i);
                var method = frame?.GetMethod();
                if (method == null || IsLoggerFrame(method, loggerType))
                {
                    continue;
                }

                return new CallerInfo(method.Name, frame.GetFileName(), frame.GetFileLineNumber());
            }

            return CallerInfo.Empty;
        }

        [HideInCallstack]
        private static bool IsLoggerFrame(MethodBase method, Type loggerType)
        {
            var declaringType = method.DeclaringType;
            return declaringType == loggerType || declaringType?.DeclaringType == loggerType;
        }

        private readonly struct CallerInfo
        {
            public static readonly CallerInfo Empty = new CallerInfo(string.Empty, string.Empty, 0);

            public CallerInfo(string memberName, string filePath, int lineNumber)
            {
                this.memberName = memberName;
                this.filePath = filePath;
                this.lineNumber = lineNumber;
            }

            private readonly string memberName;
            private readonly string filePath;
            private readonly int    lineNumber;

            public string ToUnitySourceLocation()
            {
                if (string.IsNullOrEmpty(this.filePath) || this.lineNumber <= 0)
                {
                    return string.Empty;
                }

                var normalizedPath = this.filePath.Replace('\\', '/');
                var assetsIndex = normalizedPath.LastIndexOf("/Assets/", StringComparison.OrdinalIgnoreCase);
                if (assetsIndex >= 0)
                {
                    normalizedPath = normalizedPath.Substring(assetsIndex + 1);
                }

                var memberLabel = string.IsNullOrEmpty(this.memberName) ? string.Empty : $"{this.memberName} ";
                return $"{memberLabel}(at {normalizedPath}:{this.lineNumber})";
            }
        }
    }
}
