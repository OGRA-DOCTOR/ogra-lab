using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OGRALAB.Models
{
    public class Patient
    {
        [Key]
        public int PatientId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PatientNumber { get; set; } = string.Empty; // رقم المريض الفريد

        [Required]
        [StringLength(20)]
        public string NationalId { get; set; } = string.Empty; // رقم الهوية الوطنية

        public DateTime DateOfBirth { get; set; }

        [Required]
        [StringLength(10)]
        public string Gender { get; set; } = string.Empty; // Male, Female

        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? BloodType { get; set; }

        [StringLength(500)]
        public string? MedicalHistory { get; set; }

        [StringLength(500)]
        public string? Allergies { get; set; }

        [StringLength(100)]
        public string? EmergencyContact { get; set; }

        [StringLength(15)]
        public string? EmergencyPhone { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        // Calculated property
        [NotMapped]
        public int Age => DateTime.Now.Year - DateOfBirth.Year;

        // Navigation properties
        public virtual ICollection<PatientTest> PatientTests { get; set; } = new List<PatientTest>();
    }
}
