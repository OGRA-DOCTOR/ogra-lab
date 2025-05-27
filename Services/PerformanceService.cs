using System;
using OGRALAB.Helpers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;

namespace OGRALAB.Services
{
    public class PerformanceService : IPerformanceService, IDisposable
    {
        private readonly OgraLabDbContext _context;
        private readonly ILoggingService _loggingService;
        
        private readonly ConcurrentDictionary<string, object> _cache;
        private readonly ConcurrentDictionary<string, DateTime> _cacheTimestamps;
        private TimeSpan _cacheExpiration = TimeSpan.FromMinutes(Constants.CacheDurationMinutes);
        
        private long _memoryLimit = Constants.MaxPageSize * 1024 * 1024; // 500MB default
        private readonly DateTime _startTime;
        private readonly PerformanceCounter? _cpuCounter;
        private readonly List<System.Timers.Timer> _maintenanceTasks;
        private bool _monitoringEnabled;
        private readonly ConcurrentDictionary<string, Stopwatch> _operationTimers;

        public PerformanceService(OgraLabDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
            _cache = new ConcurrentDictionary<string, object>();
            _cacheTimestamps = new ConcurrentDictionary<string, DateTime>();
            _startTime = DateTime.Now;
            _maintenanceTasks = new List<System.Timers.Timer>();
            _operationTimers = new ConcurrentDictionary<string, Stopwatch>();

            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // First call always returns 0
            }
            catch
            {
                _cpuCounter = null;
            }

            // Schedule automatic cleanup
            ScheduleMaintenanceTask(TimeSpan.FromMinutes(Constants.DatabaseTimeoutSeconds), CleanupCacheAsync);
        }

        #region Memory Management

        public void OptimizeMemory()
        {
            try
            {
                // Clear expired cache
                ClearExpiredCache();

                // Force garbage collection
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                // Compact large object heap
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();

                _ = _loggingService.LogInfoAsync("تم تحسين استخدام الذاكرة", "Performance");
            }
            catch (Exception ex)
            {
                _ = _loggingService.LogErrorAsync("خطأ في تحسين الذاكرة", ex, "Performance");
            }
        }

        public void CleanupResources()
        {
            try
            {
                // Clear all caches
                ClearCache();

                // Cleanup database connection pool
                _context.Database.CloseConnection();

                // Optimize memory
                OptimizeMemory();

                _ = _loggingService.LogInfoAsync("تم تنظيف الموارد", "Performance");
            }
            catch (Exception ex)
            {
                _ = _loggingService.LogErrorAsync("خطأ في تنظيف الموارد", ex, "Performance");
            }
        }

