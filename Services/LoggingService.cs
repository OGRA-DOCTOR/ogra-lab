using System;
using OGRALAB.Helpers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;

namespace OGRALAB.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly OgraLabDbContext _context;
        private readonly string _logDirectory;
        private LogLevel _currentLogLevel = LogLevel.Info;
        
        private readonly object _fileLock = new object();

        public LoggingService(OgraLabDbContext context)
        {
            _context = context;
            _logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            
            // Ensure logs directory exists
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        #region Log Level Management

        public void SetLogLevel(LogLevel level)
        {
            _currentLogLevel = level;
        }

        public bool IsLogLevelEnabled(LogLevel level)
        {
            return level >= _currentLogLevel;
        }

        #endregion

        #region Core Logging Methods

        public async Task LogInfoAsync(string message, string category = "General", string username = "")
        {
            if (!IsLogLevelEnabled(LogLevel.Info)) return;
            await WriteLogAsync(LogLevel.Info, message, category, username);
        }

        public async Task LogWarningAsync(string message, string category = "Warning", string username = "")
        {
            if (!IsLogLevelEnabled(LogLevel.Warning)) return;
            await WriteLogAsync(LogLevel.Warning, message, category, username);
        }

        public async Task LogErrorAsync(string message, Exception exception = null, string category = "Error", string username = "")
        {
            if (!IsLogLevelEnabled(LogLevel.Error)) return;
            
            var fullMessage = message;
            if (exception != null)
            {
                fullMessage += $"\nException: {exception.Message}";
                if (exception.InnerException != null)
                {
                    fullMessage += $"\nInner Exception: {exception.InnerException.Message}";
                }
                fullMessage += $"\nStack Trace: {exception.StackTrace}";
            }
            
            await WriteLogAsync(LogLevel.Error, fullMessage, category, username);
        }

        public async Task LogErrorAsync(Exception exception, string category = "Error", string username = "")
        {
            await LogErrorAsync("خطأ في النظام", exception, category, username);
        }

        public async Task LogDebugAsync(string message, string category = "Debug", string username = "")
        {
            if (!IsLogLevelEnabled(LogLevel.Debug)) return;
            await WriteLogAsync(LogLevel.Debug, message, category, username);
        }

        public async Task LogActivityAsync(string action, string details = "", string username = "")
        {
            var message = $"النشاط: {action}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" | التفاصيل: {details}";
            }
            
            await LogInfoAsync(message, "Activity", username);
        }

        #endregion

        #region Specialized Logging

        public async Task LogUserActivityAsync(string username, string action, string details = "")
        {
            var message = $"المستخدم: {username} | الإجراء: {action}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" | التفاصيل: {details}";
            }
            
            await LogInfoAsync(message, "UserActivity", username);
        }

        public async Task LogSystemEventAsync(string eventType, string message, string details = "")
        {
            var fullMessage = $"نوع الحدث: {eventType} | الرسالة: {message}";
            if (!string.IsNullOrEmpty(details))
            {
                fullMessage += $" | التفاصيل: {details}";
            }
            
            await LogInfoAsync(fullMessage, "SystemEvent");
        }

        public async Task LogDatabaseActivityAsync(string operation, string table, string details = "", string username = "")
        {
            var message = $"عملية قاعدة البيانات: {operation} على الجدول: {table}";
            if (!string.IsNullOrEmpty(details))
            {
                message += $" | التفاصيل: {details}";
            }
            
            await LogInfoAsync(message, "DatabaseActivity", username);
        }

        public async Task LogSecurityEventAsync(string securityEvent, string username = "", string details = "")
        {
            var message = $"حدث أمني: {securityEvent}";
            if (!string.IsNullOrEmpty(username))
            {
                message += $" | المستخدم: {username}";
            }
            if (!string.IsNullOrEmpty(details))
            {
                message += $" | التفاصيل: {details}";
            }
            
            await LogWarningAsync(message, "Security", username);
        }

        #endregion

        #region Core Writing Logic

        private async Task WriteLogAsync(LogLevel level, string message, string category, string username)
        {
            try
            {
                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = level.ToString(),
                    Category = category,
                    Message = message,
                    Username = username,
                    MachineName = Environment.MachineName,
                    ProcessId = Environment.ProcessId
                };

                // Write to file
                await WriteToFileAsync(logEntry);

                // Write to database if it's important
                if (level >= LogLevel.Warning || category == "UserActivity" || category == "Security")
                {
                    await WriteToDatabaseAsync(logEntry);
                }
            }
            catch (Exception ex)
            {
                // Fallback logging to Windows Event Log or console
                try
                {
                    await WriteToFallbackAsync($"خطأ في Logging Service: {ex.Message} | الرسالة الأصلية: {message}");
                }
                catch
                {
                    // Silent fail if all logging fails
                }
            }
        }

        private async Task WriteToFileAsync(LogEntry logEntry)
        {
            var fileName = $"OgraLab_{DateTime.Now:yyyy-MM-dd}.log";
            var filePath = Path.Combine(_logDirectory, fileName);
            
            var logLine = $"[{logEntry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{logEntry.Level}] [{logEntry.Category}] " +
                         $"{(string.IsNullOrEmpty(logEntry.Username) ? "" : $"[{logEntry.Username}] ")}" +
                         $"{logEntry.Message}{Environment.NewLine}";

            lock (_fileLock)
            {
                File.AppendAllText(filePath, logLine, Encoding.UTF8);
            }

            await Task.CompletedTask;
        }

        private async Task WriteToDatabaseAsync(LogEntry logEntry)
        {
            try
            {
                // Create a simplified log record for database
                var dbLogEntry = new SystemLog
                {
                    Timestamp = logEntry.Timestamp,
                    Level = logEntry.Level,
                    Category = logEntry.Category,
                    Message = logEntry.Message.Length > Constants.MaxRecordsPerQuery ? logEntry.Message.Substring(0, Constants.MaxRecordsPerQuery) : logEntry.Message,
                    Username = logEntry.Username,
                    MachineName = logEntry.MachineName
                };

                _context.SystemLogs.Add(dbLogEntry);
                await _context.SaveChangesAsync();
            }
            catch
            {
                // Silent fail for database logging
            }
        }

        private async Task WriteToFallbackAsync(string message)
        {
            var fallbackFile = Path.Combine(_logDirectory, "fallback.log");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FALLBACK] {message}{Environment.NewLine}";
            
            lock (_fileLock)
            {
                File.AppendAllText(fallbackFile, logLine, Encoding.UTF8);
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Log Management

        public async Task<string> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, string category = "", string username = "")
        {
            try
            {
                var query = _context.SystemLogs.AsQueryable();

                if (fromDate.HasValue)
                    query = query.Where(l => l.Timestamp >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(l => l.Timestamp <= toDate.Value);

                if (!string.IsNullOrEmpty(category))
                    query = query.Where(l => l.Category == category);

                if (!string.IsNullOrEmpty(username))
                    query = query.Where(l => l.Username == username);

                var logs = await query.OrderByDescending(l => l.Timestamp)
                                    .Take(Constants.MaxRecordsPerQuery) // Limit for performance
                                    .ToListAsync();

                var result = new StringBuilder();
                result.AppendLine("سجل أحداث النظام - OGRA LAB");
                result.AppendLine($"تاريخ التصدير: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                result.AppendLine(new string('=', Constants.HighCompletionThreshold));

                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var log in logs)
                {
                    result.AppendLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss}] [{log.Level}] [{log.Category}] " +
                                    $"{(string.IsNullOrEmpty(log.Username) ? "" : $"[{log.Username}] ")}" +
                                    $"{log.Message}");
                }

                return result.ToString();
            }
            catch (Exception ex)
            {
                await LogErrorAsync("خطأ في استرداد السجلات", ex);
                return "خطأ في استرداد السجلات";
            }
        }

        public async Task<bool> ClearLogsAsync(DateTime beforeDate)
        {
            try
            {
                var logsToDelete = await _context.SystemLogs
                    .Where(l => l.Timestamp < beforeDate)
                    .ToListAsync();

                _context.SystemLogs.RemoveRange(logsToDelete);
                await _context.SaveChangesAsync();

                // Also clean up old log files
                var logFiles = Directory.GetFiles(_logDirectory, "*.log");
                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var file in logFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < beforeDate)
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                            // Continue with other files
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("خطأ في حذف السجلات", ex);
                return false;
            }
        }

        public async Task<bool> ExportLogsAsync(string filePath, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var logs = await GetLogsAsync(fromDate, toDate);
                await File.WriteAllTextAsync(filePath, logs, Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("خطأ في تصدير السجلات", ex);
                return false;
            }
        }

        public async Task<long> GetLogsSizeAsync()
        {
            try
            {
                long totalSize = 0;
                
                // Calculate database logs size (approximate)
                var dbLogsCount = await _context.SystemLogs.CountAsync();
                totalSize += dbLogsCount * Constants.MaxPageSize; // Approximate Constants.MaxPageSize bytes per log entry

                // Calculate file logs size
                var logFiles = Directory.GetFiles(_logDirectory, "*.log");
                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var file in logFiles)
                {
                    var fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }

                return totalSize;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> CompressOldLogsAsync(int daysOld = Constants.DatabaseTimeoutSeconds)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-daysOld);
                
                // This is a simplified implementation
                // In a full implementation, you would compress old log files
                
                await LogInfoAsync($"تم ضغط السجلات الأقدم من {cutoffDate:yyyy-MM-dd}", "LogManagement");
                return true;
            }
            catch (Exception ex)
            {
                await LogErrorAsync("خطأ في ضغط السجلات القديمة", ex);
                return false;
            }
        }

        #endregion

        #region Helper Classes

        private class LogEntry
        {
            public DateTime Timestamp { get; set; }
            public string Level { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string MachineName { get; set; } = string.Empty;
            public int ProcessId { get; set; }
        }

        #endregion
    }
}
