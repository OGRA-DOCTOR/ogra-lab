using System;
using OGRALAB.Helpers;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public interface ILoggingService
    {
        // Log Levels
        Task LogInfoAsync(string message, string category = "General", string username = "");
        Task LogWarningAsync(string message, string category = "Warning", string username = "");
        Task LogErrorAsync(string message, Exception exception = null, string category = "Error", string username = "");
        Task LogErrorAsync(Exception exception, string category = "Error", string username = "");
        Task LogDebugAsync(string message, string category = "Debug", string username = "");
        Task LogActivityAsync(string action, string details = "", string username = "");

        // Specialized Logging
        Task LogUserActivityAsync(string username, string action, string details = "");
        Task LogSystemEventAsync(string eventType, string message, string details = "");
        Task LogDatabaseActivityAsync(string operation, string table, string details = "", string username = "");
        Task LogSecurityEventAsync(string securityEvent, string username = "", string details = "");

        // Log Management
        Task<string> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string category = "", string username = "");
        Task<bool> ClearLogsAsync(DateTime beforeDate);
        Task<bool> ExportLogsAsync(string filePath, DateTime? fromDate = null, DateTime? toDate = null);
        Task<long> GetLogsSizeAsync();
        Task<bool> CompressOldLogsAsync(int daysOld = Constants.DatabaseTimeoutSeconds);

        // Configuration
        void SetLogLevel(LogLevel level);
        bool IsLogLevelEnabled(LogLevel level);
    }

    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        None = 4
    }
}
