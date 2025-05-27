using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public interface IPerformanceService
    {
        // Memory Management
        void OptimizeMemory();
        void CleanupResources();
        long GetMemoryUsage();
        void SetMemoryLimit(long limitInMB);

        // Database Performance
        Task OptimizeDatabaseAsync();
        Task<DatabaseStats> GetDatabaseStatsAsync();
        Task VacuumDatabaseAsync();
        Task ReindexDatabaseAsync();

        // Cache Management
        void ClearCache();
        void ClearCache(string cacheKey);
        void SetCacheExpiration(TimeSpan expiration);
        T GetFromCache<T>(string key);
        void SetCache<T>(string key, T value);

        // Performance Monitoring
        void StartPerformanceMonitoring();
        void StopPerformanceMonitoring();
        PerformanceMetrics GetPerformanceMetrics();
        Task<string> GeneratePerformanceReportAsync();

        // Optimization Suggestions
        Task<List<OptimizationSuggestion>> GetOptimizationSuggestionsAsync();
        Task ApplyOptimizationAsync(string optimizationId);

        // Background Tasks
        void ScheduleMaintenanceTask(TimeSpan interval, Func<Task> task);
        void StopMaintenanceTasks();
    }

    public class DatabaseStats
    {
        public long DatabaseSize { get; set; }
        public int TableCount { get; set; }
        public int IndexCount { get; set; }
        public Dictionary<string, int> RecordCounts { get; set; } = new();
        public DateTime LastVacuum { get; set; }
        public DateTime LastReindex { get; set; }
    }

    public class PerformanceMetrics
    {
        public long MemoryUsage { get; set; }
        public double CpuUsage { get; set; }
        public TimeSpan Uptime { get; set; }
        public int ActiveConnections { get; set; }
        public Dictionary<string, TimeSpan> OperationTimes { get; set; } = new();
        public int CacheHitRatio { get; set; }
        public long DatabaseSize { get; set; }
    }

    public class OptimizationSuggestion
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty; // Low, Medium, High
        public bool IsAutoApplicable { get; set; }
        public string Category { get; set; } = string.Empty; // Memory, Database, UI, etc.
    }
}
