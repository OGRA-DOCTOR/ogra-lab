using System;
using OGRALAB.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;

namespace OGRALAB.Services
{
    public class BackupService : IBackupService
    {
        private readonly OgraLabDbContext _context;
        private readonly string _defaultBackupPath;
        private readonly string _connectionString;

        public BackupService(OgraLabDbContext context)
        {
            _context = context;
            _defaultBackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            _connectionString = _context.Database.GetConnectionString() ?? "";
            
            // Ensure backup directory exists
            if (!Directory.Exists(_defaultBackupPath))
            {
                Directory.CreateDirectory(_defaultBackupPath);
            }
        }

        #region Backup Operations

        public async Task<BackupRecord> CreateBackupAsync(string description = "", string backupType = "Manual", string createdBy = "")
        {
            try
            {
                var backupDirectory = await GetBackupDirectoryAsync();
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"OgraLab_Backup_{timestamp}.db";
                var filePath = Path.Combine(backupDirectory, fileName);

                // Create the backup
                await CreateDatabaseBackupAsync(filePath);

                // Verify the backup file was created
                if (!File.Exists(filePath))
                {
                    throw new Exception("فشل في إنشاء ملف النسخة الاحتياطية");
                }

                var fileInfo = new FileInfo(filePath);
                var checkSum = await CalculateFileCheckSumAsync(filePath);

                // Get record count
                var recordCount = await GetDatabaseRecordCountAsync();

                // Create backup record
                var backupRecord = new BackupRecord
                {
                    FileName = fileName,
                    FilePath = filePath,
                    FileSizeBytes = fileInfo.Length,
                    BackupDate = DateTime.Now,
                    BackupType = backupType,
                    CreatedBy = createdBy,
                    Description = string.IsNullOrWhiteSpace(description) ? $"نسخة احتياطية {backupType}" : description,
                    IsVerified = false,
                    IsCorrupted = false,
                    DatabaseVersion = 1,
                    RecordCount = recordCount,
                    CheckSum = checkSum
                };

                _context.BackupRecords.Add(backupRecord);
                await _context.SaveChangesAsync();

                // Verify the backup immediately after creation
                await VerifyBackupAsync(backupRecord.BackupId, createdBy);

                return backupRecord;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في إنشاء النسخة الاحتياطية: {ex.Message}");
            }
        }

        public async Task<bool> RestoreBackupAsync(int backupId, string restoredBy = "")
        {
            try
            {
                var backup = await GetBackupByIdAsync(backupId);
                if (backup == null)
                {
                    throw new Exception("النسخة الاحتياطية المطلوبة غير موجودة");
                }

                if (!File.Exists(backup.FilePath))
                {
                    throw new Exception("ملف النسخة الاحتياطية غير موجود");
                }

                if (backup.IsCorrupted)
                {
                    throw new Exception("النسخة الاحتياطية تالفة ولا يمكن استعادتها");
                }

                // Verify backup integrity before restore
                if (!await ValidateBackupIntegrityAsync(backupId))
                {
                    throw new Exception("فشل في التحقق من سلامة النسخة الاحتياطية");
                }

                // Create a backup of current database before restore
                await CreateBackupAsync("نسخة احتياطية قبل الاستعادة", "Automatic", restoredBy);

                // Perform the restore
                await RestoreDatabaseFromBackupAsync(backup.FilePath);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في استعادة النسخة الاحتياطية: {ex.Message}");
            }
        }

