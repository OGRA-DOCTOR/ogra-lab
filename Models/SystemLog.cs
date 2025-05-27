using System;
using OGRALAB.Helpers;

namespace OGRALAB.Models
{
    public class SystemLog
    {
        [Key]
        public int LogId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(20)]
        public string Level { get; set; } = string.Empty; // Debug, Info, Warning, Error

        [Required]
        [MaxLength(Constants.DefaultPageSize)]
        public string Category { get; set; } = string.Empty; // General, UserActivity, Security, etc.

        [Required]
        [MaxLength(Constants.MaxRecordsPerQuery)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(Constants.DefaultPageSize)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(Constants.CompletePercentage)]
        public string MachineName { get; set; } = string.Empty;

        [MaxLength(Constants.DefaultPageSize)]
        public string? SourceMethod { get; set; }

        [MaxLength(Constants.MaxPatientNameLength)]
        public string? AdditionalInfo { get; set; }

        // Computed Properties
        public string LevelDisplay => Level switch
        {
            "Debug" => "تصحيح",
            "Info" => "معلومات",
            "Warning" => "تحذير",
            "Error" => "خطأ",
            _ => Level
        };

        public string CategoryDisplay => Category switch
        {
            "General" => "عام",
            "UserActivity" => "نشاط المستخدم",
            "Security" => "أمان",
            "DatabaseActivity" => "قاعدة البيانات",
            "SystemEvent" => "حدث النظام",
            "Error" => "خطأ",
            "Warning" => "تحذير",
            "Activity" => "نشاط",
            _ => Category
        };

        public string TimestampFormatted => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
