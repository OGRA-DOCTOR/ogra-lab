using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using System;
using System.Threading.Tasks;

namespace OGRALAB.Helpers
{
    public static class DatabaseHelper
    {
        public static async Task<bool> TestConnectionAsync(string connectionString)
        {
            try
            {
                var options = new DbContextOptionsBuilder<OgraLabDbContext>()
                    .UseSqlite(connectionString)
                    .Options;

                await context.Database.CanConnectAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> CreateDatabaseAsync(string connectionString)
        {
            try
            {
                var options = new DbContextOptionsBuilder<OgraLabDbContext>()
                    .UseSqlite(connectionString)
                    .Options;

                await context.Database.EnsureCreatedAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> BackupDatabaseAsync(string sourcePath, string backupPath)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    return false;

                // Ensure backup directory exists
                var backupDirectory = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(backupDirectory) && !Directory.Exists(backupDirectory))
                {
                    Directory.CreateDirectory(backupDirectory);
                }

                // Copy database file
                await Task.Run(() => File.Copy(sourcePath, backupPath, true));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> RestoreDatabaseAsync(string backupPath, string targetPath)
        {
            try
            {
                if (!File.Exists(backupPath))
                    return false;

                // Ensure target directory exists
                var targetDirectory = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                // Copy backup file to target location
                await Task.Run(() => File.Copy(backupPath, targetPath, true));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string GetDatabasePath()
        {
            var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            return Path.Combine(dataDirectory, "OgraLab.db");
        }

        public static string GetBackupPath(DateTime? backupDate = null)
        {
            var date = backupDate ?? DateTime.Now;
            var backupDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backups");
            var fileName = $"OgraLab_Backup_{date:yyyyMMdd_HHmmss}.db";
            return Path.Combine(backupDirectory, fileName);
        }

        public static async Task<long> GetDatabaseSizeAsync(string databasePath)
        {
            try
            {
                if (!File.Exists(databasePath))
                    return 0;

                var fileInfo = new FileInfo(databasePath);
                return await Task.FromResult(fileInfo.Length);
            }
            catch
            {
                return 0;
            }
        }

        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double size = bytes;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }

            return $"{size:0.##} {sizes[order]}";
        }
    }
}