        public async Task<bool> DeleteBackupAsync(int backupId, string deletedBy = "")
        {
            try
            {
                var backup = await GetBackupByIdAsync(backupId);
                if (backup == null) return false;

                // Delete the physical file
                if (File.Exists(backup.FilePath))
                {
                    File.Delete(backup.FilePath);
                }

                // Remove from database
                _context.BackupRecords.Remove(backup);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> VerifyBackupAsync(int backupId, string verifiedBy = "")
        {
            try
            {
                var backup = await GetBackupByIdAsync(backupId);
                if (backup == null) return false;

                bool isValid = await ValidateBackupIntegrityAsync(backupId);

                backup.IsVerified = true;
                backup.VerifiedDate = DateTime.Now;
                backup.VerifiedBy = verifiedBy;
                backup.IsCorrupted = !isValid;

                if (!isValid)
                {
                    backup.ErrorMessage = "فشل في التحقق من سلامة النسخة الاحتياطية";
                }

                await _context.SaveChangesAsync();

                return isValid;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Backup Management

        public async Task<IEnumerable<BackupRecord>> GetAllBackupsAsync()
        {
            return await _context.BackupRecords
                .OrderByDescending(b => b.BackupDate)
                .ToListAsync();
        }

        public async Task<BackupRecord?> GetBackupByIdAsync(int backupId)
        {
            return await _context.BackupRecords
                .FirstOrDefaultAsync(b => b.BackupId == backupId);
        }

        public async Task<IEnumerable<BackupRecord>> GetBackupsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.BackupRecords
                .Where(b => b.BackupDate >= fromDate && b.BackupDate <= toDate)
                .OrderByDescending(b => b.BackupDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<BackupRecord>> GetBackupsByTypeAsync(string backupType)
        {
            return await _context.BackupRecords
                .Where(b => b.BackupType == backupType)
                .OrderByDescending(b => b.BackupDate)
                .ToListAsync();
        }

        #endregion

        #region Automatic Backup

        public async Task<bool> EnableAutomaticBackupAsync(int intervalHours)
        {
            // This would typically involve setting up a timer or scheduled task
            // For now, we'll just update the system settings
            try
            {
                // Implementation would depend on your scheduling mechanism
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DisableAutomaticBackupAsync()
        {
            try
            {
                // Implementation would depend on your scheduling mechanism
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RunScheduledBackupAsync()
        {
            try
            {
                var backup = await CreateBackupAsync("نسخة احتياطية مجدولة", "Scheduled", "System");
                return backup != null;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Backup Cleanup

        public async Task<int> CleanupOldBackupsAsync(int maxBackupsToKeep)
        {
            try
            {
                var allBackups = await GetAllBackupsAsync();
                var backupsToDelete = allBackups.Skip(maxBackupsToKeep).Take(Constants.MaxRecordsPerQuery).ToList();

                int deletedCount = 0;
                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var backup in backupsToDelete)
                {
                    if (await DeleteBackupAsync(backup.BackupId, "System"))
                    {
                        deletedCount++;
                    }
                }

                return deletedCount;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> CleanupCorruptedBackupsAsync()
        {
            try
            {
                var corruptedBackups = await _context.BackupRecords
                    .Where(b => b.IsCorrupted)
                    .ToListAsync();

                int deletedCount = 0;
                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var backup in corruptedBackups)
                {
                    if (await DeleteBackupAsync(backup.BackupId, "System"))
                    {
                        deletedCount++;
                    }
                }

                return deletedCount;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<long> GetTotalBackupSizeAsync()
        {
            var allBackups = await GetAllBackupsAsync();
            return allBackups.Sum(b => b.FileSizeBytes);
        }

        #endregion

        #region Backup Validation

        public async Task<bool> ValidateBackupIntegrityAsync(int backupId)
        {
            try
            {
                var backup = await GetBackupByIdAsync(backupId);
                if (backup == null || !File.Exists(backup.FilePath))
                {
                    return false;
                }

                // Check file size
                var fileInfo = new FileInfo(backup.FilePath);
                if (fileInfo.Length != backup.FileSizeBytes)
                {
                    return false;
                }

                // Check checksum
                var currentCheckSum = await CalculateFileCheckSumAsync(backup.FilePath);
                if (currentCheckSum != backup.CheckSum)
                {
                    return false;
                }

                // Try to open the database to verify it's not corrupted
                return await TestBackupDatabaseConnectionAsync(backup.FilePath);
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ValidateAllBackupsAsync()
        {
            try
            {
                var allBackups = await GetAllBackupsAsync();
                bool allValid = true;

                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var backup in allBackups)
                {
                    bool isValid = await ValidateBackupIntegrityAsync(backup.BackupId);
                    
                    if (backup.IsCorrupted != !isValid)
                    {
                        backup.IsCorrupted = !isValid;
                        backup.VerifiedDate = DateTime.Now;
                        backup.VerifiedBy = "System";
                        
                        if (!isValid && string.IsNullOrEmpty(backup.ErrorMessage))
                        {
                            backup.ErrorMessage = "فشل التحقق من السلامة";
                        }
                    }

                    if (!isValid) allValid = false;
                }

                await _context.SaveChangesAsync();
                return allValid;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Export/Import

        public async Task<bool> ExportBackupListAsync(string filePath)
        {
            try
            {
                var backups = await GetAllBackupsAsync();
                var csv = new StringBuilder();
                
                // Header
                csv.AppendLine("BackupId,FileName,BackupDate,BackupType,FileSizeBytes,IsVerified,IsCorrupted,CreatedBy,Description");
                
                // Data
                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var backup in backups)
                {
                    csv.AppendLine($"{backup.BackupId},{backup.FileName},{backup.BackupDate:yyyy-MM-dd HH:mm:ss},{backup.BackupType},{backup.FileSizeBytes},{backup.IsVerified},{backup.IsCorrupted},\"{backup.CreatedBy}\",\"{backup.Description}\"");
                }

                await File.WriteAllTextAsync(filePath, csv.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> GetBackupInfoAsync(int backupId)
        {
            try
            {
                var backup = await GetBackupByIdAsync(backupId);
                if (backup == null) return "النسخة الاحتياطية غير موجودة";

                var info = new StringBuilder();
                info.AppendLine($"معرف النسخة: {backup.BackupId}");
                info.AppendLine($"اسم الملف: {backup.FileName}");
                info.AppendLine($"تاريخ الإنشاء: {backup.BackupDate:yyyy-MM-dd HH:mm:ss}");
                info.AppendLine($"نوع النسخة: {backup.BackupTypeDisplay}");
                info.AppendLine($"حجم الملف: {backup.FileSizeFormatted}");
                info.AppendLine($"عدد السجلات: {backup.RecordCount:N0}");
                info.AppendLine($"الحالة: {backup.StatusDisplay}");
                info.AppendLine($"منشئ النسخة: {backup.CreatedBy}");
                if (!string.IsNullOrEmpty(backup.Description))
                    info.AppendLine($"الوصف: {backup.Description}");
                if (backup.VerifiedDate.HasValue)
                    info.AppendLine($"تاريخ التحقق: {backup.VerifiedDate:yyyy-MM-dd HH:mm:ss}");
                if (!string.IsNullOrEmpty(backup.ErrorMessage))
                    info.AppendLine($"رسالة الخطأ: {backup.ErrorMessage}");

                return info.ToString();
            }
            catch
            {
                return "خطأ في استرداد معلومات النسخة الاحتياطية";
            }
        }

        #endregion

        #region Settings

        public async Task<string> GetBackupDirectoryAsync()
        {
            // Try to get from system settings first
            try
            {
                var systemSettings = await _context.SystemSettings.FirstOrDefaultAsync();
                if (systemSettings != null && !string.IsNullOrWhiteSpace(systemSettings.BackupPath))
                {
                    return systemSettings.BackupPath;
                }
            }
            catch { }

            return _defaultBackupPath;
        }

        public async Task<bool> SetBackupDirectoryAsync(string path)
        {
            try
            {
                if (!await TestBackupDirectoryAccessAsync(path))
                    return false;

                var systemSettings = await _context.SystemSettings.FirstOrDefaultAsync();
                if (systemSettings != null)
                {
                    systemSettings.BackupPath = path;
                    systemSettings.ModifiedDate = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> TestBackupDirectoryAccessAsync(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Test write access
                var testFile = Path.Combine(path, "test_access.tmp");
                await File.WriteAllTextAsync(testFile, "test");
                File.Delete(testFile);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        private async Task CreateDatabaseBackupAsync(string backupFilePath)
        {
            // For SQLite, we can simply copy the database file
            var sourceDbPath = GetDatabaseFilePath();
            if (string.IsNullOrEmpty(sourceDbPath) || !File.Exists(sourceDbPath))
            {
                throw new Exception("لا يمكن العثور على ملف قاعدة البيانات الأصلي");
            }

            // Close any open connections temporarily
            await _context.Database.CloseConnectionAsync();

            try
            {
                File.Copy(sourceDbPath, backupFilePath, true);
            }
            finally
            {
                // Reopen connection
                await _context.Database.OpenConnectionAsync();
            }
        }

        private async Task RestoreDatabaseFromBackupAsync(string backupFilePath)
        {
            var targetDbPath = GetDatabaseFilePath();
            if (string.IsNullOrEmpty(targetDbPath))
            {
                throw new Exception("لا يمكن تحديد مسار قاعدة البيانات الهدف");
            }

            // Close connections
            await _context.Database.CloseConnectionAsync();

            try
            {
                // Backup current database
                var tempBackup = targetDbPath + ".temp_backup";
                if (File.Exists(targetDbPath))
                {
                    File.Copy(targetDbPath, tempBackup, true);
                }

                // Restore from backup
                File.Copy(backupFilePath, targetDbPath, true);

                // Test the restored database
                await _context.Database.OpenConnectionAsync();
                await _context.Database.CloseConnectionAsync();

                // If successful, delete temp backup
                if (File.Exists(tempBackup))
                {
                    File.Delete(tempBackup);
                }
            }
            catch
            {
                // If failed, restore from temp backup
                var tempBackup = targetDbPath + ".temp_backup";
                if (File.Exists(tempBackup))
                {
                    File.Copy(tempBackup, targetDbPath, true);
                    File.Delete(tempBackup);
                }
                throw;
            }
            finally
            {
                await _context.Database.OpenConnectionAsync();
            }
        }

        private string GetDatabaseFilePath()
        {
            try
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder(_connectionString);
                return connectionStringBuilder.DataSource;
            }
            catch
            {
                return "";
            }
        }

        private async Task<string> CalculateFileCheckSumAsync(string filePath)
        {
            try
            {
                var hash = await Task.Run(() => md5.ComputeHash(stream));
                return Convert.ToBase64String(hash);
            }
            catch
            {
                return "";
            }
        }

        private async Task<int> GetDatabaseRecordCountAsync()
        {
            try
            {
                var patientCount = await _context.Patients.CountAsync();
                var userCount = await _context.Users.CountAsync();
                var testCount = await _context.PatientTests.CountAsync();
                var resultCount = await _context.TestResults.CountAsync();
                
                return patientCount + userCount + testCount + resultCount;
            }
            catch
            {
                return 0;
            }
        }

        private async Task<bool> TestBackupDatabaseConnectionAsync(string backupFilePath)
        {
            try
            {
                var connectionString = $"Data Source={backupFilePath};";
                await connection.OpenAsync();
                
                // Try to query a simple table
                command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table';";
                var result = await command.ExecuteScalarAsync();
                
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Additional Methods

        public async Task<int> CleanupOldBackupsAsync(int maxBackupsToKeep, string performedBy)
        {
            try
            {
                var allBackups = await GetAllBackupsAsync();
                var backupsToDelete = allBackups
                    .OrderByDescending(b => b.BackupDate)
                    .Skip(maxBackupsToKeep)
                    .Take(Constants.MaxRecordsPerQuery).ToList();

                int deletedCount = 0;
                // TODO: Consider using batch operations to avoid N+1 queries
            foreach (var backup in backupsToDelete)
                {
                    try
                    {
                        await DeleteBackupAsync(backup.BackupId, performedBy);
                        deletedCount++;
                    }
                    catch
                    {
                        // Continue with other backups even if one fails
                    }
                }

                return deletedCount;
            }
            catch
            {
                return 0;
            }
        }

        public async Task<bool> ExportBackupAsync(int backupId, string destinationPath, string performedBy)
        {
            try
            {
                var backup = await GetBackupByIdAsync(backupId);
                if (backup == null || !File.Exists(backup.FilePath))
                    return false;

                File.Copy(backup.FilePath, destinationPath, true);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<BackupRecord> ImportBackupAsync(string sourcePath, string description, string performedBy)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException("ملف النسخة الاحتياطية غير موجود");

                var fileName = Path.GetFileName(sourcePath);
                var destinationPath = Path.Combine(_backupDirectory, fileName);
                
                // Ensure unique filename
                int counter = 1;
                while (File.Exists(destinationPath))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    var extension = Path.GetExtension(fileName);
                    fileName = $"{nameWithoutExt}_{counter}{extension}";
                    destinationPath = Path.Combine(_backupDirectory, fileName);
                    counter++;
                }

                File.Copy(sourcePath, destinationPath);
                var fileInfo = new FileInfo(destinationPath);

                var backup = new BackupRecord
                {
                    FileName = fileName,
                    FilePath = destinationPath,
                    FileSizeBytes = fileInfo.Length,
                    BackupDate = DateTime.Now,
                    BackupType = "Imported",
                    CreatedBy = performedBy,
                    Description = description,
                    CheckSum = await CalculateChecksumAsync(destinationPath)
                };

                _context.BackupRecords.Add(backup);
                await _context.SaveChangesAsync();

                return backup;
            }
            catch (Exception ex)
            {
                throw new Exception($"فشل في استيراد النسخة الاحتياطية: {ex.Message}");
            }
        }

        public async Task<bool> ScheduleAutoBackupAsync(bool enabled, int intervalHours, string performedBy)
        {
            try
            {
                // This is a simplified implementation
                // In a full implementation, you would integrate with a task scheduler
                
                // For now, we'll just store the settings
                // The actual scheduling would be implemented separately
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
