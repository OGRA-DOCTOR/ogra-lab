using System;
using System.ComponentModel.DataAnnotations;

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
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty; // General, UserActivity, Security, etc.

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [MaxLength(100)]
        public string MachineName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? SourceMethod { get; set; }

        [MaxLength(200)]
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
