using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public interface IDatabaseOptimizationService
    {
        // Query Optimization
        Task OptimizeQueriesAsync();
        Task AnalyzeQueryPerformanceAsync();
        Task<List<SlowQuery>> GetSlowQueriesAsync();
        Task CreateMissingIndexesAsync();
        Task RemoveUnusedIndexesAsync();

        // Data Cleanup
        Task CleanupOldRecordsAsync(int daysToKeep = 365);
        Task CleanupTemporaryDataAsync();
        Task ArchiveOldDataAsync(DateTime cutoffDate);
        Task DeleteDuplicateRecordsAsync();

        // Database Maintenance
        Task UpdateStatisticsAsync();
        Task CheckDatabaseIntegrityAsync();
        Task RepairDatabaseAsync();
        Task ShrinkDatabaseAsync();

        // Performance Analysis
        Task<DatabasePerformanceReport> GeneratePerformanceReportAsync();
        Task<List<TableStatistics>> GetTableStatisticsAsync();
        Task<List<IndexUsageStatistics>> GetIndexUsageAsync();

        // Batch Operations
        Task OptimizeBatchOperationsAsync();
        Task EnableBatchProcessingAsync(bool enable);
        Task SetBatchSizeAsync(int size);

        // Connection Pool Management
        Task OptimizeConnectionPoolAsync();
        Task ClearConnectionPoolAsync();
        Task<ConnectionPoolStatistics> GetConnectionPoolStatsAsync();
    }

    public class SlowQuery
    {
        public string QueryText { get; set; } = "";
        public TimeSpan ExecutionTime { get; set; }
        public int ExecutionCount { get; set; }
        public DateTime LastExecuted { get; set; }
        public string Recommendations { get; set; } = "";
    }

    public class DatabasePerformanceReport
    {
        public DateTime GeneratedAt { get; set; }
        public long TotalSize { get; set; }
        public int TableCount { get; set; }
        public int IndexCount { get; set; }
        public List<TableStatistics> TableStats { get; set; } = new();
        public List<SlowQuery> SlowQueries { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public TimeSpan AnalysisTime { get; set; }
    }

    public class TableStatistics
    {
        public string TableName { get; set; } = "";
        public long RecordCount { get; set; }
        public long SizeInBytes { get; set; }
        public int IndexCount { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<string> MostUsedColumns { get; set; } = new();
    }

    public class IndexUsageStatistics
    {
        public string IndexName { get; set; } = "";
        public string TableName { get; set; } = "";
        public long UsageCount { get; set; }
        public DateTime LastUsed { get; set; }
        public long SizeInBytes { get; set; }
        public bool IsRecommendedForRemoval { get; set; }
    }

    public class ConnectionPoolStatistics
    {
        public int ActiveConnections { get; set; }
        public int IdleConnections { get; set; }
        public int MaxPoolSize { get; set; }
        public int MinPoolSize { get; set; }
        public TimeSpan AverageConnectionTime { get; set; }
        public int TotalConnectionsCreated { get; set; }
        public int TotalConnectionsDisposed { get; set; }
    }
}
