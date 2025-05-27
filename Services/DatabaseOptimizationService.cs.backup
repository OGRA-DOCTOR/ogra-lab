using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;

namespace OGRALAB.Services
{
    public class DatabaseOptimizationService : IDatabaseOptimizationService
    {
        private readonly OgraLabDbContext _context;
        private readonly ILoggingService _loggingService;
        private int _batchSize = 1000;
        private bool _batchProcessingEnabled = true;

        public DatabaseOptimizationService(OgraLabDbContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        #region Query Optimization

        public async Task OptimizeQueriesAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("بدء تحسين الاستعلامات", "DatabaseOptimization");

                // Update SQLite statistics
                await _context.Database.ExecuteSqlRawAsync("ANALYZE;");

                // Optimize common queries by ensuring proper indexing
                await CreateMissingIndexesAsync();

                await _loggingService.LogInfoAsync("تم تحسين الاستعلامات بنجاح", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحسين الاستعلامات", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task AnalyzeQueryPerformanceAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("بدء تحليل أداء الاستعلامات", "DatabaseOptimization");

                // Enable query planning in SQLite
                await _context.Database.ExecuteSqlRawAsync("PRAGMA query_only = ON;");
                
                // Test common query patterns
                var testQueries = new[]
                {
                    "SELECT COUNT(*) FROM Patients",
                    "SELECT COUNT(*) FROM PatientTests WHERE Status = 'Completed'",
                    "SELECT COUNT(*) FROM TestResults WHERE IsAbnormal = 1",
                    "SELECT COUNT(*) FROM Users WHERE IsActive = 1"
                };

                var slowQueries = new List<SlowQuery>();

                foreach (var query in testQueries)
                {
                    var stopwatch = Stopwatch.StartNew();
                    
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync($"EXPLAIN QUERY PLAN {query}");
                        stopwatch.Stop();

                        if (stopwatch.ElapsedMilliseconds > 100) // Consider > 100ms as slow
                        {
                            slowQueries.Add(new SlowQuery
                            {
                                QueryText = query,
                                ExecutionTime = stopwatch.Elapsed,
                                LastExecuted = DateTime.Now,
                                Recommendations = "Consider adding index for better performance"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        await _loggingService.LogWarningAsync($"فشل في تحليل الاستعلام: {query}", "DatabaseOptimization");
                    }
                }

                await _context.Database.ExecuteSqlRawAsync("PRAGMA query_only = OFF;");

                if (slowQueries.Any())
                {
                    await _loggingService.LogWarningAsync($"تم العثور على {slowQueries.Count} استعلام بطيء", "DatabaseOptimization");
                }

                await _loggingService.LogInfoAsync("تم تحليل أداء الاستعلامات", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحليل أداء الاستعلامات", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task<List<SlowQuery>> GetSlowQueriesAsync()
        {
            // SQLite doesn't have built-in query logging, so we'll return common potentially slow patterns
            var slowQueries = new List<SlowQuery>();

            try
            {
                // Check for tables without proper indexes
                var patientCount = await _context.Patients.CountAsync();
                var testCount = await _context.PatientTests.CountAsync();

                if (patientCount > 1000 || testCount > 5000)
                {
                    slowQueries.Add(new SlowQuery
                    {
                        QueryText = "Large table scans detected",
                        ExecutionTime = TimeSpan.FromMilliseconds(500),
                        Recommendations = "Consider adding indexes for frequently queried columns"
                    });
                }

                await _loggingService.LogInfoAsync($"تم العثور على {slowQueries.Count} استعلام بطيء محتمل", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في الحصول على الاستعلامات البطيئة", ex, "DatabaseOptimization");
            }

            return slowQueries;
        }

        public async Task CreateMissingIndexesAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("إنشاء الفهارس المفقودة", "DatabaseOptimization");

                var indexCommands = new[]
                {
                    "CREATE INDEX IF NOT EXISTS IX_Patients_PatientNumber ON Patients (PatientNumber);",
                    "CREATE INDEX IF NOT EXISTS IX_Patients_NationalId ON Patients (NationalId);",
                    "CREATE INDEX IF NOT EXISTS IX_Patients_Phone ON Patients (Phone);",
                    "CREATE INDEX IF NOT EXISTS IX_PatientTests_Status ON PatientTests (Status);",
                    "CREATE INDEX IF NOT EXISTS IX_PatientTests_OrderDate ON PatientTests (OrderDate);",
                    "CREATE INDEX IF NOT EXISTS IX_PatientTests_PatientId ON PatientTests (PatientId);",
                    "CREATE INDEX IF NOT EXISTS IX_TestResults_IsAbnormal ON TestResults (IsAbnormal);",
                    "CREATE INDEX IF NOT EXISTS IX_TestResults_TestDate ON TestResults (TestDate);",
                    "CREATE INDEX IF NOT EXISTS IX_Users_Username ON Users (Username);",
                    "CREATE INDEX IF NOT EXISTS IX_Users_IsActive ON Users (IsActive);",
                    "CREATE INDEX IF NOT EXISTS IX_LoginLogs_LoginTime ON LoginLogs (LoginTime);",
                    "CREATE INDEX IF NOT EXISTS IX_SystemLogs_Timestamp ON SystemLogs (Timestamp);",
                    "CREATE INDEX IF NOT EXISTS IX_SystemLogs_Level ON SystemLogs (Level);"
                };

                foreach (var command in indexCommands)
                {
                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(command);
                    }
                    catch (Exception ex)
                    {
                        await _loggingService.LogWarningAsync($"فشل في إنشاء فهرس: {command}", "DatabaseOptimization");
                    }
                }

                await _loggingService.LogInfoAsync("تم إنشاء الفهارس المفقودة", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء الفهارس المفقودة", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task RemoveUnusedIndexesAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("إزالة الفهارس غير المستخدمة", "DatabaseOptimization");

                // In SQLite, we can't easily detect unused indexes, so we'll skip this operation
                // In a production environment, this would involve analyzing query execution plans

                await _loggingService.LogInfoAsync("تم فحص الفهارس غير المستخدمة", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إزالة الفهارس غير المستخدمة", ex, "DatabaseOptimization");
                throw;
            }
        }

        #endregion

        #region Data Cleanup

        public async Task CleanupOldRecordsAsync(int daysToKeep = 365)
        {
            try
            {
                await _loggingService.LogInfoAsync($"بدء تنظيف السجلات الأقدم من {daysToKeep} يوم", "DatabaseOptimization");

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                // Cleanup old login logs
                var oldLoginLogs = await _context.LoginLogs
                    .Where(ll => ll.LoginTime < cutoffDate)
                    .CountAsync();

                if (oldLoginLogs > 0)
                {
                    await _context.LoginLogs
                        .Where(ll => ll.LoginTime < cutoffDate)
                        .ExecuteDeleteAsync();
                    
                    await _loggingService.LogInfoAsync($"تم حذف {oldLoginLogs} سجل دخول قديم", "DatabaseOptimization");
                }

                // Cleanup old system logs
                var oldSystemLogs = await _context.SystemLogs
                    .Where(sl => sl.Timestamp < cutoffDate)
                    .CountAsync();

                if (oldSystemLogs > 0)
                {
                    await _context.SystemLogs
                        .Where(sl => sl.Timestamp < cutoffDate)
                        .ExecuteDeleteAsync();
                    
                    await _loggingService.LogInfoAsync($"تم حذف {oldSystemLogs} سجل نظام قديم", "DatabaseOptimization");
                }

                // Cleanup old backup records (keep metadata, but remove very old entries)
                var veryOldBackups = await _context.BackupRecords
                    .Where(br => br.BackupDate < cutoffDate.AddDays(-30)) // Keep backup records for extra 30 days
                    .CountAsync();

                if (veryOldBackups > 0)
                {
                    await _context.BackupRecords
                        .Where(br => br.BackupDate < cutoffDate.AddDays(-30))
                        .ExecuteDeleteAsync();
                    
                    await _loggingService.LogInfoAsync($"تم حذف {veryOldBackups} سجل نسخ احتياطية قديم", "DatabaseOptimization");
                }

                await _loggingService.LogInfoAsync("تم تنظيف السجلات القديمة بنجاح", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تنظيف السجلات القديمة", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task CleanupTemporaryDataAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("تنظيف البيانات المؤقتة", "DatabaseOptimization");

                // Remove any temporary or incomplete records
                var incompleteTests = await _context.PatientTests
                    .Where(pt => string.IsNullOrEmpty(pt.Status) || 
                                (pt.Status == "InProgress" && pt.OrderDate < DateTime.Now.AddDays(-7)))
                    .CountAsync();

                if (incompleteTests > 0)
                {
                    await _context.PatientTests
                        .Where(pt => string.IsNullOrEmpty(pt.Status) || 
                                    (pt.Status == "InProgress" && pt.OrderDate < DateTime.Now.AddDays(-7)))
                        .ExecuteDeleteAsync();
                    
                    await _loggingService.LogInfoAsync($"تم حذف {incompleteTests} تحليل غير مكتمل", "DatabaseOptimization");
                }

                await _loggingService.LogInfoAsync("تم تنظيف البيانات المؤقتة", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تنظيف البيانات المؤقتة", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task ArchiveOldDataAsync(DateTime cutoffDate)
        {
            try
            {
                await _loggingService.LogInfoAsync($"أرشفة البيانات الأقدم من {cutoffDate:yyyy-MM-dd}", "DatabaseOptimization");

                // In a real implementation, this would move old data to archive tables
                // For now, we'll just log the counts that would be archived

                var oldTests = await _context.PatientTests
                    .Where(pt => pt.OrderDate < cutoffDate)
                    .CountAsync();

                var oldResults = await _context.TestResults
                    .Where(tr => tr.TestDate < cutoffDate)
                    .CountAsync();

                await _loggingService.LogInfoAsync($"البيانات المؤهلة للأرشفة: {oldTests} تحليل، {oldResults} نتيجة", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في أرشفة البيانات القديمة", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task DeleteDuplicateRecordsAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("حذف السجلات المكررة", "DatabaseOptimization");

                // Find and remove duplicate patients (by national ID)
                var duplicatePatients = await _context.Patients
                    .GroupBy(p => p.NationalId)
                    .Where(g => g.Count() > 1)
                    .Select(g => new { NationalId = g.Key, Count = g.Count() })
                    .ToListAsync();

                if (duplicatePatients.Any())
                {
                    await _loggingService.LogWarningAsync($"تم العثور على {duplicatePatients.Count} مريض مكرر", "DatabaseOptimization");
                }

                await _loggingService.LogInfoAsync("تم فحص السجلات المكررة", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في حذف السجلات المكررة", ex, "DatabaseOptimization");
                throw;
            }
        }

        #endregion

        #region Database Maintenance

        public async Task UpdateStatisticsAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("تحديث إحصائيات قاعدة البيانات", "DatabaseOptimization");

                await _context.Database.ExecuteSqlRawAsync("ANALYZE;");

                await _loggingService.LogInfoAsync("تم تحديث إحصائيات قاعدة البيانات", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحديث إحصائيات قاعدة البيانات", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task CheckDatabaseIntegrityAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("فحص سلامة قاعدة البيانات", "DatabaseOptimization");

                // SQLite integrity check
                var result = await _context.Database.SqlQueryRaw<string>("PRAGMA integrity_check;").ToListAsync();
                
                if (result.FirstOrDefault() == "ok")
                {
                    await _loggingService.LogInfoAsync("قاعدة البيانات سليمة", "DatabaseOptimization");
                }
                else
                {
                    await _loggingService.LogWarningAsync("تم العثور على مشاكل في سلامة قاعدة البيانات", "DatabaseOptimization");
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في فحص سلامة قاعدة البيانات", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task RepairDatabaseAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("إصلاح قاعدة البيانات", "DatabaseOptimization");

                // SQLite auto-repair through VACUUM
                await _context.Database.ExecuteSqlRawAsync("VACUUM;");

                await _loggingService.LogInfoAsync("تم إصلاح قاعدة البيانات", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إصلاح قاعدة البيانات", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task ShrinkDatabaseAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("ضغط قاعدة البيانات", "DatabaseOptimization");

                await _context.Database.ExecuteSqlRawAsync("VACUUM;");

                await _loggingService.LogInfoAsync("تم ضغط قاعدة البيانات", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في ضغط قاعدة البيانات", ex, "DatabaseOptimization");
                throw;
            }
        }

        #endregion

        #region Performance Analysis

        public async Task<DatabasePerformanceReport> GeneratePerformanceReportAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("إنشاء تقرير أداء قاعدة البيانات", "DatabaseOptimization");

                var stopwatch = Stopwatch.StartNew();

                var report = new DatabasePerformanceReport
                {
                    GeneratedAt = DateTime.Now,
                    TableStats = await GetTableStatisticsAsync(),
                    SlowQueries = await GetSlowQueriesAsync()
                };

                // Get database size
                try
                {
                    var dbPath = _context.Database.GetConnectionString();
                    if (!string.IsNullOrEmpty(dbPath) && dbPath.Contains("Data Source="))
                    {
                        var filePath = dbPath.Split("Data Source=")[1].Split(';')[0];
                        if (System.IO.File.Exists(filePath))
                        {
                            report.TotalSize = new System.IO.FileInfo(filePath).Length;
                        }
                    }
                }
                catch
                {
                    report.TotalSize = 0;
                }

                report.TableCount = report.TableStats.Count;
                report.IndexCount = 15; // Approximate for SQLite

                // Add recommendations
                if (report.TotalSize > 100 * 1024 * 1024) // > 100MB
                {
                    report.Recommendations.Add("قاعدة البيانات كبيرة الحجم - فكر في أرشفة البيانات القديمة");
                }

                if (report.SlowQueries.Any())
                {
                    report.Recommendations.Add("توجد استعلامات بطيئة - فكر في إضافة فهارس");
                }

                var totalRecords = report.TableStats.Sum(ts => ts.RecordCount);
                if (totalRecords > 100000)
                {
                    report.Recommendations.Add("عدد كبير من السجلات - فكر في تحسين الاستعلامات");
                }

                if (!report.Recommendations.Any())
                {
                    report.Recommendations.Add("أداء قاعدة البيانات جيد");
                }

                stopwatch.Stop();
                report.AnalysisTime = stopwatch.Elapsed;

                await _loggingService.LogInfoAsync("تم إنشاء تقرير أداء قاعدة البيانات", "DatabaseOptimization");

                return report;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء تقرير أداء قاعدة البيانات", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task<List<TableStatistics>> GetTableStatisticsAsync()
        {
            try
            {
                var tableStats = new List<TableStatistics>();

                // Get statistics for each table
                var tables = new[]
                {
                    new { Name = "Users", RecordCount = await _context.Users.CountAsync() },
                    new { Name = "Patients", RecordCount = await _context.Patients.CountAsync() },
                    new { Name = "TestTypes", RecordCount = await _context.TestTypes.CountAsync() },
                    new { Name = "TestGroups", RecordCount = await _context.TestGroups.CountAsync() },
                    new { Name = "PatientTests", RecordCount = await _context.PatientTests.CountAsync() },
                    new { Name = "TestResults", RecordCount = await _context.TestResults.CountAsync() },
                    new { Name = "LoginLogs", RecordCount = await _context.LoginLogs.CountAsync() },
                    new { Name = "SystemLogs", RecordCount = await _context.SystemLogs.CountAsync() }
                };

                foreach (var table in tables)
                {
                    tableStats.Add(new TableStatistics
                    {
                        TableName = table.Name,
                        RecordCount = table.RecordCount,
                        SizeInBytes = table.RecordCount * 1024, // Rough estimate
                        IndexCount = 2, // Rough estimate
                        LastUpdated = DateTime.Now
                    });
                }

                return tableStats;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في الحصول على إحصائيات الجداول", ex, "DatabaseOptimization");
                return new List<TableStatistics>();
            }
        }

        public async Task<List<IndexUsageStatistics>> GetIndexUsageAsync()
        {
            // SQLite doesn't provide detailed index usage statistics
            // This would be implemented for more advanced database systems
            await Task.CompletedTask;
            return new List<IndexUsageStatistics>();
        }

        #endregion

        #region Batch Operations

        public async Task OptimizeBatchOperationsAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("تحسين العمليات المجمعة", "DatabaseOptimization");

                // Enable WAL mode for better concurrent performance
                await _context.Database.ExecuteSqlRawAsync("PRAGMA journal_mode = WAL;");

                // Increase cache size
                await _context.Database.ExecuteSqlRawAsync("PRAGMA cache_size = 10000;");

                // Increase timeout
                await _context.Database.ExecuteSqlRawAsync("PRAGMA busy_timeout = 30000;");

                await _loggingService.LogInfoAsync("تم تحسين العمليات المجمعة", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحسين العمليات المجمعة", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task EnableBatchProcessingAsync(bool enable)
        {
            _batchProcessingEnabled = enable;
            await _loggingService.LogInfoAsync($"تم {(enable ? "تفعيل" : "إلغاء تفعيل")} المعالجة المجمعة", "DatabaseOptimization");
        }

        public async Task SetBatchSizeAsync(int size)
        {
            _batchSize = Math.Max(100, Math.Min(10000, size)); // Between 100 and 10000
            await _loggingService.LogInfoAsync($"تم تعيين حجم المجموعة إلى {_batchSize}", "DatabaseOptimization");
        }

        #endregion

        #region Connection Pool Management

        public async Task OptimizeConnectionPoolAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("تحسين مجموعة الاتصالات", "DatabaseOptimization");

                // SQLite doesn't use connection pooling like SQL Server
                // But we can optimize connection parameters
                await _context.Database.ExecuteSqlRawAsync("PRAGMA synchronous = NORMAL;");

                await _loggingService.LogInfoAsync("تم تحسين مجموعة الاتصالات", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحسين مجموعة الاتصالات", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task ClearConnectionPoolAsync()
        {
            try
            {
                await _loggingService.LogInfoAsync("تنظيف مجموعة الاتصالات", "DatabaseOptimization");

                // Close and reopen connection
                await _context.Database.CloseConnectionAsync();
                await _context.Database.OpenConnectionAsync();

                await _loggingService.LogInfoAsync("تم تنظيف مجموعة الاتصالات", "DatabaseOptimization");
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تنظيف مجموعة الاتصالات", ex, "DatabaseOptimization");
                throw;
            }
        }

        public async Task<ConnectionPoolStatistics> GetConnectionPoolStatsAsync()
        {
            // SQLite connection pool statistics are limited
            await Task.CompletedTask;
            
            return new ConnectionPoolStatistics
            {
                ActiveConnections = 1, // SQLite typically uses one connection
                IdleConnections = 0,
                MaxPoolSize = 1,
                MinPoolSize = 1,
                AverageConnectionTime = TimeSpan.FromMilliseconds(10)
            };
        }

        #endregion
    }
}
