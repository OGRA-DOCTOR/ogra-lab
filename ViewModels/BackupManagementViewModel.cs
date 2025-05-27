using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Helpers;

namespace OGRALAB.ViewModels
{
    public class BackupManagementViewModel : INotifyPropertyChanged
    {
        private readonly IBackupService _backupService;
        private readonly IAuthenticationService _authenticationService;
        
        private ObservableCollection<BackupRecord> _backups;
        private BackupRecord? _selectedBackup;
        private bool _isLoading;
        private string _newBackupDescription = string.Empty;
        private bool _autoBackupEnabled;
        private int _autoBackupIntervalHours = Constants.AutoBackupIntervalHours;
        private int _maxBackupFiles = Constants.MaxConcurrentOperations;
        private string _backupPath = string.Empty;

        public BackupManagementViewModel(IBackupService backupService, IAuthenticationService authenticationService)
        {
            _backupService = backupService;
            _authenticationService = authenticationService;
            
            _backups = new ObservableCollection<BackupRecord>();
            
            // Commands
            LoadBackupsCommand = new RelayCommand(async () => await LoadBackupsAsync());
            CreateBackupCommand = new RelayCommand(async () => await CreateBackupAsync(), CanCreateBackup);
            CreateBackupWithDescriptionCommand = new RelayCommand(async () => await CreateBackupWithDescriptionAsync(), CanCreateBackup);
            RestoreBackupCommand = new RelayCommand(async () => await RestoreBackupAsync(), CanRestoreBackup);
            DeleteBackupCommand = new RelayCommand(async () => await DeleteBackupAsync(), CanDeleteBackup);
            VerifyBackupCommand = new RelayCommand(async () => await VerifyBackupAsync(), CanVerifyBackup);
            RefreshBackupsCommand = new RelayCommand(async () => await LoadBackupsAsync());
            CleanupOldBackupsCommand = new RelayCommand(async () => await CleanupOldBackupsAsync(), CanManageBackups);
            ExportBackupCommand = new RelayCommand(async () => await ExportBackupAsync(), CanExportBackup);
            ImportBackupCommand = new RelayCommand(async () => await ImportBackupAsync(), CanImportBackup);
            ScheduleBackupCommand = new RelayCommand(async () => await ScheduleBackupAsync(), CanManageBackups);
            
            // Load initial data
            _ = LoadBackupsAsync();
            _ = LoadBackupSettingsAsync();
        }

        // Properties
        public ObservableCollection<BackupRecord> Backups
        {
            get => _backups;
            set => SetProperty(ref _backups, value);
        }

