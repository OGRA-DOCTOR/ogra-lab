using OGRALAB.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync();
        Task<Patient?> GetPatientByIdAsync(int patientId);
        Task<Patient?> GetPatientByPatientNumberAsync(string patientNumber);
        Task<Patient?> GetPatientByNationalIdAsync(string nationalId);
        Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm);
        Task<Patient> CreatePatientAsync(Patient patient);
        Task<Patient> UpdatePatientAsync(Patient patient);
        Task<bool> DeletePatientAsync(int patientId);
        Task<bool> ActivatePatientAsync(int patientId);
        Task<bool> DeactivatePatientAsync(int patientId);
        Task<IEnumerable<Patient>> GetActivePatientsAsync();
        Task<IEnumerable<Patient>> GetPatientsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<string> GeneratePatientNumberAsync();
        Task<bool> IsPatientNumberExistsAsync(string patientNumber);
        Task<bool> IsNationalIdExistsAsync(string nationalId);
        Task<int> GetTotalPatientsCountAsync();
        Task<int> GetTodayPatientsCountAsync();
    }
}
