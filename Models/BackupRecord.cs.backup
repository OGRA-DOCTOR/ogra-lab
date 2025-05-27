using System;
using System.ComponentModel.DataAnnotations;

namespace OGRALAB.Models
{
    public class BackupRecord
    {
        [Key]
        public int BackupId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public DateTime BackupDate { get; set; } = DateTime.Now;

        [MaxLength(20)]
        public string BackupType { get; set; } = "Manual"; // Manual, Automatic, Scheduled

        [MaxLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedDate { get; set; }

        [MaxLength(50)]
        public string? VerifiedBy { get; set; }

        public bool IsCorrupted { get; set; } = false;

        [MaxLength(1000)]
        public string? ErrorMessage { get; set; }

        public int DatabaseVersion { get; set; } = 1;

        public int RecordCount { get; set; } = 0;

        [MaxLength(100)]
        public string CheckSum { get; set; } = string.Empty;

        // Computed Properties
        public long FileSize => FileSizeBytes; // Alias for compatibility
        public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

        public string BackupTypeDisplay => BackupType switch
        {
            "Manual" => "يدوي",
            "Automatic" => "تلقائي",
            "Scheduled" => "مجدول",
            _ => "غير معروف"
        };

        public string StatusDisplay => IsCorrupted ? "تالف" : IsVerified ? "مُتحقق" : "غير مُتحقق";

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1073741824) // GB
                return $"{bytes / 1073741824.0:F2} GB";
            if (bytes >= 1048576) // MB
                return $"{bytes / 1048576.0:F2} MB";
            if (bytes >= 1024) // KB
                return $"{bytes / 1024.0:F2} KB";
            return $"{bytes} Bytes";
        }
    }
}
