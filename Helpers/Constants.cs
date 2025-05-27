using System;

namespace OGRALAB.Helpers
{
    /// <summary>
    /// Constants class to replace magic numbers and commonly used values
    /// </summary>
    public static class Constants
    {
        #region Database Constants
        
        /// <summary>
        /// Default timeout for database operations in seconds
        /// </summary>
        public const int DatabaseTimeoutSeconds = 30;
        
        /// <summary>
        /// Maximum number of records to fetch at once for performance
        /// </summary>
        public const int MaxRecordsPerQuery = 1000;
        
        /// <summary>
        /// Default page size for pagination
        /// </summary>
        public const int DefaultPageSize = 50;
        
        /// <summary>
        /// Maximum page size allowed
        /// </summary>
        public const int MaxPageSize = 500;
        
        #endregion
        
        #region UI Constants
        
        /// <summary>
        /// Default window width in pixels
        /// </summary>
        public const int DefaultWindowWidth = 800;
        
        /// <summary>
        /// Default window height in pixels
        /// </summary>
        public const int DefaultWindowHeight = 600;
        
        /// <summary>
        /// Minimum window width in pixels
        /// </summary>
        public const int MinWindowWidth = 400;
        
        /// <summary>
        /// Minimum window height in pixels
        /// </summary>
        public const int MinWindowHeight = 300;
        
        /// <summary>
        /// Default grid row height
        /// </summary>
        public const int DefaultRowHeight = 25;
        
        #endregion
        
        #region Performance Constants
        
        /// <summary>
        /// Maximum number of concurrent operations
        /// </summary>
        public const int MaxConcurrentOperations = 10;
        
        /// <summary>
        /// Cache duration in minutes
        /// </summary>
        public const int CacheDurationMinutes = 15;
        
        /// <summary>
        /// Performance monitoring interval in seconds
        /// </summary>
        public const int PerformanceMonitoringIntervalSeconds = 60;
        
        /// <summary>
        /// Maximum memory usage percentage before warning
        /// </summary>
        public const int MaxMemoryUsagePercentage = 85;
        
        #endregion
        
        #region Test Management Constants
        
        /// <summary>
        /// Default number of test results to display
        /// </summary>
        public const int DefaultTestResultsCount = 20;
        
        /// <summary>
        /// Maximum test name length
        /// </summary>
        public const int MaxTestNameLength = 255;
        
        /// <summary>
        /// Maximum patient name length
        /// </summary>
        public const int MaxPatientNameLength = 200;
        
        /// <summary>
        /// Minimum age for patients
        /// </summary>
        public const int MinPatientAge = 0;
        
        /// <summary>
        /// Maximum age for patients
        /// </summary>
        public const int MaxPatientAge = 150;
        
        #endregion
        
        #region Backup Constants
        
        /// <summary>
        /// Maximum number of backup files to keep
        /// </summary>
        public const int MaxBackupFiles = 30;
        
        /// <summary>
        /// Backup retention period in days
        /// </summary>
        public const int BackupRetentionDays = 90;
        
        /// <summary>
        /// Automatic backup interval in hours
        /// </summary>
        public const int AutoBackupIntervalHours = 24;
        
        #endregion
        
        #region Validation Constants
        
        /// <summary>
        /// Minimum password length
        /// </summary>
        public const int MinPasswordLength = 8;
        
        /// <summary>
        /// Maximum password length
        /// </summary>
        public const int MaxPasswordLength = 128;
        
        /// <summary>
        /// Password hash rounds for BCrypt
        /// </summary>
        public const int PasswordHashRounds = 12;
        
        /// <summary>
        /// Maximum login attempts before lockout
        /// </summary>
        public const int MaxLoginAttempts = 5;
        
        /// <summary>
        /// Lockout duration in minutes
        /// </summary>
        public const int LockoutDurationMinutes = 15;
        
        #endregion
        
        #region File and System Constants
        
        /// <summary>
        /// Maximum file size for uploads in MB
        /// </summary>
        public const int MaxFileUploadSizeMB = 10;
        
        /// <summary>
        /// Maximum log file size in MB
        /// </summary>
        public const int MaxLogFileSizeMB = 50;
        
        /// <summary>
        /// Log rotation count
        /// </summary>
        public const int LogRotationCount = 5;
        
        /// <summary>
        /// System cleanup interval in hours
        /// </summary>
        public const int SystemCleanupIntervalHours = 6;
        
        #endregion
        
        #region Date and Time Constants
        
        /// <summary>
        /// Standard date format for the application
        /// </summary>
        public const string StandardDateFormat = "yyyy-MM-dd";
        
        /// <summary>
        /// Standard datetime format for the application
        /// </summary>
        public const string StandardDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        
        /// <summary>
        /// Report date format
        /// </summary>
        public const string ReportDateFormat = "dd/MM/yyyy";
        
        /// <summary>
        /// Default session timeout in minutes
        /// </summary>
        public const int SessionTimeoutMinutes = 480; // 8 hours
        
        #endregion
        
        #region Search and Filter Constants
        
        /// <summary>
        /// Minimum search term length
        /// </summary>
        public const int MinSearchTermLength = 2;
        
        /// <summary>
        /// Maximum search results to display
        /// </summary>
        public const int MaxSearchResults = 100;
        
        /// <summary>
        /// Search debounce delay in milliseconds
        /// </summary>
        public const int SearchDebounceDelayMs = 300;
        
        #endregion
        
        #region Percentage Constants
        
        /// <summary>
        /// Complete percentage (100%)
        /// </summary>
        public const int CompletePercentage = 100;
        
        /// <summary>
        /// High completion threshold (80%)
        /// </summary>
        public const int HighCompletionThreshold = 80;
        
        /// <summary>
        /// Medium completion threshold (50%)
        /// </summary>
        public const int MediumCompletionThreshold = 50;
        
        /// <summary>
        /// Low completion threshold (25%)
        /// </summary>
        public const int LowCompletionThreshold = 25;
        
        #endregion
        
        #region Error and Retry Constants
        
        /// <summary>
        /// Maximum retry attempts for operations
        /// </summary>
        public const int MaxRetryAttempts = 3;
        
        /// <summary>
        /// Retry delay in milliseconds
        /// </summary>
        public const int RetryDelayMs = 1000;
        
        /// <summary>
        /// Network timeout in seconds
        /// </summary>
        public const int NetworkTimeoutSeconds = 30;
        
        #endregion
    }
}
