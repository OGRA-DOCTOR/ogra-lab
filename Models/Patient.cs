using System;
using OGRALAB.Helpers;

namespace OGRALAB.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        [StringLength(Constants.CompletePercentage)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PatientNumber { get; set; } = string.Empty; // رقم المريض الفريد

        [Required]
        [StringLength(20)]
        public string NationalId { get; set; } = string.Empty; // رقم الهوية الوطنية

        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(Constants.MaxConcurrentOperations)]
        public string Gender { get; set; } = string.Empty; // Male, Female

        [StringLength(Constants.CacheDurationMinutes)]
        public string? PhoneNumber { get; set; }

        [StringLength(Constants.CompletePercentage)]
        public string? Email { get; set; }

        [StringLength(Constants.MaxPatientNameLength)]
        public string? Address { get; set; }

        [StringLength(Constants.DefaultPageSize)]
        public string? BloodType { get; set; }

        [StringLength(Constants.MaxPageSize)]
        public string? MedicalHistory { get; set; }

        [StringLength(Constants.MaxPageSize)]
        public string? Allergies { get; set; }

        [StringLength(Constants.CompletePercentage)]
        public string? EmergencyContact { get; set; }

        [StringLength(Constants.CacheDurationMinutes)]
        public string? EmergencyPhone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        [StringLength(Constants.MaxPageSize)]
        public string? Notes { get; set; }

        // Calculated property
        [NotMapped]
        public int Age => DateTime.Now.Year - DateOfBirth.Year;

        // Navigation properties
        public virtual ICollection<PatientTest> PatientTests { get; set; } = new List<PatientTest>();
    }
}
