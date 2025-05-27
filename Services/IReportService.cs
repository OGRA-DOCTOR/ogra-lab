using OGRALAB.Models;
using System;
using OGRALAB.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public interface IReportService
    {
        // Daily Reports
        Task<DailyReportData> GetDailyReportAsync(DateTime date);
        Task<IEnumerable<PatientTest>> GetDailyTestsAsync(DateTime date);
        Task<decimal> GetDailyRevenueAsync(DateTime date);
        
        // Monthly Reports
        Task<MonthlyReportData> GetMonthlyReportAsync(int year, int month);
        Task<IEnumerable<PatientTest>> GetMonthlyTestsAsync(int year, int month);
        Task<decimal> GetMonthlyRevenueAsync(int year, int month);
        
        // Patient Reports
        Task<IEnumerable<PatientTest>> GetPatientTestHistoryAsync(int patientId);
        Task<IEnumerable<TestResult>> GetPatientResultsAsync(int patientId, int? testTypeId = null);
        
        // Revenue Reports
        Task<IEnumerable<RevenueByDateData>> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<RevenueByTestTypeData>> GetRevenueByTestTypeAsync(DateTime fromDate, DateTime toDate);
        
        // Statistics
        Task<SystemStatistics> GetSystemStatisticsAsync();
        Task<IEnumerable<TopTestTypeData>> GetTopTestTypesAsync(DateTime fromDate, DateTime toDate, int top = Constants.MaxConcurrentOperations);
    }

    // Data Transfer Objects for Reports
    public class DailyReportData
    {
        public DateTime Date { get; set; }
        public int TotalTests { get; set; }
        public int CompletedTests { get; set; }
        public int PendingTests { get; set; }
        public int CancelledTests { get; set; }
        public int NewPatients { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
    }

    public class MonthlyReportData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalTests { get; set; }
        public int CompletedTests { get; set; }
        public int PendingTests { get; set; }
        public int CancelledTests { get; set; }
        public int NewPatients { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public IEnumerable<DailyReportData> DailyBreakdown { get; set; } = new List<DailyReportData>();
    }

    public class RevenueByDateData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int TestsCount { get; set; }
    }

    public class RevenueByTestTypeData
    {
        public string TestTypeName { get; set; } = string.Empty;
        public string TestTypeCode { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int TestsCount { get; set; }
    }

    public class TopTestTypeData
    {
        public string TestTypeName { get; set; } = string.Empty;
        public string TestTypeCode { get; set; } = string.Empty;
        public int TestsCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SystemStatistics
    {
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int TotalTestTypes { get; set; }
        public int ActiveTestTypes { get; set; }
        public int TotalTests { get; set; }
        public int CompletedTests { get; set; }
        public int PendingTests { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime? LastBackupDate { get; set; }
    }
}
