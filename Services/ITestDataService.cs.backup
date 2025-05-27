using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public interface ITestDataService
    {
        // Data Creation
        Task<bool> CreateSamplePatientsAsync(int count = 50);
        Task<bool> CreateSampleTestTypesAsync();
        Task<bool> CreateSampleTestGroupsAsync();
        Task<bool> CreateSampleUsersAsync();
        Task<bool> CreateSamplePatientTestsAsync(int count = 100);
        Task<bool> CreateSampleTestResultsAsync();

        // Full Sample Data
        Task<bool> CreateFullSampleDataAsync();
        Task<bool> ClearAllSampleDataAsync();

        // Specific Scenarios
        Task<bool> CreateDemoScenarioAsync(); // For demonstrations
        Task<bool> CreatePerformanceTestDataAsync(); // Large dataset for testing
        Task<bool> CreateReportingTestDataAsync(); // Data optimized for report testing

        // Data Validation
        Task<bool> ValidateDataIntegrityAsync();
        Task<string> GetSampleDataSummaryAsync();
    }
}
