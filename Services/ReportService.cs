using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;
using System;
using OGRALAB.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public class ReportService : IReportService
    {
        private readonly OgraLabDbContext _context;

        public ReportService(OgraLabDbContext context)
        {
            _context = context;
        }

        // Daily Reports
        public async Task<DailyReportData> GetDailyReportAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var tests = await _context.PatientTests
                .Where(pt => pt.OrderDate >= startDate && pt.OrderDate < endDate)
                .ToListAsync();

            var newPatients = await _context.Patients
                .Where(p => p.CreatedDate >= startDate && p.CreatedDate < endDate)
                .CountAsync();

            return new DailyReportData
            {
                Date = date,
                TotalTests = tests.Count,
                CompletedTests = tests.Count(t => t.Status == "Completed"),
                PendingTests = tests.Count(t => t.Status != "Completed" && t.Status != "Cancelled"),
                CancelledTests = tests.Count(t => t.Status == "Cancelled"),
                NewPatients = newPatients,
                TotalRevenue = tests.Sum(t => t.TotalAmount),
                PaidAmount = tests.Sum(t => t.PaidAmount),
                PendingAmount = tests.Sum(t => t.TotalAmount - t.PaidAmount)
            };
        }

        public async Task<IEnumerable<PatientTest>> GetDailyTestsAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _context.PatientTests
                .Include(pt => pt.Patient)
                .Include(pt => pt.TestType)
                .Where(pt => pt.OrderDate >= startDate && pt.OrderDate < endDate)
                .OrderByDescending(pt => pt.OrderDate)
                .ToListAsync();
        }

        public async Task<decimal> GetDailyRevenueAsync(DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _context.PatientTests
                .Where(pt => pt.OrderDate >= startDate && pt.OrderDate < endDate)
                .SumAsync(pt => pt.PaidAmount);
        }

        // Monthly Reports
        public async Task<MonthlyReportData> GetMonthlyReportAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var tests = await _context.PatientTests
                .Where(pt => pt.OrderDate >= startDate && pt.OrderDate < endDate)
                .ToListAsync();

            var newPatients = await _context.Patients
                .Where(p => p.CreatedDate >= startDate && p.CreatedDate < endDate)
                .CountAsync();

            // Get daily breakdown
            var dailyBreakdown = new List<DailyReportData>();
            for (var date = startDate; date < endDate; date = date.AddDays(1))
            {
                dailyBreakdown.Add(await GetDailyReportAsync(date));
            }

            return new MonthlyReportData
            {
                Year = year,
                Month = month,
                TotalTests = tests.Count,
                CompletedTests = tests.Count(t => t.Status == "Completed"),
                PendingTests = tests.Count(t => t.Status != "Completed" && t.Status != "Cancelled"),
                CancelledTests = tests.Count(t => t.Status == "Cancelled"),
                NewPatients = newPatients,
                TotalRevenue = tests.Sum(t => t.TotalAmount),
                PaidAmount = tests.Sum(t => t.PaidAmount),
                PendingAmount = tests.Sum(t => t.TotalAmount - t.PaidAmount),
                DailyBreakdown = dailyBreakdown
            };
        }

        public async Task<IEnumerable<PatientTest>> GetMonthlyTestsAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await _context.PatientTests
                .Include(pt => pt.Patient)
                .Include(pt => pt.TestType)
                .Where(pt => pt.OrderDate >= startDate && pt.OrderDate < endDate)
                .OrderByDescending(pt => pt.OrderDate)
                .ToListAsync();
        }

        public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            return await _context.PatientTests
                .Where(pt => pt.OrderDate >= startDate && pt.OrderDate < endDate)
                .SumAsync(pt => pt.PaidAmount);
        }

        // Patient Reports
        public async Task<IEnumerable<PatientTest>> GetPatientTestHistoryAsync(int patientId)
        {
            return await _context.PatientTests
                .Include(pt => pt.TestType)
                .Include(pt => pt.TestResults)
                .Where(pt => pt.PatientId == patientId)
                .OrderByDescending(pt => pt.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestResult>> GetPatientResultsAsync(int patientId, int? testTypeId = null)
        {
            var query = _context.TestResults
                .Include(tr => tr.PatientTest)
                .Include(tr => tr.TestType)
                .Where(tr => tr.PatientTest.PatientId == patientId);

            if (testTypeId.HasValue)
            {
                query = query.Where(tr => tr.TestTypeId == testTypeId.Value);
            }

            return await query
                .OrderByDescending(tr => tr.ResultDate)
                .ToListAsync();
        }

        // Revenue Reports
        public async Task<IEnumerable<RevenueByDateData>> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            var endDate = toDate.AddDays(1);

            var revenues = new List<RevenueByDateData>();
            
            for (var date = fromDate.Date; date < endDate; date = date.AddDays(1))
            {
                var nextDate = date.AddDays(1);
                var dayTests = await _context.PatientTests
                    .Where(pt => pt.OrderDate >= date && pt.OrderDate < nextDate)
                    .ToListAsync();

                revenues.Add(new RevenueByDateData
                {
                    Date = date,
                    Revenue = dayTests.Sum(pt => pt.PaidAmount),
                    TestsCount = dayTests.Count
                });
            }

            return revenues;
        }

        public async Task<IEnumerable<RevenueByTestTypeData>> GetRevenueByTestTypeAsync(DateTime fromDate, DateTime toDate)
        {
            var endDate = toDate.AddDays(1);

            return await _context.PatientTests
                .Include(pt => pt.TestType)
                .Where(pt => pt.OrderDate >= fromDate && pt.OrderDate < endDate)
                .GroupBy(pt => new { pt.TestType.TestName, pt.TestType.TestCode })
                .Select(g => new RevenueByTestTypeData
                {
                    TestTypeName = g.Key.TestName,
                    TestTypeCode = g.Key.TestCode,
                    Revenue = g.Sum(pt => pt.PaidAmount),
                    TestsCount = g.Count()
                })
                .OrderByDescending(r => r.Revenue)
                .ToListAsync();
        }

        // Statistics
        public async Task<SystemStatistics> GetSystemStatisticsAsync()
        {
            var totalPatients = await _context.Patients.CountAsync();
            var activePatients = await _context.Patients.Where(p => p.IsActive).CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.Where(u => u.IsActive).CountAsync();
            var totalTestTypes = await _context.TestTypes.CountAsync();
            var activeTestTypes = await _context.TestTypes.Where(t => t.IsActive).CountAsync();
            var totalTests = await _context.PatientTests.CountAsync();
            var completedTests = await _context.PatientTests.Where(pt => pt.Status == "Completed").CountAsync();
            var pendingTests = await _context.PatientTests.Where(pt => pt.Status != "Completed" && pt.Status != "Cancelled").CountAsync();
            var totalRevenue = await _context.PatientTests.SumAsync(pt => pt.PaidAmount);

            return new SystemStatistics
            {
                TotalPatients = totalPatients,
                ActivePatients = activePatients,
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalTestTypes = totalTestTypes,
                ActiveTestTypes = activeTestTypes,
                TotalTests = totalTests,
                CompletedTests = completedTests,
                PendingTests = pendingTests,
                TotalRevenue = totalRevenue,
                LastBackupDate = null // TODO: Implement backup tracking
            };
        }

        public async Task<IEnumerable<TopTestTypeData>> GetTopTestTypesAsync(DateTime fromDate, DateTime toDate, int top = Constants.MaxConcurrentOperations)
        {
            var endDate = toDate.AddDays(1);

            return await _context.PatientTests
                .Include(pt => pt.TestType)
                .Where(pt => pt.OrderDate >= fromDate && pt.OrderDate < endDate)
                .GroupBy(pt => new { pt.TestType.TestName, pt.TestType.TestCode })
                .Select(g => new TopTestTypeData
                {
                    TestTypeName = g.Key.TestName,
                    TestTypeCode = g.Key.TestCode,
                    TestsCount = g.Count(),
                    Revenue = g.Sum(pt => pt.PaidAmount)
                })
                .OrderByDescending(t => t.TestsCount)
                .Take(top)
                .ToListAsync();
        }
    }
}
