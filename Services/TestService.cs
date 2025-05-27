using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;
using OGRALAB.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public class TestService : ITestService
    {
        private readonly OgraLabDbContext _context;

        public TestService(OgraLabDbContext context)
        {
            _context = context;
        }

        // Test Types Management
        public async Task<IEnumerable<TestType>> GetAllTestTypesAsync()
        {
            return await _context.TestTypes
                .Include(t => t.TestGroup)
                .OrderBy(t => t.TestName)
                .Take(Constants.MaxRecordsPerQuery)
                .ToListAsync();
        }

        /// <summary>
        /// Get test types with pagination for better performance
        /// </summary>
        public async Task<IEnumerable<TestType>> GetTestTypesPagedAsync(int page = 1, int pageSize = 0)
        {
            if (pageSize <= 0) pageSize = Constants.DefaultPageSize;
            if (pageSize > Constants.MaxPageSize) pageSize = Constants.MaxPageSize;
            
            return await _context.TestTypes
                .Include(t => t.TestGroup)
                .OrderBy(t => t.TestName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<TestType?> GetTestTypeByIdAsync(int testTypeId)
        {
            return await _context.TestTypes
                .Include(t => t.TestGroup)
                .FirstOrDefaultAsync(t => t.TestTypeId == testTypeId);
        }

        public async Task<TestType?> GetTestTypeByCodeAsync(string testCode)
        {
            return await _context.TestTypes
                .Include(t => t.TestGroup)
                .FirstOrDefaultAsync(t => t.TestCode == testCode);
        }

        public async Task<IEnumerable<TestType>> GetTestTypesByCategoryAsync(string category)
        {
            return await _context.TestTypes
                .Include(t => t.TestGroup)
                .Where(t => t.Category == category && t.IsActive)
                .OrderBy(t => t.TestName)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestType>> GetActiveTestTypesAsync()
        {
            return await _context.TestTypes
                .Include(t => t.TestGroup)
                .Where(t => t.IsActive)
                .OrderBy(t => t.TestName)
                .ToListAsync();
        }

        public async Task<TestType> CreateTestTypeAsync(TestType testType)
        {
            // Check if test code already exists
            if (await _context.TestTypes.AnyAsync(t => t.TestCode == testType.TestCode))
            {
                throw new InvalidOperationException("كود التحليل موجود بالفعل");
            }

            testType.CreatedDate = DateTime.Now;
            testType.IsActive = true;

            _context.TestTypes.Add(testType);
            await _context.SaveChangesAsync();

            return testType;
        }

        public async Task<TestType> UpdateTestTypeAsync(TestType testType)
        {
            var existingTestType = await _context.TestTypes.FindAsync(testType.TestTypeId);
            if (existingTestType == null)
            {
                throw new InvalidOperationException("نوع التحليل غير موجود");
            }

            // Check if test code is changed and if new test code already exists
            if (existingTestType.TestCode != testType.TestCode)
            {
                if (await _context.TestTypes.AnyAsync(t => t.TestCode == testType.TestCode && t.TestTypeId != testType.TestTypeId))
                {
                    throw new InvalidOperationException("كود التحليل موجود بالفعل");
                }
            }

            // Update properties
            existingTestType.TestCode = testType.TestCode;
            existingTestType.TestName = testType.TestName;
            existingTestType.Description = testType.Description;
            existingTestType.Category = testType.Category;
            existingTestType.Unit = testType.Unit;
            existingTestType.MinNormalValue = testType.MinNormalValue;
            existingTestType.MaxNormalValue = testType.MaxNormalValue;
            existingTestType.NormalRange = testType.NormalRange;
            existingTestType.Price = testType.Price;
            existingTestType.PreparationTime = testType.PreparationTime;
            existingTestType.ResultTime = testType.ResultTime;
            existingTestType.PreparationInstructions = testType.PreparationInstructions;
            existingTestType.IsActive = testType.IsActive;
            existingTestType.RequiresFasting = testType.RequiresFasting;
            existingTestType.TestGroupId = testType.TestGroupId;
            existingTestType.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingTestType;
        }

        public async Task<bool> DeleteTestTypeAsync(int testTypeId)
        {
            var testType = await _context.TestTypes.FindAsync(testTypeId);
            if (testType == null)
            {
                return false;
            }

            // Check if test type has related patient tests
            var hasPatientTests = await _context.PatientTests.AnyAsync(pt => pt.TestTypeId == testTypeId);
            if (hasPatientTests)
            {
                // Instead of deleting, deactivate the test type
                testType.IsActive = false;
                testType.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }

            _context.TestTypes.Remove(testType);
            await _context.SaveChangesAsync();
            return true;
        }

        // Test Groups Management
        public async Task<IEnumerable<TestGroup>> GetAllTestGroupsAsync()
        {
            return await _context.TestGroups
                .Include(g => g.TestTypes)
                .OrderBy(g => g.GroupName)
                .ToListAsync();
        }

        public async Task<TestGroup?> GetTestGroupByIdAsync(int testGroupId)
        {
            return await _context.TestGroups
                .Include(g => g.TestTypes)
                .FirstOrDefaultAsync(g => g.TestGroupId == testGroupId);
        }

        public async Task<TestGroup?> GetTestGroupByCodeAsync(string groupCode)
        {
            return await _context.TestGroups
                .Include(g => g.TestTypes)
                .FirstOrDefaultAsync(g => g.GroupCode == groupCode);
        }

        public async Task<IEnumerable<TestGroup>> GetActiveTestGroupsAsync()
        {
            return await _context.TestGroups
                .Include(g => g.TestTypes)
                .Where(g => g.IsActive)
                .OrderBy(g => g.GroupName)
                .ToListAsync();
        }

        public async Task<TestGroup> CreateTestGroupAsync(TestGroup testGroup)
        {
            // Check if group code already exists
            if (await _context.TestGroups.AnyAsync(g => g.GroupCode == testGroup.GroupCode))
            {
                throw new InvalidOperationException("كود المجموعة موجود بالفعل");
            }

            testGroup.CreatedDate = DateTime.Now;
            testGroup.IsActive = true;

            _context.TestGroups.Add(testGroup);
            await _context.SaveChangesAsync();

            return testGroup;
        }

        public async Task<TestGroup> UpdateTestGroupAsync(TestGroup testGroup)
        {
            var existingTestGroup = await _context.TestGroups.FindAsync(testGroup.TestGroupId);
            if (existingTestGroup == null)
            {
                throw new InvalidOperationException("مجموعة التحليل غير موجودة");
            }

            // Check if group code is changed and if new group code already exists
            if (existingTestGroup.GroupCode != testGroup.GroupCode)
            {
                if (await _context.TestGroups.AnyAsync(g => g.GroupCode == testGroup.GroupCode && g.TestGroupId != testGroup.TestGroupId))
                {
                    throw new InvalidOperationException("كود المجموعة موجود بالفعل");
                }
            }

            // Update properties
            existingTestGroup.GroupCode = testGroup.GroupCode;
            existingTestGroup.GroupName = testGroup.GroupName;
            existingTestGroup.Description = testGroup.Description;
            existingTestGroup.GroupPrice = testGroup.GroupPrice;
            existingTestGroup.DiscountPercentage = testGroup.DiscountPercentage;
            existingTestGroup.IsActive = testGroup.IsActive;
            existingTestGroup.Notes = testGroup.Notes;
            existingTestGroup.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingTestGroup;
        }

        public async Task<bool> DeleteTestGroupAsync(int testGroupId)
        {
            var testGroup = await _context.TestGroups.FindAsync(testGroupId);
            if (testGroup == null)
            {
                return false;
            }

            // Check if test group has related test types
            var hasTestTypes = await _context.TestTypes.AnyAsync(t => t.TestGroupId == testGroupId);
            if (hasTestTypes)
            {
                // Instead of deleting, deactivate the test group
                testGroup.IsActive = false;
                testGroup.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }

            _context.TestGroups.Remove(testGroup);
            await _context.SaveChangesAsync();
            return true;
        }

        // Patient Tests Management
        public async Task<IEnumerable<PatientTest>> GetAllPatientTestsAsync()
        {
            return await _context.PatientTests
                .Include(pt => pt.Patient)
                .Include(pt => pt.TestType)
                .OrderByDescending(pt => pt.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PatientTest>> GetPatientTestsAsync(int patientId)
        {
            return await _context.PatientTests
                .Include(pt => pt.TestType)
                .Include(pt => pt.TestResults)
                .Where(pt => pt.PatientId == patientId)
                .OrderByDescending(pt => pt.OrderDate)
                .ToListAsync();
        }

        public async Task<PatientTest?> GetPatientTestByIdAsync(int patientTestId)
        {
            return await _context.PatientTests
                .Include(pt => pt.Patient)
                .Include(pt => pt.TestType)
                .Include(pt => pt.TestResults)
                .FirstOrDefaultAsync(pt => pt.PatientTestId == patientTestId);
        }

        public async Task<PatientTest?> GetPatientTestByOrderNumberAsync(string orderNumber)
        {
            return await _context.PatientTests
                .Include(pt => pt.Patient)
                .Include(pt => pt.TestType)
                .Include(pt => pt.TestResults)
                .FirstOrDefaultAsync(pt => pt.OrderNumber == orderNumber);
        }

        public async Task<IEnumerable<PatientTest>> GetPatientTestsByPatientIdAsync(int patientId)
        {
            return await _context.PatientTests
                .Include(pt => pt.TestType)
                .Include(pt => pt.TestResults)
                .Where(pt => pt.PatientId == patientId)
                .OrderByDescending(pt => pt.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PatientTest>> GetPatientTestsByStatusAsync(string status)
        {
            return await _context.PatientTests
                .Include(pt => pt.Patient)
                .Include(pt => pt.TestType)
                .Where(pt => pt.Status == status)
                .OrderByDescending(pt => pt.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PatientTest>> GetTodayPatientTestsAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.PatientTests
                .Include(pt => pt.Patient)
                .Include(pt => pt.TestType)
                .Where(pt => pt.OrderDate >= today && pt.OrderDate < tomorrow)
                .OrderByDescending(pt => pt.OrderDate)
                .ToListAsync();
        }

        public async Task<PatientTest> CreatePatientTestAsync(PatientTest patientTest)
        {
            // Generate order number if not provided
            if (string.IsNullOrWhiteSpace(patientTest.OrderNumber))
            {
                patientTest.OrderNumber = await GenerateOrderNumberAsync();
            }
            else
            {
                // Check if order number already exists
                if (await _context.PatientTests.AnyAsync(pt => pt.OrderNumber == patientTest.OrderNumber))
                {
                    throw new InvalidOperationException("رقم الطلب موجود بالفعل");
                }
            }

            patientTest.OrderDate = DateTime.Now;
            patientTest.Status = "Ordered";

            _context.PatientTests.Add(patientTest);
            await _context.SaveChangesAsync();

            return patientTest;
        }

        public async Task<PatientTest> UpdatePatientTestAsync(PatientTest patientTest)
        {
            var existingPatientTest = await _context.PatientTests.FindAsync(patientTest.PatientTestId);
            if (existingPatientTest == null)
            {
                throw new InvalidOperationException("طلب التحليل غير موجود");
            }

            // Update properties
            existingPatientTest.Status = patientTest.Status;
            existingPatientTest.SampleCollectionDate = patientTest.SampleCollectionDate;
            existingPatientTest.ResultDate = patientTest.ResultDate;
            existingPatientTest.SampleType = patientTest.SampleType;
            existingPatientTest.OrderedBy = patientTest.OrderedBy;
            existingPatientTest.PaidAmount = patientTest.PaidAmount;
            existingPatientTest.TotalAmount = patientTest.TotalAmount;
            existingPatientTest.DiscountPercentage = patientTest.DiscountPercentage;
            existingPatientTest.PaymentStatus = patientTest.PaymentStatus;
            existingPatientTest.Priority = patientTest.Priority;
            existingPatientTest.IsEmergency = patientTest.IsEmergency;
            existingPatientTest.Notes = patientTest.Notes;
            existingPatientTest.ProcessedByUserId = patientTest.ProcessedByUserId;
            existingPatientTest.ApprovedByUserId = patientTest.ApprovedByUserId;

            await _context.SaveChangesAsync();
            return existingPatientTest;
        }

        public async Task<bool> CancelPatientTestAsync(int patientTestId, string cancellationReason)
        {
            var patientTest = await _context.PatientTests.FindAsync(patientTestId);
            if (patientTest == null)
            {
                return false;
            }

            patientTest.Status = "Cancelled";
            patientTest.CancellationReason = cancellationReason;
            patientTest.CancellationDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateOrderNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"T{today:yyyyMMdd}";
            
            var lastOrder = await _context.PatientTests
                .Where(pt => pt.OrderNumber.StartsWith(prefix))
                .OrderByDescending(pt => pt.OrderNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastOrder != null)
            {
                var lastSequence = lastOrder.OrderNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastNum))
                {
                    sequence = lastNum + 1;
                }
            }

            return $"{prefix}{sequence:D4}";
        }

        // Test Results Management
        public async Task<IEnumerable<TestResult>> GetAllTestResultsAsync()
        {
            return await _context.TestResults
                .Include(tr => tr.PatientTest)
                .ThenInclude(pt => pt.Patient)
                .Include(tr => tr.PatientTest)
                .ThenInclude(pt => pt.TestType)
                .Include(tr => tr.TestType)
                .OrderByDescending(tr => tr.EnteredDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<TestResult>> GetTestResultsByPatientTestIdAsync(int patientTestId)
        {
            return await _context.TestResults
                .Include(tr => tr.TestType)
                .Where(tr => tr.PatientTestId == patientTestId)
                .OrderBy(tr => tr.ParameterName)
                .ToListAsync();
        }

        public async Task<TestResult?> GetTestResultByIdAsync(int testResultId)
        {
            return await _context.TestResults
                .Include(tr => tr.PatientTest)
                .ThenInclude(pt => pt.Patient)
                .Include(tr => tr.TestType)
                .FirstOrDefaultAsync(tr => tr.TestResultId == testResultId);
        }

        public async Task<TestResult> CreateTestResultAsync(TestResult testResult)
        {
            testResult.ResultDate = DateTime.Now;
            testResult.CreatedDate = DateTime.Now;
            testResult.IsVerified = false;

            _context.TestResults.Add(testResult);
            await _context.SaveChangesAsync();

            return testResult;
        }

        public async Task<TestResult> UpdateTestResultAsync(TestResult testResult)
        {
            var existingTestResult = await _context.TestResults.FindAsync(testResult.TestResultId);
            if (existingTestResult == null)
            {
                throw new InvalidOperationException("نتيجة التحليل غير موجودة");
            }

            // Update properties
            existingTestResult.ParameterName = testResult.ParameterName;
            existingTestResult.ResultValue = testResult.ResultValue;
            existingTestResult.NumericValue = testResult.NumericValue;
            existingTestResult.Unit = testResult.Unit;
            existingTestResult.ReferenceRange = testResult.ReferenceRange;
            existingTestResult.Status = testResult.Status;
            existingTestResult.Flag = testResult.Flag;
            existingTestResult.Comments = testResult.Comments;
            existingTestResult.Interpretation = testResult.Interpretation;
            existingTestResult.IsCritical = testResult.IsCritical;
            existingTestResult.IsRepeated = testResult.IsRepeated;
            existingTestResult.RepeatReason = testResult.RepeatReason;
            existingTestResult.MethodUsed = testResult.MethodUsed;
            existingTestResult.InstrumentUsed = testResult.InstrumentUsed;
            existingTestResult.UpdatedDate = DateTime.Now;
            existingTestResult.UpdatedByUserId = testResult.UpdatedByUserId;

            await _context.SaveChangesAsync();
            return existingTestResult;
        }

        public async Task<bool> VerifyTestResultAsync(int testResultId, int verifiedByUserId)
        {
            var testResult = await _context.TestResults.FindAsync(testResultId);
            if (testResult == null)
            {
                return false;
            }

            testResult.IsVerified = true;
            testResult.VerificationDate = DateTime.Now;
            testResult.VerifiedByUserId = verifiedByUserId;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTestResultAsync(int testResultId)
        {
            var testResult = await _context.TestResults.FindAsync(testResultId);
            if (testResult == null)
            {
                return false;
            }

            _context.TestResults.Remove(testResult);
            await _context.SaveChangesAsync();
            return true;
        }

        // Statistics
        public async Task<int> GetActiveTestsCountAsync()
        {
            return await _context.PatientTests
                .Where(pt => pt.Status != "Completed" && pt.Status != "Cancelled")
                .CountAsync();
        }

        public async Task<int> GetPendingTestsCountAsync()
        {
            return await _context.PatientTests
                .Where(pt => pt.Status == "Ordered" || pt.Status == "SampleCollected")
                .CountAsync();
        }

        public async Task<int> GetCompletedTodayTestsCountAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.PatientTests
                .Where(pt => pt.Status == "Completed" && 
                            pt.ResultDate >= today && pt.ResultDate < tomorrow)
                .CountAsync();
        }

        public async Task<int> GetTestsCountByStatusAsync(string status)
        {
            return await _context.PatientTests
                .Where(pt => pt.Status == status)
                .CountAsync();
        }

        // Test Groups and Test Types Relationship Management
        public async Task<IEnumerable<TestType>> GetTestTypesByGroupIdAsync(int testGroupId)
        {
            var testGroup = await _context.TestGroups
                .Include(tg => tg.TestTypes)
                .FirstOrDefaultAsync(tg => tg.TestGroupId == testGroupId);

            return testGroup?.TestTypes ?? new List<TestType>();
        }

        public async Task<bool> AddTestTypeToGroupAsync(int testGroupId, int testTypeId)
        {
            try
            {
                var testGroup = await _context.TestGroups
                    .Include(tg => tg.TestTypes)
                    .FirstOrDefaultAsync(tg => tg.TestGroupId == testGroupId);

                var testType = await _context.TestTypes
                    .FirstOrDefaultAsync(tt => tt.TestTypeId == testTypeId);

                if (testGroup == null || testType == null) return false;

                // Check if already exists
                if (testGroup.TestTypes.Any(tt => tt.TestTypeId == testTypeId))
                    return true; // Already exists

                testGroup.TestTypes.Add(testType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveTestTypeFromGroupAsync(int testGroupId, int testTypeId)
        {
            try
            {
                var testGroup = await _context.TestGroups
                    .Include(tg => tg.TestTypes)
                    .FirstOrDefaultAsync(tg => tg.TestGroupId == testGroupId);

                if (testGroup == null) return false;

                var testType = testGroup.TestTypes.FirstOrDefault(tt => tt.TestTypeId == testTypeId);
                if (testType == null) return true; // Already removed

                testGroup.TestTypes.Remove(testType);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTestGroupTestTypesAsync(int testGroupId, List<int> testTypeIds)
        {
            try
            {
                var testGroup = await _context.TestGroups
                    .Include(tg => tg.TestTypes)
                    .FirstOrDefaultAsync(tg => tg.TestGroupId == testGroupId);

                if (testGroup == null) return false;

                // Clear existing test types
                testGroup.TestTypes.Clear();

                // Add new test types
                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var testTypeId in testTypeIds)
                {
                    var testType = await _context.TestTypes
                        .FirstOrDefaultAsync(tt => tt.TestTypeId == testTypeId);
                    
                    if (testType != null)
                    {
                        testGroup.TestTypes.Add(testType);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
