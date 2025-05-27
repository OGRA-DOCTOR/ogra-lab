using System.Threading.Tasks;
using OGRALAB.Models;

namespace OGRALAB.Services
{
    public interface ISettingsService
    {
        // Lab Settings
        Task<LabSettings> GetLabSettingsAsync();
        Task<LabSettings> UpdateLabSettingsAsync(LabSettings labSettings);
        Task ResetLabSettingsToDefaultAsync();

        // System Settings
        Task<SystemSettings> GetSystemSettingsAsync();
        Task<SystemSettings> UpdateSystemSettingsAsync(SystemSettings systemSettings);
        Task ResetSystemSettingsToDefaultAsync();

        // Settings Validation
        Task<bool> ValidateLabSettingsAsync(LabSettings labSettings);
        Task<bool> ValidateSystemSettingsAsync(SystemSettings systemSettings);

        // Settings Export/Import
        Task<bool> ExportSettingsAsync(string filePath);
        Task<bool> ImportSettingsAsync(string filePath);

        // Settings History
        Task LogSettingsChangeAsync(string settingType, string fieldName, string oldValue, string newValue, string changedBy);
        
        // Apply Settings
        Task ApplySettingsAsync();
        Task<bool> RequiresRestartAsync();
    }
}
