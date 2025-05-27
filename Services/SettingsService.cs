using System;
using OGRALAB.Helpers;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;

namespace OGRALAB.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly OgraLabDbContext _context;
        private readonly string _settingsBackupPath;

        public SettingsService(OgraLabDbContext context)
        {
            _context = context;
            _settingsBackupPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings");
            
            // Ensure settings directory exists
            if (!Directory.Exists(_settingsBackupPath))
            {
                Directory.CreateDirectory(_settingsBackupPath);
            }
        }

        #region Lab Settings

        public async Task<LabSettings> GetLabSettingsAsync()
        {
            var settings = await _context.LabSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                // Create default settings if none exist
                settings = new LabSettings();
                _context.LabSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        public async Task<LabSettings> UpdateLabSettingsAsync(LabSettings labSettings)
        {
            var existingSettings = await _context.LabSettings.FirstOrDefaultAsync();
            
            if (existingSettings == null)
            {
                _context.LabSettings.Add(labSettings);
            }
            else
            {
                // Log changes before updating
                await LogLabSettingsChanges(existingSettings, labSettings);
                
                // Update existing settings
                existingSettings.LabName = labSettings.LabName;
                existingSettings.LabNameEnglish = labSettings.LabNameEnglish;
                existingSettings.Address = labSettings.Address;
                existingSettings.AddressEnglish = labSettings.AddressEnglish;
                existingSettings.Phone = labSettings.Phone;
                existingSettings.Mobile = labSettings.Mobile;
                existingSettings.Email = labSettings.Email;
                existingSettings.Website = labSettings.Website;
                existingSettings.LicenseNumber = labSettings.LicenseNumber;
                existingSettings.DirectorName = labSettings.DirectorName;
                existingSettings.DirectorTitle = labSettings.DirectorTitle;
                existingSettings.LogoPath = labSettings.LogoPath;
                existingSettings.ShowLogoInReports = labSettings.ShowLogoInReports;
                existingSettings.ShowHeaderInReports = labSettings.ShowHeaderInReports;
                existingSettings.ShowFooterInReports = labSettings.ShowFooterInReports;
                existingSettings.ReportFooterText = labSettings.ReportFooterText;
                existingSettings.ReportFooterTextEnglish = labSettings.ReportFooterTextEnglish;
                existingSettings.ModifiedDate = DateTime.Now;
                existingSettings.ModifiedBy = labSettings.ModifiedBy;
            }

            await _context.SaveChangesAsync();
            return existingSettings ?? labSettings;
        }

        public async Task ResetLabSettingsToDefaultAsync()
        {
            var existingSettings = await _context.LabSettings.FirstOrDefaultAsync();
            
            if (existingSettings != null)
            {
                _context.LabSettings.Remove(existingSettings);
            }

            var defaultSettings = new LabSettings();
            _context.LabSettings.Add(defaultSettings);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region System Settings

        public async Task<SystemSettings> GetSystemSettingsAsync()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                // Create default settings if none exist
                settings = new SystemSettings();
                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        public async Task<SystemSettings> UpdateSystemSettingsAsync(SystemSettings systemSettings)
        {
            var existingSettings = await _context.SystemSettings.FirstOrDefaultAsync();
            
            if (existingSettings == null)
            {
                _context.SystemSettings.Add(systemSettings);
            }
            else
            {
                // Log changes before updating
                await LogSystemSettingsChanges(existingSettings, systemSettings);
                
                // Update existing settings
                existingSettings.Language = systemSettings.Language;
                existingSettings.FontFamily = systemSettings.FontFamily;
                existingSettings.FontSize = systemSettings.FontSize;
                existingSettings.Theme = systemSettings.Theme;
                existingSettings.SessionTimeoutMinutes = systemSettings.SessionTimeoutMinutes;
                existingSettings.AutoBackupEnabled = systemSettings.AutoBackupEnabled;
                existingSettings.AutoBackupIntervalHours = systemSettings.AutoBackupIntervalHours;
                existingSettings.MaxBackupFiles = systemSettings.MaxBackupFiles;
                existingSettings.BackupPath = systemSettings.BackupPath;
                existingSettings.EnableAuditLog = systemSettings.EnableAuditLog;
                existingSettings.AuditLogRetentionDays = systemSettings.AuditLogRetentionDays;
                existingSettings.EnablePasswordComplexity = systemSettings.EnablePasswordComplexity;
                existingSettings.MinPasswordLength = systemSettings.MinPasswordLength;
                existingSettings.MaxLoginAttempts = systemSettings.MaxLoginAttempts;
                existingSettings.LoginLockoutMinutes = systemSettings.LoginLockoutMinutes;
                existingSettings.EnableDataValidation = systemSettings.EnableDataValidation;
                existingSettings.ShowConfirmationDialogs = systemSettings.ShowConfirmationDialogs;
                existingSettings.EnableAutoSave = systemSettings.EnableAutoSave;
                existingSettings.AutoSaveIntervalMinutes = systemSettings.AutoSaveIntervalMinutes;
                existingSettings.DefaultPrinter = systemSettings.DefaultPrinter;
                existingSettings.PrintHeaderFooter = systemSettings.PrintHeaderFooter;
                existingSettings.ReportMarginTop = systemSettings.ReportMarginTop;
                existingSettings.ReportMarginBottom = systemSettings.ReportMarginBottom;
                existingSettings.ReportMarginLeft = systemSettings.ReportMarginLeft;
                existingSettings.ReportMarginRight = systemSettings.ReportMarginRight;
                existingSettings.DateFormat = systemSettings.DateFormat;
                existingSettings.TimeFormat = systemSettings.TimeFormat;
                existingSettings.NumberFormat = systemSettings.NumberFormat;
                existingSettings.ModifiedDate = DateTime.Now;
                existingSettings.ModifiedBy = systemSettings.ModifiedBy;
            }

            await _context.SaveChangesAsync();
            return existingSettings ?? systemSettings;
        }

        public async Task ResetSystemSettingsToDefaultAsync()
        {
            var existingSettings = await _context.SystemSettings.FirstOrDefaultAsync();
            
            if (existingSettings != null)
            {
                _context.SystemSettings.Remove(existingSettings);
            }

            var defaultSettings = new SystemSettings();
            _context.SystemSettings.Add(defaultSettings);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Validation

        public async Task<bool> ValidateLabSettingsAsync(LabSettings labSettings)
        {
            if (labSettings == null) return false;
            
            // Validate required fields
            if (string.IsNullOrWhiteSpace(labSettings.LabName)) return false;
            
            // Validate email format if provided
            if (!string.IsNullOrWhiteSpace(labSettings.Email))
            {
                try
                {
                    var addr = new System.Net.Mail.MailAddress(labSettings.Email);
                    if (addr.Address != labSettings.Email) return false;
                }
                catch
                {
                    return false;
                }
            }

            // Validate logo path if provided
            if (!string.IsNullOrWhiteSpace(labSettings.LogoPath))
            {
                if (!File.Exists(labSettings.LogoPath)) return false;
            }

            return true;
        }

        public async Task<bool> ValidateSystemSettingsAsync(SystemSettings systemSettings)
        {
            if (systemSettings == null) return false;
            
            // Validate numeric ranges
            if (systemSettings.FontSize < Constants.MinPasswordLength || systemSettings.FontSize > 32) return false;
            if (systemSettings.SessionTimeoutMinutes < 5 || systemSettings.SessionTimeoutMinutes > 480) return false;
            if (systemSettings.AutoBackupIntervalHours < 1 || systemSettings.AutoBackupIntervalHours > 168) return false;
            if (systemSettings.MaxBackupFiles < 1 || systemSettings.MaxBackupFiles > Constants.CompletePercentage) return false;
            if (systemSettings.MinPasswordLength < 4 || systemSettings.MinPasswordLength > Constants.DefaultPageSize) return false;
            if (systemSettings.MaxLoginAttempts < 1 || systemSettings.MaxLoginAttempts > Constants.MaxConcurrentOperations) return false;

            // Validate backup path if provided
            if (!string.IsNullOrWhiteSpace(systemSettings.BackupPath))
            {
                if (!Directory.Exists(systemSettings.BackupPath)) return false;
            }

            return true;
        }

        #endregion

        #region Export/Import

        public async Task<bool> ExportSettingsAsync(string filePath)
        {
            try
            {
                var labSettings = await GetLabSettingsAsync();
                var systemSettings = await GetSystemSettingsAsync();

                var settingsExport = new
                {
                    ExportDate = DateTime.Now,
                    Version = "1.0",
                    LabSettings = labSettings,
                    SystemSettings = systemSettings
                };

                var json = JsonSerializer.Serialize(settingsExport, new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                await File.WriteAllTextAsync(filePath, json);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ImportSettingsAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return false;

                var json = await File.ReadAllTextAsync(filePath);
                var settingsImport = JsonSerializer.Deserialize<dynamic>(json);

                // This is a simplified implementation
                // In a real scenario, you would parse the JSON properly and update settings
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Logging

        public async Task LogSettingsChangeAsync(string settingType, string fieldName, string oldValue, string newValue, string changedBy)
        {
            // In a real implementation, you would save this to an audit log table
            // For now, we'll just add a simple log entry
            try
            {
                var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {settingType}.{fieldName} changed by {changedBy}: '{oldValue}' -> '{newValue}'";
                var logPath = Path.Combine(_settingsBackupPath, "settings_changes.log");
                await File.AppendAllTextAsync(logPath, logEntry + Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors
            }
        }

        private async Task LogLabSettingsChanges(LabSettings oldSettings, LabSettings newSettings)
        {
            var changedBy = newSettings.ModifiedBy ?? "System";

            if (oldSettings.LabName != newSettings.LabName)
                await LogSettingsChangeAsync("LabSettings", "LabName", oldSettings.LabName, newSettings.LabName, changedBy);
            
            if (oldSettings.Address != newSettings.Address)
                await LogSettingsChangeAsync("LabSettings", "Address", oldSettings.Address, newSettings.Address, changedBy);
            
            // Add more field comparisons as needed
        }

        private async Task LogSystemSettingsChanges(SystemSettings oldSettings, SystemSettings newSettings)
        {
            var changedBy = newSettings.ModifiedBy ?? "System";

            if (oldSettings.Language != newSettings.Language)
                await LogSettingsChangeAsync("SystemSettings", "Language", oldSettings.Language, newSettings.Language, changedBy);
            
            if (oldSettings.FontSize != newSettings.FontSize)
                await LogSettingsChangeAsync("SystemSettings", "FontSize", oldSettings.FontSize.ToString(), newSettings.FontSize.ToString(), changedBy);
            
            // Add more field comparisons as needed
        }

        #endregion

        #region Apply Settings

        public async Task ApplySettingsAsync()
        {
            // Apply settings that can be changed immediately
            var systemSettings = await GetSystemSettingsAsync();
            
            // Apply font settings (this would require UI updates)
            // Apply theme settings
            // Apply other immediate settings
            
            // Note: Some settings may require application restart
        }

        public async Task<bool> RequiresRestartAsync()
        {
            // Check if any settings that require restart have been changed
            // For now, return false, but in a real implementation you would track this
            return false;
        }

        #endregion
    }
}