        public BackupRecord? SelectedBackup
        {
            get => _selectedBackup;
            set
            {
                SetProperty(ref _selectedBackup, value);
                NotifyCommandsCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string NewBackupDescription
        {
            get => _newBackupDescription;
            set => SetProperty(ref _newBackupDescription, value);
        }

        public bool AutoBackupEnabled
        {
            get => _autoBackupEnabled;
            set
            {
                if (SetProperty(ref _autoBackupEnabled, value))
                {
                    _ = UpdateAutoBackupSettingsAsync();
                }
            }
        }

        public int AutoBackupIntervalHours
        {
            get => _autoBackupIntervalHours;
            set
            {
                if (SetProperty(ref _autoBackupIntervalHours, value))
                {
                    _ = UpdateAutoBackupSettingsAsync();
                }
            }
        }

        public int MaxBackupFiles
        {
            get => _maxBackupFiles;
            set
            {
                if (SetProperty(ref _maxBackupFiles, value))
                {
                    _ = UpdateAutoBackupSettingsAsync();
                }
            }
        }

        public string BackupPath
        {
            get => _backupPath;
            set => SetProperty(ref _backupPath, value);
        }

        // Display Properties
        public int BackupsCount => Backups.Count;
        public bool HasBackups => Backups.Any();
        public long TotalBackupSize => Backups.Sum(b => b.FileSize);
        public string TotalBackupSizeFormatted => FormatFileSize(TotalBackupSize);

        public BackupRecord? LatestBackup => Backups.OrderByDescending(b => b.BackupDate).FirstOrDefault();
        public BackupRecord? OldestBackup => Backups.OrderBy(b => b.BackupDate).FirstOrDefault();

        public int ValidBackupsCount => Backups.Count(b => !b.IsCorrupted);
        public int CorruptedBackupsCount => Backups.Count(b => b.IsCorrupted);

        // Permissions
        public bool CanManageBackups => _authenticationService.CurrentUser?.Role != null && 
                                       (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                        _authenticationService.CurrentUser.Role == "AdminUser");

        // Commands
        public ICommand LoadBackupsCommand { get; }
        public ICommand CreateBackupCommand { get; }
        public ICommand CreateBackupWithDescriptionCommand { get; }
        public ICommand RestoreBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand VerifyBackupCommand { get; }
        public ICommand RefreshBackupsCommand { get; }
        public ICommand CleanupOldBackupsCommand { get; }
        public ICommand ExportBackupCommand { get; }
        public ICommand ImportBackupCommand { get; }
        public ICommand ScheduleBackupCommand { get; }

        // Methods
        private async Task LoadBackupsAsync()
        {
            try
            {
                IsLoading = true;
                
                var backups = await _backupService.GetAllBackupsAsync();
                
                Backups.Clear();
                foreach (var backup in backups.OrderByDescending(b => b.BackupDate))
                {
                    Backups.Add(backup);
                }
                
                NotifyCollectionPropertiesChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل قائمة النسخ الاحتياطية: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadBackupSettingsAsync()
        {
            try
            {
                // This would typically load from system settings
                // For now, we'll use default values
                AutoBackupEnabled = true;
                AutoBackupIntervalHours = Constants.AutoBackupIntervalHours;
                MaxBackupFiles = Constants.MaxConcurrentOperations;
                BackupPath = @"C:\OgraLab\Backups";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل إعدادات النسخ الاحتياطي: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CreateBackupAsync()
        {
            try
            {
                IsLoading = true;
                
                var description = string.IsNullOrWhiteSpace(NewBackupDescription) 
                    ? "نسخة احتياطية يدوية" 
                    : NewBackupDescription;
                    
                var backup = await _backupService.CreateBackupAsync(
                    description, 
                    "Manual", 
                    _authenticationService.CurrentUser?.Username ?? "System");
                
                Backups.Insert(0, backup);
                NewBackupDescription = string.Empty;
                
                NotifyCollectionPropertiesChanged();
                
                MessageBox.Show("تم إنشاء النسخة الاحتياطية بنجاح", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء النسخة الاحتياطية: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateBackupWithDescriptionAsync()
        {
            // For now, use a simple prompt. In a full implementation, 
            // you would create a custom dialog window
            var result = MessageBox.Show(
                "هل تريد إضافة وصف مخصص للنسخة الاحتياطية؟",
                "وصف النسخة الاحتياطية",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                NewBackupDescription = $"نسخة احتياطية مخصصة - {DateTime.Now:yyyy-MM-dd HH:mm}";
            }
            else
            {
                NewBackupDescription = "نسخة احتياطية يدوية";
            }
            
            await CreateBackupAsync();
        }

        private async Task RestoreBackupAsync()
        {
            if (SelectedBackup == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من استعادة النسخة الاحتياطية '{SelectedBackup.FileName}'؟\n" +
                "سيتم إنشاء نسخة احتياطية من البيانات الحالية قبل الاستعادة.\n" +
                "هذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الاستعادة",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                
                await _backupService.RestoreBackupAsync(
                    SelectedBackup.BackupId, 
                    _authenticationService.CurrentUser?.Username ?? "System");
                
                MessageBox.Show("تم استعادة النسخة الاحتياطية بنجاح\nسيتم إعادة تشغيل التطبيق", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Request application restart
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في استعادة النسخة الاحتياطية: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteBackupAsync()
        {
            if (SelectedBackup == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف النسخة الاحتياطية '{SelectedBackup.FileName}'؟\n" +
                "هذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                
                await _backupService.DeleteBackupAsync(
                    SelectedBackup.BackupId, 
                    _authenticationService.CurrentUser?.Username ?? "System");
                
                Backups.Remove(SelectedBackup);
                SelectedBackup = null;
                
                NotifyCollectionPropertiesChanged();
                
                MessageBox.Show("تم حذف النسخة الاحتياطية بنجاح", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حذف النسخة الاحتياطية: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task VerifyBackupAsync()
        {
            if (SelectedBackup == null) return;

            try
            {
                IsLoading = true;
                
                var isValid = await _backupService.VerifyBackupAsync(
                    SelectedBackup.BackupId, 
                    _authenticationService.CurrentUser?.Username ?? "System");
                
                if (isValid)
                {
                    MessageBox.Show("النسخة الاحتياطية سليمة وقابلة للاستعادة", "نجح التحقق", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("النسخة الاحتياطية تالفة أو غير قابلة للاستعادة", "فشل التحقق", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                
                // Refresh the backup to show updated status
                await LoadBackupsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التحقق من النسخة الاحتياطية: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CleanupOldBackupsAsync()
        {
            var result = MessageBox.Show(
                $"هل تريد حذف النسخ الاحتياطية الزائدة عن {MaxBackupFiles} نسخة؟\n" +
                "سيتم الاحتفاظ بأحدث النسخ فقط.",
                "تنظيف النسخ الاحتياطية",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                
                var deletedCount = await _backupService.CleanupOldBackupsAsync(
                    MaxBackupFiles, 
                    _authenticationService.CurrentUser?.Username ?? "System");
                
                await LoadBackupsAsync();
                
                MessageBox.Show($"تم حذف {deletedCount} نسخة احتياطية قديمة", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تنظيف النسخ الاحتياطية: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportBackupAsync()
        {
            if (SelectedBackup == null) return;

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Backup Files (*.bak)|*.bak|All Files (*.*)|*.*";
            saveFileDialog.FileName = SelectedBackup.FileName;
            
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    
                    var success = await _backupService.ExportBackupAsync(
                        SelectedBackup.BackupId, 
                        saveFileDialog.FileName,
                        _authenticationService.CurrentUser?.Username ?? "System");
                    
                    if (success)
                    {
                        MessageBox.Show("تم تصدير النسخة الاحتياطية بنجاح", "نجح", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في تصدير النسخة الاحتياطية", "خطأ", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في تصدير النسخة الاحتياطية: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ImportBackupAsync()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Backup Files (*.bak)|*.bak|All Files (*.*)|*.*";
            
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    
                    var backup = await _backupService.ImportBackupAsync(
                        openFileDialog.FileName,
                        "نسخة احتياطية مستوردة",
                        _authenticationService.CurrentUser?.Username ?? "System");
                    
                    Backups.Insert(0, backup);
                    NotifyCollectionPropertiesChanged();
                    
                    MessageBox.Show("تم استيراد النسخة الاحتياطية بنجاح", "نجح", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في استيراد النسخة الاحتياطية: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ScheduleBackupAsync()
        {
            try
            {
                var success = await _backupService.ScheduleAutoBackupAsync(
                    AutoBackupEnabled, 
                    AutoBackupIntervalHours,
                    _authenticationService.CurrentUser?.Username ?? "System");
                
                if (success)
                {
                    var message = AutoBackupEnabled 
                        ? $"تم تفعيل النسخ الاحتياطي التلقائي كل {AutoBackupIntervalHours} ساعة"
                        : "تم إلغاء النسخ الاحتياطي التلقائي";
                        
                    MessageBox.Show(message, "نجح", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("فشل في تحديث جدولة النسخ الاحتياطي", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في جدولة النسخ الاحتياطي: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateAutoBackupSettingsAsync()
        {
            // This would typically save to system settings
            // For now, we'll just schedule the backup
            await ScheduleBackupAsync();
        }

        // Helper Methods
        private void NotifyCollectionPropertiesChanged()
        {
            OnPropertyChanged(nameof(BackupsCount));
            OnPropertyChanged(nameof(HasBackups));
            OnPropertyChanged(nameof(TotalBackupSize));
            OnPropertyChanged(nameof(TotalBackupSizeFormatted));
            OnPropertyChanged(nameof(LatestBackup));
            OnPropertyChanged(nameof(OldestBackup));
            OnPropertyChanged(nameof(ValidBackupsCount));
            OnPropertyChanged(nameof(CorruptedBackupsCount));
        }

        private void NotifyCommandsCanExecuteChanged()
        {
            ((RelayCommand)RestoreBackupCommand).RaiseCanExecuteChanged();
            ((RelayCommand)DeleteBackupCommand).RaiseCanExecuteChanged();
            ((RelayCommand)VerifyBackupCommand).RaiseCanExecuteChanged();
            ((RelayCommand)ExportBackupCommand).RaiseCanExecuteChanged();
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        // Command Conditions
        private bool CanCreateBackup()
        {
            return CanManageBackups;
        }

        private bool CanRestoreBackup()
        {
            return CanManageBackups && SelectedBackup != null && !SelectedBackup.IsCorrupted;
        }

        private bool CanDeleteBackup()
        {
            return CanManageBackups && SelectedBackup != null;
        }

        private bool CanVerifyBackup()
        {
            return CanManageBackups && SelectedBackup != null;
        }

        private bool CanExportBackup()
        {
            return CanManageBackups && SelectedBackup != null && !SelectedBackup.IsCorrupted;
        }

        private bool CanImportBackup()
        {
            return CanManageBackups;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
