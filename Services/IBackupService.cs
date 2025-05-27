using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OGRALAB.Models;

namespace OGRALAB.Services
{
    public interface IBackupService
    {
        // Backup Operations
        Task<BackupRecord> CreateBackupAsync(string description = "", string backupType = "Manual", string createdBy = "");
        Task<bool> RestoreBackupAsync(int backupId, string restoredBy = "");
        Task<bool> DeleteBackupAsync(int backupId, string deletedBy = "");
        Task<bool> VerifyBackupAsync(int backupId, string verifiedBy = "");

        // Backup Management
        Task<IEnumerable<BackupRecord>> GetAllBackupsAsync();
        Task<BackupRecord?> GetBackupByIdAsync(int backupId);
        Task<IEnumerable<BackupRecord>> GetBackupsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<BackupRecord>> GetBackupsByTypeAsync(string backupType);

        // Automatic Backup
        Task<bool> EnableAutomaticBackupAsync(int intervalHours);
        Task<bool> DisableAutomaticBackupAsync();
        Task<bool> RunScheduledBackupAsync();
        Task<bool> ScheduleAutoBackupAsync(bool enabled, int intervalHours, string performedBy);

        // Backup Cleanup
        Task<int> CleanupOldBackupsAsync(int maxBackupsToKeep);
        Task<int> CleanupOldBackupsAsync(int maxBackupsToKeep, string performedBy);
        Task<int> CleanupCorruptedBackupsAsync();
        Task<long> GetTotalBackupSizeAsync();

        // Backup Validation
        Task<bool> ValidateBackupIntegrityAsync(int backupId);
        Task<bool> ValidateAllBackupsAsync();
        
        // Export/Import
        Task<bool> ExportBackupListAsync(string filePath);
        Task<bool> ExportBackupAsync(int backupId, string destinationPath, string performedBy);
        Task<BackupRecord> ImportBackupAsync(string sourcePath, string description, string performedBy);
        Task<string> GetBackupInfoAsync(int backupId);

        // Settings
        Task<string> GetBackupDirectoryAsync();
        Task<bool> SetBackupDirectoryAsync(string path);
        Task<bool> TestBackupDirectoryAccessAsync(string path);
    }
}
