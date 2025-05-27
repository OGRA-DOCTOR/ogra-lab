using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public interface IStatsService
    {
        // General Statistics
        Task<int> GetTotalPatientsCountAsync();
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetTotalTestTypesCountAsync();
        Task<int> GetTotalPatientTestsCountAsync();
        Task<int> GetTotalTestResultsCountAsync();

        // Date-based Statistics
        Task<int> GetPatientsCountByDateAsync(DateTime date);
        Task<int> GetPatientsCountByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<int> GetTestsCountByDateAsync(DateTime date);
        Task<int> GetTestsCountByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<int> GetCompletedTestsCountByDateAsync(DateTime date);
        Task<int> GetCompletedTestsCountByDateRangeAsync(DateTime fromDate, DateTime toDate);

        // Status-based Statistics
        Task<int> GetTestsCountByStatusAsync(string status);
        Task<Dictionary<string, int>> GetTestsCountByAllStatusesAsync();
        Task<int> GetPendingTestsCountAsync();
        Task<int> GetInProgressTestsCountAsync();
        Task<int> GetCompletedTestsCountAsync();

        // User Activity Statistics
        Task<Dictionary<string, int>> GetUserActivityStatsAsync();
        Task<Dictionary<string, int>> GetUserLoginStatsAsync(DateTime fromDate, DateTime toDate);
        Task<int> GetActiveUsersCountAsync(DateTime fromDate, DateTime toDate);
        Task<string> GetMostActiveUserAsync(DateTime fromDate, DateTime toDate);

        // Test Type Statistics
        Task<Dictionary<string, int>> GetMostRequestedTestTypesAsync(int topCount = 10);
        Task<Dictionary<string, int>> GetTestTypesByCategoryAsync();
        Task<Dictionary<string, int>> GetTestTypesUsageAsync(DateTime fromDate, DateTime toDate);

        // Performance Statistics
        Task<double> GetAverageTestProcessingTimeAsync();
        Task<double> GetAverageTestProcessingTimeAsync(DateTime fromDate, DateTime toDate);
        Task<Dictionary<string, double>> GetProcessingTimeByTestTypeAsync();
        Task<int> GetTestsProcessedPerDayAverageAsync(DateTime fromDate, DateTime toDate);

        // Monthly/Yearly Reports
        Task<Dictionary<string, int>> GetMonthlyPatientsStatsAsync(int year);
        Task<Dictionary<string, int>> GetMonthlyTestsStatsAsync(int year);
        Task<Dictionary<string, int>> GetYearlyStatsAsync();

        // System Health Statistics
        Task<long> GetDatabaseSizeAsync();
        Task<int> GetActiveSessionsCountAsync();
        Task<DateTime> GetLastBackupDateAsync();
        Task<int> GetErrorLogsCountAsync(DateTime fromDate, DateTime toDate);

        // Export Statistics
        Task<string> GenerateStatsReportAsync(DateTime fromDate, DateTime toDate);
        Task<bool> ExportStatsToJsonAsync(string filePath, DateTime fromDate, DateTime toDate);
        Task<bool> ExportStatsToCsvAsync(string filePath, DateTime fromDate, DateTime toDate);

        // Real-time Statistics
        Task<Dictionary<string, object>> GetDashboardStatsAsync();
        Task<Dictionary<string, object>> GetRealTimeStatsAsync();
    }
}
