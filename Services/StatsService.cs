using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;

namespace OGRALAB.Services
{
    public class StatsService : IStatsService
    {
        private readonly OgraLabDbContext _context;

        public StatsService(OgraLabDbContext context)
        {
            _context = context;
        }

        #region General Statistics

        public async Task<int> GetTotalPatientsCountAsync()
        {
            return await _context.Patients.CountAsync();
        }

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        public async Task<int> GetTotalTestTypesCountAsync()
        {
            return await _context.TestTypes.CountAsync();
        }

        public async Task<int> GetTotalPatientTestsCountAsync()
        {
            return await _context.PatientTests.CountAsync();
        }

        public async Task<int> GetTotalTestResultsCountAsync()
        {
            return await _context.TestResults.CountAsync();
        }

        #endregion

        #region Date-based Statistics

        public async Task<int> GetPatientsCountByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            
            return await _context.Patients
                .Where(p => p.CreatedDate >= startDate && p.CreatedDate < endDate)
                .CountAsync();
        }

        public async Task<int> GetPatientsCountByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Patients
                .Where(p => p.CreatedDate >= fromDate && p.CreatedDate <= toDate)
                .CountAsync();
        }

        public async Task<int> GetTestsCountByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            
            return await _context.PatientTests
                .Where(pt => pt.CreatedDate >= startDate && pt.CreatedDate < endDate)
                .CountAsync();
        }

        public async Task<int> GetTestsCountByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.PatientTests
                .Where(pt => pt.CreatedDate >= fromDate && pt.CreatedDate <= toDate)
                .CountAsync();
        }

        public async Task<int> GetCompletedTestsCountByDateAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);
            
            return await _context.PatientTests
                .Where(pt => pt.Status == "Completed" && 
                            pt.CompletedDate >= startDate && pt.CompletedDate < endDate)
                .CountAsync();
        }

        public async Task<int> GetCompletedTestsCountByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.PatientTests
                .Where(pt => pt.Status == "Completed" && 
                            pt.CompletedDate >= fromDate && pt.CompletedDate <= toDate)
                .CountAsync();
        }

        #endregion

        #region Status-based Statistics

        public async Task<int> GetTestsCountByStatusAsync(string status)
        {
            return await _context.PatientTests
                .Where(pt => pt.Status == status)
                .CountAsync();
        }

        public async Task<Dictionary<string, int>> GetTestsCountByAllStatusesAsync()
        {
            var statusCounts = await _context.PatientTests
                .GroupBy(pt => pt.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            foreach (var item in statusCounts)
            {
                var displayStatus = item.Status switch
                {
                    "Pending" => "معلق",
                    "InProgress" => "جاري",
                    "Completed" => "مكتمل",
                    "Cancelled" => "ملغي",
                    _ => item.Status
                };
                result[displayStatus] = item.Count;
            }

            return result;
        }

        public async Task<int> GetPendingTestsCountAsync()
        {
            return await GetTestsCountByStatusAsync("Pending");
        }

        public async Task<int> GetInProgressTestsCountAsync()
        {
            return await GetTestsCountByStatusAsync("InProgress");
        }

        public async Task<int> GetCompletedTestsCountAsync()
        {
            return await GetTestsCountByStatusAsync("Completed");
        }

        #endregion

        #region User Activity Statistics

        public async Task<Dictionary<string, int>> GetUserActivityStatsAsync()
        {
            var userStats = await _context.Users
                .GroupBy(u => u.Role)
                .Select(g => new { Role = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            foreach (var item in userStats)
            {
                var displayRole = item.Role switch
                {
                    "SystemUser" => "مستخدم نظام",
                    "AdminUser" => "مدير",
                    "RegularUser" => "مستخدم عادي",
                    _ => item.Role
                };
                result[displayRole] = item.Count;
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetUserLoginStatsAsync(DateTime fromDate, DateTime toDate)
        {
            var loginStats = await _context.LoginLogs
                .Where(l => l.LoginTime >= fromDate && l.LoginTime <= toDate)
                .GroupBy(l => l.Username)
                .Select(g => new { Username = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return loginStats.ToDictionary(x => x.Username, x => x.Count);
        }

        public async Task<int> GetActiveUsersCountAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.LoginLogs
                .Where(l => l.LoginTime >= fromDate && l.LoginTime <= toDate)
                .Select(l => l.Username)
                .Distinct()
                .CountAsync();
        }

        public async Task<string> GetMostActiveUserAsync(DateTime fromDate, DateTime toDate)
        {
            var mostActive = await _context.LoginLogs
                .Where(l => l.LoginTime >= fromDate && l.LoginTime <= toDate)
                .GroupBy(l => l.Username)
                .Select(g => new { Username = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            return mostActive?.Username ?? "لا يوجد";
        }

        #endregion

        #region Test Type Statistics

        public async Task<Dictionary<string, int>> GetMostRequestedTestTypesAsync(int topCount = 10)
        {
            var testStats = await _context.PatientTests
                .Include(pt => pt.TestType)
                .GroupBy(pt => pt.TestType.TestName)
                .Select(g => new { TestName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .ToListAsync();

            return testStats.ToDictionary(x => x.TestName, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetTestTypesByCategoryAsync()
        {
            var categoryStats = await _context.TestTypes
                .GroupBy(tt => tt.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            foreach (var item in categoryStats)
            {
                var displayCategory = item.Category switch
                {
                    "Biochemistry" => "كيميائية",
                    "Hematology" => "دموية",
                    "Immunology" => "مناعية",
                    "Microbiology" => "ميكروبيولوجية",
                    "Molecular" => "جزيئية",
                    "Pathology" => "باثولوجية",
                    _ => item.Category ?? "غير محدد"
                };
                result[displayCategory] = item.Count;
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetTestTypesUsageAsync(DateTime fromDate, DateTime toDate)
        {
            var usageStats = await _context.PatientTests
                .Include(pt => pt.TestType)
                .Where(pt => pt.CreatedDate >= fromDate && pt.CreatedDate <= toDate)
                .GroupBy(pt => pt.TestType.TestName)
                .Select(g => new { TestName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return usageStats.ToDictionary(x => x.TestName, x => x.Count);
        }

        #endregion

        #region Performance Statistics

        public async Task<double> GetAverageTestProcessingTimeAsync()
        {
            var completedTests = await _context.PatientTests
                .Where(pt => pt.Status == "Completed" && pt.CompletedDate.HasValue)
                .Select(pt => new { pt.CreatedDate, pt.CompletedDate })
                .ToListAsync();

            if (!completedTests.Any()) return 0;

            var totalHours = completedTests
                .Where(t => t.CompletedDate.HasValue)
                .Sum(t => (t.CompletedDate.Value - t.CreatedDate).TotalHours);

            return totalHours / completedTests.Count;
        }

        public async Task<double> GetAverageTestProcessingTimeAsync(DateTime fromDate, DateTime toDate)
        {
            var completedTests = await _context.PatientTests
                .Where(pt => pt.Status == "Completed" && 
                            pt.CompletedDate.HasValue &&
                            pt.CreatedDate >= fromDate && 
                            pt.CreatedDate <= toDate)
                .Select(pt => new { pt.CreatedDate, pt.CompletedDate })
                .ToListAsync();

            if (!completedTests.Any()) return 0;

            var totalHours = completedTests
                .Where(t => t.CompletedDate.HasValue)
                .Sum(t => (t.CompletedDate.Value - t.CreatedDate).TotalHours);

            return totalHours / completedTests.Count;
        }

        public async Task<Dictionary<string, double>> GetProcessingTimeByTestTypeAsync()
        {
            var processingTimes = await _context.PatientTests
                .Include(pt => pt.TestType)
                .Where(pt => pt.Status == "Completed" && pt.CompletedDate.HasValue)
                .GroupBy(pt => pt.TestType.TestName)
                .Select(g => new 
                { 
                    TestName = g.Key,
                    AvgHours = g.Average(pt => EF.Functions.DateDiffHour(pt.CreatedDate, pt.CompletedDate.Value))
                })
                .ToListAsync();

            return processingTimes.ToDictionary(x => x.TestName, x => x.AvgHours ?? 0);
        }

        public async Task<int> GetTestsProcessedPerDayAverageAsync(DateTime fromDate, DateTime toDate)
        {
            var totalDays = (toDate - fromDate).Days + 1;
            if (totalDays <= 0) return 0;

            var totalCompleted = await GetCompletedTestsCountByDateRangeAsync(fromDate, toDate);
            return totalCompleted / totalDays;
        }

        #endregion

        #region Monthly/Yearly Reports

        public async Task<Dictionary<string, int>> GetMonthlyPatientsStatsAsync(int year)
        {
            var monthlyStats = await _context.Patients
                .Where(p => p.CreatedDate.Year == year)
                .GroupBy(p => p.CreatedDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            string[] monthNames = { "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
                                   "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };

            for (int i = 1; i <= 12; i++)
            {
                var count = monthlyStats.FirstOrDefault(x => x.Month == i)?.Count ?? 0;
                result[monthNames[i]] = count;
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetMonthlyTestsStatsAsync(int year)
        {
            var monthlyStats = await _context.PatientTests
                .Where(pt => pt.CreatedDate.Year == year)
                .GroupBy(pt => pt.CreatedDate.Month)
                .Select(g => new { Month = g.Key, Count = g.Count() })
                .ToListAsync();

            var result = new Dictionary<string, int>();
            string[] monthNames = { "", "يناير", "فبراير", "مارس", "أبريل", "مايو", "يونيو",
                                   "يوليو", "أغسطس", "سبتمبر", "أكتوبر", "نوفمبر", "ديسمبر" };

            for (int i = 1; i <= 12; i++)
            {
                var count = monthlyStats.FirstOrDefault(x => x.Month == i)?.Count ?? 0;
                result[monthNames[i]] = count;
            }

            return result;
        }

        public async Task<Dictionary<string, int>> GetYearlyStatsAsync()
        {
            var currentYear = DateTime.Now.Year;
            var yearlyStats = new Dictionary<string, int>();

            for (int year = currentYear - 4; year <= currentYear; year++)
            {
                var patientsCount = await _context.Patients
                    .Where(p => p.CreatedDate.Year == year)
                    .CountAsync();
                    
                yearlyStats[year.ToString()] = patientsCount;
            }

            return yearlyStats;
        }

        #endregion

        #region System Health Statistics

        public async Task<long> GetDatabaseSizeAsync()
        {
            try
            {
                // This is a simplified implementation
                // In a real scenario, you would query the database system tables
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> GetActiveSessionsCountAsync()
        {
            // Count sessions from the last hour
            var oneHourAgo = DateTime.Now.AddHours(-1);
            return await _context.LoginLogs
                .Where(l => l.LoginTime >= oneHourAgo && !l.LogoutTime.HasValue)
                .CountAsync();
        }

        public async Task<DateTime> GetLastBackupDateAsync()
        {
            var lastBackup = await _context.BackupRecords
                .OrderByDescending(b => b.BackupDate)
                .FirstOrDefaultAsync();

            return lastBackup?.BackupDate ?? DateTime.MinValue;
        }

        public async Task<int> GetErrorLogsCountAsync(DateTime fromDate, DateTime toDate)
        {
            // This would typically query an error log table
            // For now, return 0 as we don't have error logging implemented
            return 0;
        }

        #endregion

        #region Export Statistics

        public async Task<string> GenerateStatsReportAsync(DateTime fromDate, DateTime toDate)
        {
            var report = new StringBuilder();
            
            report.AppendLine("تقرير إحصائيات النظام");
            report.AppendLine($"من: {fromDate:yyyy-MM-dd} إلى: {toDate:yyyy-MM-dd}");
            report.AppendLine($"تاريخ التقرير: {DateTime.Now:yyyy-MM-dd HH:mm}");
            report.AppendLine(new string('=', 50));
            report.AppendLine();

            // General Statistics
            report.AppendLine("الإحصائيات العامة:");
            report.AppendLine($"إجمالي المرضى: {await GetTotalPatientsCountAsync():N0}");
            report.AppendLine($"إجمالي المستخدمين: {await GetTotalUsersCountAsync():N0}");
            report.AppendLine($"إجمالي أنواع التحاليل: {await GetTotalTestTypesCountAsync():N0}");
            report.AppendLine($"إجمالي التحاليل: {await GetTotalPatientTestsCountAsync():N0}");
            report.AppendLine($"إجمالي النتائج: {await GetTotalTestResultsCountAsync():N0}");
            report.AppendLine();

            // Period Statistics
            report.AppendLine("إحصائيات الفترة:");
            report.AppendLine($"مرضى جدد: {await GetPatientsCountByDateRangeAsync(fromDate, toDate):N0}");
            report.AppendLine($"تحاليل جديدة: {await GetTestsCountByDateRangeAsync(fromDate, toDate):N0}");
            report.AppendLine($"تحاليل مكتملة: {await GetCompletedTestsCountByDateRangeAsync(fromDate, toDate):N0}");
            report.AppendLine();

            // Status Statistics
            report.AppendLine("إحصائيات الحالات:");
            var statusStats = await GetTestsCountByAllStatusesAsync();
            foreach (var status in statusStats)
            {
                report.AppendLine($"{status.Key}: {status.Value:N0}");
            }
            report.AppendLine();

            // Performance
            report.AppendLine("إحصائيات الأداء:");
            var avgProcessingTime = await GetAverageTestProcessingTimeAsync(fromDate, toDate);
            report.AppendLine($"متوسط وقت معالجة التحليل: {avgProcessingTime:F1} ساعة");
            var avgPerDay = await GetTestsProcessedPerDayAverageAsync(fromDate, toDate);
            report.AppendLine($"متوسط التحاليل المكتملة يومياً: {avgPerDay:N0}");
            report.AppendLine();

            return report.ToString();
        }

        public async Task<bool> ExportStatsToJsonAsync(string filePath, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var stats = new
                {
                    ReportDate = DateTime.Now,
                    PeriodFrom = fromDate,
                    PeriodTo = toDate,
                    General = new
                    {
                        TotalPatients = await GetTotalPatientsCountAsync(),
                        TotalUsers = await GetTotalUsersCountAsync(),
                        TotalTestTypes = await GetTotalTestTypesCountAsync(),
                        TotalTests = await GetTotalPatientTestsCountAsync(),
                        TotalResults = await GetTotalTestResultsCountAsync()
                    },
                    Period = new
                    {
                        NewPatients = await GetPatientsCountByDateRangeAsync(fromDate, toDate),
                        NewTests = await GetTestsCountByDateRangeAsync(fromDate, toDate),
                        CompletedTests = await GetCompletedTestsCountByDateRangeAsync(fromDate, toDate)
                    },
                    StatusStats = await GetTestsCountByAllStatusesAsync(),
                    TestTypeStats = await GetMostRequestedTestTypesAsync(),
                    UserActivityStats = await GetUserActivityStatsAsync()
                };

                var json = JsonSerializer.Serialize(stats, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ExportStatsToCsvAsync(string filePath, DateTime fromDate, DateTime toDate)
        {
            try
            {
                var csv = new StringBuilder();
                
                // Header
                csv.AppendLine("إحصائيات النظام");
                csv.AppendLine($"من تاريخ,{fromDate:yyyy-MM-dd}");
                csv.AppendLine($"إلى تاريخ,{toDate:yyyy-MM-dd}");
                csv.AppendLine($"تاريخ التقرير,{DateTime.Now:yyyy-MM-dd HH:mm}");
                csv.AppendLine();

                // General stats
                csv.AppendLine("الإحصائيات العامة");
                csv.AppendLine("المؤشر,القيمة");
                csv.AppendLine($"إجمالي المرضى,{await GetTotalPatientsCountAsync()}");
                csv.AppendLine($"إجمالي المستخدمين,{await GetTotalUsersCountAsync()}");
                csv.AppendLine($"إجمالي أنواع التحاليل,{await GetTotalTestTypesCountAsync()}");
                csv.AppendLine($"إجمالي التحاليل,{await GetTotalPatientTestsCountAsync()}");
                csv.AppendLine($"إجمالي النتائج,{await GetTotalTestResultsCountAsync()}");
                csv.AppendLine();

                await File.WriteAllTextAsync(filePath, csv.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Real-time Statistics

        public async Task<Dictionary<string, object>> GetDashboardStatsAsync()
        {
            var today = DateTime.Today;
            var thisMonth = new DateTime(today.Year, today.Month, 1);
            
            return new Dictionary<string, object>
            {
                ["TotalPatients"] = await GetTotalPatientsCountAsync(),
                ["TotalTests"] = await GetTotalPatientTestsCountAsync(),
                ["PendingTests"] = await GetPendingTestsCountAsync(),
                ["CompletedTests"] = await GetCompletedTestsCountAsync(),
                ["TodayPatients"] = await GetPatientsCountByDateAsync(today),
                ["TodayTests"] = await GetTestsCountByDateAsync(today),
                ["TodayCompleted"] = await GetCompletedTestsCountByDateAsync(today),
                ["MonthlyPatients"] = await GetPatientsCountByDateRangeAsync(thisMonth, today.AddDays(1)),
                ["MonthlyTests"] = await GetTestsCountByDateRangeAsync(thisMonth, today.AddDays(1)),
                ["AvgProcessingTime"] = await GetAverageTestProcessingTimeAsync(),
                ["LastBackupDate"] = await GetLastBackupDateAsync()
            };
        }

        public async Task<Dictionary<string, object>> GetRealTimeStatsAsync()
        {
            return await GetDashboardStatsAsync();
        }

        #endregion
    }
}