        public long GetMemoryUsage()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                return process.WorkingSet64;
            }
            catch
            {
                return 0;
            }
        }

        public void SetMemoryLimit(long limitInMB)
        {
            _memoryLimit = limitInMB * 1024 * 1024;
            _ = _loggingService.LogInfoAsync($"تم تعيين حد الذاكرة إلى {limitInMB} MB", "Performance");
        }

        #endregion

        #region Database Performance

        public async Task OptimizeDatabaseAsync()
        {
            try
            {
                await VacuumDatabaseAsync();
                await ReindexDatabaseAsync();
                await _loggingService.LogInfoAsync("تم تحسين قاعدة البيانات", "Performance");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحسين قاعدة البيانات", ex, "Performance");
            }
        }

        public async Task<DatabaseStats> GetDatabaseStatsAsync()
        {
            try
            {
                var stats = new DatabaseStats();

                // Get database file size
                var dbPath = _context.Database.GetConnectionString();
                if (!string.IsNullOrEmpty(dbPath) && dbPath.Contains("Data Source="))
                {
                    var filePath = dbPath.Split("Data Source=")[1].Split(';')[0];
                    if (File.Exists(filePath))
                    {
                        stats.DatabaseSize = new FileInfo(filePath).Length;
                    }
                }

                // Get record counts
                stats.RecordCounts["المستخدمين"] = await _context.Users.CountAsync();
                stats.RecordCounts["المرضى"] = await _context.Patients.CountAsync();
                stats.RecordCounts["أنواع التحاليل"] = await _context.TestTypes.CountAsync();
                stats.RecordCounts["تحاليل المرضى"] = await _context.PatientTests.CountAsync();
                stats.RecordCounts["نتائج التحاليل"] = await _context.TestResults.CountAsync();
                stats.RecordCounts["مجموعات التحاليل"] = await _context.TestGroups.CountAsync();
                stats.RecordCounts["سجل الدخول"] = await _context.LoginLogs.CountAsync();
                stats.RecordCounts["سجل النظام"] = await _context.SystemLogs.CountAsync();

                // Table and index counts would require direct SQL queries
                stats.TableCount = Constants.MinPasswordLength; // Known table count
                stats.IndexCount = Constants.CacheDurationMinutes; // Approximate index count

                return stats;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في جمع إحصائيات قاعدة البيانات", ex, "Performance");
                return new DatabaseStats();
            }
        }

        public async Task VacuumDatabaseAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("VACUUM;");
                await _loggingService.LogInfoAsync("تم تنفيذ VACUUM على قاعدة البيانات", "Performance");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تنفيذ VACUUM", ex, "Performance");
            }
        }

        public async Task ReindexDatabaseAsync()
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("REINDEX;");
                await _loggingService.LogInfoAsync("تم إعادة فهرسة قاعدة البيانات", "Performance");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إعادة الفهرسة", ex, "Performance");
            }
        }

        #endregion

        #region Cache Management

        public void ClearCache()
        {
            _cache.Clear();
            _cacheTimestamps.Clear();
            _ = _loggingService.LogInfoAsync("تم مسح جميع البيانات المخزنة مؤقتاً", "Performance");
        }

        public void ClearCache(string cacheKey)
        {
            _cache.TryRemove(cacheKey, out _);
            _cacheTimestamps.TryRemove(cacheKey, out _);
        }

        public void SetCacheExpiration(TimeSpan expiration)
        {
            _cacheExpiration = expiration;
        }

        public T GetFromCache<T>(string key)
        {
            if (_cache.TryGetValue(key, out var value) && 
                _cacheTimestamps.TryGetValue(key, out var timestamp))
            {
                if (DateTime.Now - timestamp < _cacheExpiration)
                {
                    return (T)value;
                }
                else
                {
                    // Expired, remove from cache
                    ClearCache(key);
                }
            }

            return default(T);
        }

        public void SetCache<T>(string key, T value)
        {
            _cache[key] = value;
            _cacheTimestamps[key] = DateTime.Now;
        }

        private void ClearExpiredCache()
        {
            var now = DateTime.Now;
            var expiredKeys = _cacheTimestamps
                .Where(kvp => now - kvp.Value > _cacheExpiration)
                .Select(kvp => kvp.Key)
                .Take(Constants.MaxRecordsPerQuery).ToList();

            // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var key in expiredKeys)
            {
                ClearCache(key);
            }
        }

        private async Task CleanupCacheAsync()
        {
            ClearExpiredCache();
            
            // Check memory usage and clear cache if needed
            var memoryUsage = GetMemoryUsage();
            if (memoryUsage > _memoryLimit)
            {
                ClearCache();
                OptimizeMemory();
                await _loggingService.LogWarningAsync($"تم مسح البيانات المؤقتة بسبب تجاوز حد الذاكرة. الاستخدام: {memoryUsage / 1024 / 1024} MB", "Performance");
            }
        }

        #endregion

        #region Performance Monitoring

        public void StartPerformanceMonitoring()
        {
            _monitoringEnabled = true;
            _ = _loggingService.LogInfoAsync("تم بدء مراقبة الأداء", "Performance");
        }

        public void StopPerformanceMonitoring()
        {
            _monitoringEnabled = false;
            _ = _loggingService.LogInfoAsync("تم إيقاف مراقبة الأداء", "Performance");
        }

        public PerformanceMetrics GetPerformanceMetrics()
        {
            try
            {
                var metrics = new PerformanceMetrics
                {
                    MemoryUsage = GetMemoryUsage(),
                    Uptime = DateTime.Now - _startTime,
                    CacheHitRatio = CalculateCacheHitRatio(),
                    DatabaseSize = GetDatabaseSize()
                };

                // Get CPU usage
                try
                {
                    metrics.CpuUsage = _cpuCounter?.NextValue() ?? 0;
                }
                catch
                {
                    metrics.CpuUsage = 0;
                }

                // Get operation times
                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var timer in _operationTimers)
                {
                    if (timer.Value.IsRunning)
                    {
                        metrics.OperationTimes[timer.Key] = timer.Value.Elapsed;
                    }
                }

                return metrics;
            }
            catch (Exception ex)
            {
                _ = _loggingService.LogErrorAsync("خطأ في جمع مؤشرات الأداء", ex, "Performance");
                return new PerformanceMetrics();
            }
        }

        public async Task<string> GeneratePerformanceReportAsync()
        {
            try
            {
                var metrics = GetPerformanceMetrics();
                var dbStats = await GetDatabaseStatsAsync();

                var report = new StringBuilder();
                report.AppendLine("تقرير أداء النظام - OGRA LAB");
                report.AppendLine($"تاريخ التقرير: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                report.AppendLine(new string('=', Constants.DefaultPageSize));
                report.AppendLine();

                report.AppendLine("مؤشرات الأداء:");
                report.AppendLine($"استخدام الذاكرة: {metrics.MemoryUsage / 1024 / 1024:N0} MB");
                report.AppendLine($"استخدام المعالج: {metrics.CpuUsage:F1}%");
                report.AppendLine($"وقت التشغيل: {metrics.Uptime:dd\\.hh\\:mm\\:ss}");
                report.AppendLine($"معدل إصابة التخزين المؤقت: {metrics.CacheHitRatio}%");
                report.AppendLine();

                report.AppendLine("إحصائيات قاعدة البيانات:");
                report.AppendLine($"حجم قاعدة البيانات: {dbStats.DatabaseSize / 1024 / 1024:N2} MB");
                report.AppendLine($"عدد الجداول: {dbStats.TableCount}");
                report.AppendLine($"عدد الفهارس: {dbStats.IndexCount}");
                report.AppendLine();

                report.AppendLine("عدد السجلات:");
                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var count in dbStats.RecordCounts)
                {
                    report.AppendLine($"{count.Key}: {count.Value:N0}");
                }

                return report.ToString();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء تقرير الأداء", ex, "Performance");
                return "خطأ في إنشاء تقرير الأداء";
            }
        }

        private int CalculateCacheHitRatio()
        {
            // Simplified calculation
            var totalCacheItems = _cache.Count;
            var validCacheItems = _cacheTimestamps.Count(kvp => DateTime.Now - kvp.Value < _cacheExpiration);
            
            if (totalCacheItems == 0) return Constants.CompletePercentage;
            return (validCacheItems * Constants.CompletePercentage) / totalCacheItems;
        }

        private long GetDatabaseSize()
        {
            try
            {
                var dbPath = _context.Database.GetConnectionString();
                if (!string.IsNullOrEmpty(dbPath) && dbPath.Contains("Data Source="))
                {
                    var filePath = dbPath.Split("Data Source=")[1].Split(';')[0];
                    if (File.Exists(filePath))
                    {
                        return new FileInfo(filePath).Length;
                    }
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Optimization Suggestions

        public async Task<List<OptimizationSuggestion>> GetOptimizationSuggestionsAsync()
        {
            var suggestions = new List<OptimizationSuggestion>();

            try
            {
                var metrics = GetPerformanceMetrics();
                var dbStats = await GetDatabaseStatsAsync();

                // Memory optimization suggestions
                if (metrics.MemoryUsage > 300 * 1024 * 1024) // 300MB
                {
                    suggestions.Add(new OptimizationSuggestion
                    {
                        Id = "memory_high",
                        Title = "استخدام ذاكرة مرتفع",
                        Description = "استخدام الذاكرة مرتفع. يمكن تحسينه بمسح البيانات المؤقتة وتحسين الذاكرة.",
                        Impact = "متوسط",
                        IsAutoApplicable = true,
                        Category = "الذاكرة"
                    });
                }

                // Database optimization suggestions
                if (dbStats.DatabaseSize > Constants.CompletePercentage * 1024 * 1024) // 100MB
                {
                    suggestions.Add(new OptimizationSuggestion
                    {
                        Id = "database_large",
                        Title = "حجم قاعدة بيانات كبير",
                        Description = "حجم قاعدة البيانات كبير. يمكن تحسينها بتنفيذ VACUUM وإعادة الفهرسة.",
                        Impact = "عالي",
                        IsAutoApplicable = true,
                        Category = "قاعدة البيانات"
                    });
                }

                // Log cleanup suggestions
                var logCount = dbStats.RecordCounts.GetValueOrDefault("سجل النظام", 0);
                if (logCount > 10000)
                {
                    suggestions.Add(new OptimizationSuggestion
                    {
                        Id = "logs_cleanup",
                        Title = "سجلات كثيرة",
                        Description = "توجد سجلات نظام كثيرة. يمكن حذف السجلات القديمة لتحسين الأداء.",
                        Impact = "منخفض",
                        IsAutoApplicable = false,
                        Category = "السجلات"
                    });
                }

                // Cache optimization
                if (metrics.CacheHitRatio < Constants.HighCompletionThreshold)
                {
                    suggestions.Add(new OptimizationSuggestion
                    {
                        Id = "cache_low_hit",
                        Title = "معدل إصابة تخزين مؤقت منخفض",
                        Description = "معدل إصابة التخزين المؤقت منخفض. يمكن زيادة مدة انتهاء الصلاحية لتحسين الأداء.",
                        Impact = "منخفض",
                        IsAutoApplicable = true,
                        Category = "التخزين المؤقت"
                    });
                }

                return suggestions;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في جمع اقتراحات التحسين", ex, "Performance");
                return suggestions;
            }
        }

        public async Task ApplyOptimizationAsync(string optimizationId)
        {
            try
            {
                switch (optimizationId)
                {
                    case "memory_high":
                        OptimizeMemory();
                        break;

                    case "database_large":
                        await OptimizeDatabaseAsync();
                        break;

                    case "cache_low_hit":
                        SetCacheExpiration(TimeSpan.FromMinutes(Constants.DatabaseTimeoutSeconds));
                        break;

                    default:
                        await _loggingService.LogWarningAsync($"تحسين غير معروف: {optimizationId}", "Performance");
                        break;
                }

                await _loggingService.LogInfoAsync($"تم تطبيق التحسين: {optimizationId}", "Performance");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"خطأ في تطبيق التحسين: {optimizationId}", ex, "Performance");
            }
        }

        #endregion

        #region Background Tasks

        public void ScheduleMaintenanceTask(TimeSpan interval, Func<Task> task)
        {
            var timer = new System.Timers.Timer(interval.TotalMilliseconds);
            timer.Elapsed += async (sender, e) =>
            {
                try
                {
                    await task();
                }
                catch (Exception ex)
                {
                    await _loggingService.LogErrorAsync("خطأ في مهمة الصيانة", ex, "Performance");
                }
            };
            timer.AutoReset = true;
            timer.Enabled = true;
            
            _maintenanceTasks.Add(timer);
        }

        public void StopMaintenanceTasks()
        {
            // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var timer in _maintenanceTasks)
            {
                timer.Stop();
                timer.Dispose();
            }
            _maintenanceTasks.Clear();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            StopMaintenanceTasks();
            _cpuCounter?.Dispose();
            
            // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var timer in _operationTimers.Values)
            {
                timer.Stop();
            }
            _operationTimers.Clear();
        }

        #endregion
    }
}
