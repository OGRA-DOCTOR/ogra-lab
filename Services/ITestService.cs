using OGRALAB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public interface ITestService
    {
        // Test Types Management
        Task<IEnumerable<TestType>> GetAllTestTypesAsync();
        Task<TestType?> GetTestTypeByIdAsync(int testTypeId);
        Task<TestType?> GetTestTypeByCodeAsync(string testCode);
        Task<IEnumerable<TestType>> GetTestTypesByCategoryAsync(string category);
        Task<IEnumerable<TestType>> GetActiveTestTypesAsync();
        Task<TestType> CreateTestTypeAsync(TestType testType);
        Task<TestType> UpdateTestTypeAsync(TestType testType);
        Task<bool> DeleteTestTypeAsync(int testTypeId);

        // Test Groups Management
        Task<IEnumerable<TestGroup>> GetAllTestGroupsAsync();
        Task<TestGroup?> GetTestGroupByIdAsync(int testGroupId);
        Task<TestGroup?> GetTestGroupByCodeAsync(string groupCode);
        Task<IEnumerable<TestGroup>> GetActiveTestGroupsAsync();
        Task<TestGroup> CreateTestGroupAsync(TestGroup testGroup);
        Task<TestGroup> UpdateTestGroupAsync(TestGroup testGroup);
        Task<bool> DeleteTestGroupAsync(int testGroupId);

        // Patient Tests Management
        Task<IEnumerable<PatientTest>> GetAllPatientTestsAsync();
        Task<PatientTest?> GetPatientTestByIdAsync(int patientTestId);
        Task<PatientTest?> GetPatientTestByOrderNumberAsync(string orderNumber);
        Task<IEnumerable<PatientTest>> GetPatientTestsByPatientIdAsync(int patientId);
        Task<IEnumerable<PatientTest>> GetPatientTestsByStatusAsync(string status);
        Task<IEnumerable<PatientTest>> GetTodayPatientTestsAsync();
        Task<PatientTest> CreatePatientTestAsync(PatientTest patientTest);
        Task<PatientTest> UpdatePatientTestAsync(PatientTest patientTest);
        Task<bool> CancelPatientTestAsync(int patientTestId, string cancellationReason);
        Task<string> GenerateOrderNumberAsync();

        // Test Results Management
        Task<IEnumerable<TestResult>> GetTestResultsByPatientTestIdAsync(int patientTestId);
        Task<TestResult?> GetTestResultByIdAsync(int testResultId);
        Task<TestResult> CreateTestResultAsync(TestResult testResult);
        Task<TestResult> UpdateTestResultAsync(TestResult testResult);
        Task<bool> VerifyTestResultAsync(int testResultId, int verifiedByUserId);
        Task<bool> DeleteTestResultAsync(int testResultId);

        // Statistics
        Task<int> GetActiveTestsCountAsync();
        Task<int> GetPendingTestsCountAsync();
        Task<int> GetCompletedTodayTestsCountAsync();
        Task<int> GetTestsCountByStatusAsync(string status);
    }
}
