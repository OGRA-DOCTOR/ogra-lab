using System;
using OGRALAB.Helpers;

namespace OGRALAB.Models
{
    public class SystemSettings
    {
        [Key]
        public int SystemSettingsId { get; set; }

        [MaxLength(Constants.MaxConcurrentOperations)]
        public string Language { get; set; } = "ar-SA";

        [MaxLength(Constants.DefaultPageSize)]
        public string FontFamily { get; set; } = "Cairo";

        public int FontSize { get; set; } = 14;

        [MaxLength(20)]
        public string Theme { get; set; } = "Light";

        public int SessionTimeoutMinutes { get; set; } = 60;

        public bool AutoBackupEnabled { get; set; } = true;

        public int AutoBackupIntervalHours { get; set; } = Constants.AutoBackupIntervalHours;

        public int MaxBackupFiles { get; set; } = Constants.DatabaseTimeoutSeconds;

        [MaxLength(Constants.MaxPageSize)]
        public string BackupPath { get; set; } = "";

        public bool EnableAuditLog { get; set; } = true;

        public int AuditLogRetentionDays { get; set; } = 90;

        public bool EnablePasswordComplexity { get; set; } = true;

        public int MinPasswordLength { get; set; } = Constants.MinPasswordLength;

        public int MaxLoginAttempts { get; set; } = 3;

        public int LoginLockoutMinutes { get; set; } = Constants.CacheDurationMinutes;

        public bool EnableDataValidation { get; set; } = true;

        public bool ShowConfirmationDialogs { get; set; } = true;

        public bool EnableAutoSave { get; set; } = true;

        public int AutoSaveIntervalMinutes { get; set; } = 5;

        [MaxLength(Constants.CompletePercentage)]
        public string DefaultPrinter { get; set; } = "";

        public bool PrintHeaderFooter { get; set; } = true;

        public int ReportMarginTop { get; set; } = 20;

        public int ReportMarginBottom { get; set; } = 20;

        public int ReportMarginLeft { get; set; } = 20;

        public int ReportMarginRight { get; set; } = 20;

        [MaxLength(Constants.DefaultPageSize)]
        public string DateFormat { get; set; } = "dd/MM/yyyy";

        [MaxLength(Constants.DefaultPageSize)]
        public string TimeFormat { get; set; } = "HH:mm";

        [MaxLength(Constants.DefaultPageSize)]
        public string NumberFormat { get; set; } = "0.00";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        [MaxLength(Constants.DefaultPageSize)]
        public string? ModifiedBy { get; set; }
    }
}
