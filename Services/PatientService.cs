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
    public class PatientService : IPatientService
    {
        private readonly OgraLabDbContext _context;

        public PatientService(OgraLabDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync()
        {
            return await _context.Patients
                .OrderByDescending(p => p.CreatedDate)
                .Take(Constants.MaxRecordsPerQuery)
                .ToListAsync();
        }

        /// <summary>
        /// Get patients with pagination for better performance
        /// </summary>
        public async Task<IEnumerable<Patient>> GetPatientsPagedAsync(int page = 1, int pageSize = 0)
        {
            if (pageSize <= 0) pageSize = Constants.DefaultPageSize;
            if (pageSize > Constants.MaxPageSize) pageSize = Constants.MaxPageSize;
            
            return await _context.Patients
                .OrderByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Get total count of patients for pagination
        /// </summary>
        public async Task<int> GetPatientsCountAsync()
        {
            return await _context.Patients.CountAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int patientId)
        {
            return await _context.Patients
                .Include(p => p.PatientTests)
                .ThenInclude(pt => pt.TestType)
                .FirstOrDefaultAsync(p => p.PatientId == patientId);
        }

        public async Task<Patient?> GetPatientByPatientNumberAsync(string patientNumber)
        {
            return await _context.Patients
                .Include(p => p.PatientTests)
                .ThenInclude(pt => pt.TestType)
                .FirstOrDefaultAsync(p => p.PatientNumber == patientNumber);
        }

        public async Task<Patient?> GetPatientByNationalIdAsync(string nationalId)
        {
            return await _context.Patients
                .Include(p => p.PatientTests)
                .ThenInclude(pt => pt.TestType)
                .FirstOrDefaultAsync(p => p.NationalId == nationalId);
        }

        public async Task<IEnumerable<Patient>> SearchPatientsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetActivePatientsAsync();
            }

            searchTerm = searchTerm.Trim().ToLower();

            return await _context.Patients
                .Where(p => p.IsActive &&
                           (p.FullName.ToLower().Contains(searchTerm) ||
                            p.PatientNumber.ToLower().Contains(searchTerm) ||
                            p.NationalId.ToLower().Contains(searchTerm) ||
                            (p.PhoneNumber != null && p.PhoneNumber.Contains(searchTerm))))
                .OrderBy(p => p.FullName)
                .ToListAsync();
        }

        public async Task<Patient> CreatePatientAsync(Patient patient)
        {
            // Check if patient number already exists
            if (await IsPatientNumberExistsAsync(patient.PatientNumber))
            {
                throw new InvalidOperationException("رقم المريض موجود بالفعل");
            }

            // Check if national ID already exists
            if (await IsNationalIdExistsAsync(patient.NationalId))
            {
                throw new InvalidOperationException("رقم الهوية الوطنية موجود بالفعل");
            }

            // Generate patient number if not provided
            if (string.IsNullOrWhiteSpace(patient.PatientNumber))
            {
                patient.PatientNumber = await GeneratePatientNumberAsync();
            }

            patient.CreatedDate = DateTime.Now;
            patient.IsActive = true;

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            return patient;
        }

        public async Task<Patient> UpdatePatientAsync(Patient patient)
        {
            var existingPatient = await _context.Patients.FindAsync(patient.PatientId);
            if (existingPatient == null)
            {
                throw new InvalidOperationException("المريض غير موجود");
            }

            // Check if patient number is changed and if new patient number already exists
            if (existingPatient.PatientNumber != patient.PatientNumber)
            {
                if (await _context.Patients.AnyAsync(p => p.PatientNumber == patient.PatientNumber && p.PatientId != patient.PatientId))
                {
                    throw new InvalidOperationException("رقم المريض موجود بالفعل");
                }
            }

            // Check if national ID is changed and if new national ID already exists
            if (existingPatient.NationalId != patient.NationalId)
            {
                if (await _context.Patients.AnyAsync(p => p.NationalId == patient.NationalId && p.PatientId != patient.PatientId))
                {
                    throw new InvalidOperationException("رقم الهوية الوطنية موجود بالفعل");
                }
            }

            // Update properties
            existingPatient.FullName = patient.FullName;
            existingPatient.PatientNumber = patient.PatientNumber;
            existingPatient.NationalId = patient.NationalId;
            existingPatient.DateOfBirth = patient.DateOfBirth;
            existingPatient.Gender = patient.Gender;
            existingPatient.PhoneNumber = patient.PhoneNumber;
            existingPatient.Email = patient.Email;
            existingPatient.Address = patient.Address;
            existingPatient.BloodType = patient.BloodType;
            existingPatient.MedicalHistory = patient.MedicalHistory;
            existingPatient.Allergies = patient.Allergies;
            existingPatient.EmergencyContact = patient.EmergencyContact;
            existingPatient.EmergencyPhone = patient.EmergencyPhone;
            existingPatient.IsActive = patient.IsActive;
            existingPatient.Notes = patient.Notes;
            existingPatient.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return existingPatient;
        }

        public async Task<bool> DeletePatientAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return false;
            }

            // Check if patient has related tests
            var hasTests = await _context.PatientTests.AnyAsync(pt => pt.PatientId == patientId);
            if (hasTests)
            {
                // Instead of deleting, deactivate the patient
                patient.IsActive = false;
                await _context.SaveChangesAsync();
                return true;
            }

            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ActivatePatientAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return false;
            }

            patient.IsActive = true;
            patient.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivatePatientAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            if (patient == null)
            {
                return false;
            }

            patient.IsActive = false;
            patient.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Patient>> GetActivePatientsAsync()
        {
            return await _context.Patients
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Patient>> GetPatientsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Patients
                .Where(p => p.CreatedDate >= fromDate && p.CreatedDate <= toDate)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<string> GeneratePatientNumberAsync()
        {
            var today = DateTime.Now;
            var prefix = $"P{today:yyyyMMdd}";
            
            var lastPatient = await _context.Patients
                .Where(p => p.PatientNumber.StartsWith(prefix))
                .OrderByDescending(p => p.PatientNumber)
                .FirstOrDefaultAsync();

            int sequence = 1;
            if (lastPatient != null)
            {
                var lastSequence = lastPatient.PatientNumber.Substring(prefix.Length);
                if (int.TryParse(lastSequence, out int lastNum))
                {
                    sequence = lastNum + 1;
                }
            }

            return $"{prefix}{sequence:D3}";
        }

        public async Task<bool> IsPatientNumberExistsAsync(string patientNumber)
        {
            return await _context.Patients
                .AnyAsync(p => p.PatientNumber == patientNumber);
        }

        public async Task<bool> IsNationalIdExistsAsync(string nationalId)
        {
            return await _context.Patients
                .AnyAsync(p => p.NationalId == nationalId);
        }

        public async Task<int> GetTotalPatientsCountAsync()
        {
            return await _context.Patients
                .Where(p => p.IsActive)
                .CountAsync();
        }

        public async Task<int> GetTodayPatientsCountAsync()
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            return await _context.Patients
                .Where(p => p.CreatedDate >= today && p.CreatedDate < tomorrow)
                .CountAsync();
        }
    }
}
