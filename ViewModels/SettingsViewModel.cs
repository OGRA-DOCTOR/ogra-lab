using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Helpers;
using OGRALAB.Views;

namespace OGRALAB.ViewModels
{
    public class SettingsViewModel : INotifyPropertyChanged
    {
        private readonly ISettingsService _settingsService;
        private readonly IBackupService _backupService;
        private readonly IStatsService _statsService;
        private readonly IAuthenticationService _authenticationService;
        
        private LabSettings _labSettings;
        private SystemSettings _systemSettings;
        private ObservableCollection<BackupRecord> _backups;
        private bool _isLoading;
        private string _selectedTab = "LabSettings";
        private bool _hasUnsavedChanges;

        public SettingsViewModel(ISettingsService settingsService, IBackupService backupService, 
            IStatsService statsService, IAuthenticationService authenticationService)
        {
            _settingsService = settingsService;
            _backupService = backupService;
            _statsService = statsService;
            _authenticationService = authenticationService;
            
            _labSettings = new LabSettings();
            _systemSettings = new SystemSettings();
            _backups = new ObservableCollection<BackupRecord>();
            
            // Commands
            LoadSettingsCommand = new RelayCommand(async () => await LoadSettingsAsync());
            SaveLabSettingsCommand = new RelayCommand(async () => await SaveLabSettingsAsync(), CanSaveLabSettings);
            SaveSystemSettingsCommand = new RelayCommand(async () => await SaveSystemSettingsAsync(), CanSaveSystemSettings);
            ResetLabSettingsCommand = new RelayCommand(async () => await ResetLabSettingsAsync(), CanResetSettings);
            ResetSystemSettingsCommand = new RelayCommand(async () => await ResetSystemSettingsAsync(), CanResetSettings);
            
            // Backup Commands
            CreateBackupCommand = new RelayCommand(async () => await CreateBackupAsync(), CanCreateBackup);
            RestoreBackupCommand = new RelayCommand(async () => await RestoreBackupAsync(), CanRestoreBackup);
            DeleteBackupCommand = new RelayCommand(async () => await DeleteBackupAsync(), CanDeleteBackup);
            VerifyBackupCommand = new RelayCommand(async () => await VerifyBackupAsync(), CanVerifyBackup);
            RefreshBackupsCommand = new RelayCommand(async () => await LoadBackupsAsync());
            
            // Other Commands
            ExportSettingsCommand = new RelayCommand(async () => await ExportSettingsAsync(), CanExportSettings);
            ImportSettingsCommand = new RelayCommand(async () => await ImportSettingsAsync(), CanImportSettings);
            BrowseLogoCommand = new RelayCommand(BrowseLogo);
            BrowseBackupPathCommand = new RelayCommand(BrowseBackupPath);
            
            // Test Management Commands
            ManageTestTypesCommand = new RelayCommand(OpenTestTypesManagement, CanManageTestTypes);
            ManageUsersCommand = new RelayCommand(OpenUserManagement, CanManageUsers);
            ViewLoginLogsCommand = new RelayCommand(OpenLoginLogs, CanViewLoginLogs);
            ViewSystemStatsCommand = new RelayCommand(OpenSystemStats, CanViewSystemStats);
            
            // Languages and Themes
            AvailableLanguages = new[] { "العربية", "English" };
            AvailableThemes = new[] { "فاتح", "داكن", "تلقائي" };
            AvailableFonts = new[] { "Cairo", "Segoe UI", "Arial", "Tahoma" };
            
            // Load initial data
            _ = LoadSettingsAsync();
        }

        // Properties
        public LabSettings LabSettings
        {
            get => _labSettings;
            set
            {
                if (SetProperty(ref _labSettings, value))
                {
                    HasUnsavedChanges = true;
                    ((RelayCommand)SaveLabSettingsCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public SystemSettings SystemSettings
        {
            get => _systemSettings;
            set
            {
                if (SetProperty(ref _systemSettings, value))
                {
                    HasUnsavedChanges = true;
                    ((RelayCommand)SaveSystemSettingsCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<BackupRecord> Backups
        {
            get => _backups;
            set => SetProperty(ref _backups, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            set => SetProperty(ref _hasUnsavedChanges, value);
        }

        // Collections
        public string[] AvailableLanguages { get; }
        public string[] AvailableThemes { get; }
        public string[] AvailableFonts { get; }

        // Selected items
        private BackupRecord? _selectedBackup;
        public BackupRecord? SelectedBackup
        {
            get => _selectedBackup;
            set
            {
                SetProperty(ref _selectedBackup, value);
                ((RelayCommand)RestoreBackupCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteBackupCommand).RaiseCanExecuteChanged();
                ((RelayCommand)VerifyBackupCommand).RaiseCanExecuteChanged();
            }
        }

        // Display Properties
        public int BackupsCount => Backups.Count;
        public bool HasBackups => Backups.Any();

        // Permissions
        public bool CanManageSettings => _authenticationService.CurrentUser?.Role != null && 
                                        (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                         _authenticationService.CurrentUser.Role == "AdminUser");

        public bool CanManageBackups => CanManageSettings;

        // Commands
        public ICommand LoadSettingsCommand { get; }
        public ICommand SaveLabSettingsCommand { get; }
        public ICommand SaveSystemSettingsCommand { get; }
        public ICommand ResetLabSettingsCommand { get; }
        public ICommand ResetSystemSettingsCommand { get; }
        
        public ICommand CreateBackupCommand { get; }
        public ICommand RestoreBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand VerifyBackupCommand { get; }
        public ICommand RefreshBackupsCommand { get; }
        
        public ICommand ExportSettingsCommand { get; }
        public ICommand ImportSettingsCommand { get; }
        public ICommand BrowseLogoCommand { get; }
        public ICommand BrowseBackupPathCommand { get; }
        
        public ICommand ManageTestTypesCommand { get; }
        public ICommand ManageUsersCommand { get; }
        public ICommand ViewLoginLogsCommand { get; }
        public ICommand ViewSystemStatsCommand { get; }

        // Methods
        private async Task LoadSettingsAsync()
        {
            try
            {
                IsLoading = true;
                
                var labSettings = await _settingsService.GetLabSettingsAsync();
                var systemSettings = await _settingsService.GetSystemSettingsAsync();
                
                LabSettings = labSettings;
                SystemSettings = systemSettings;
                
                await LoadBackupsAsync();
                
                HasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الإعدادات: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveLabSettingsAsync()
        {
            if (!CanManageSettings)
            {
                MessageBox.Show("ليس لديك صلاحية لحفظ إعدادات المختبر", "تنبيه", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                
                LabSettings.ModifiedBy = _authenticationService.CurrentUser?.Username ?? "System";
                
                if (!await _settingsService.ValidateLabSettingsAsync(LabSettings))
                {
                    MessageBox.Show("البيانات المدخلة غير صحيحة، يرجى التحقق من جميع الحقول", "خطأ في البيانات", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updatedSettings = await _settingsService.UpdateLabSettingsAsync(LabSettings);
                LabSettings = updatedSettings;
                
                HasUnsavedChanges = false;
                
                MessageBox.Show("تم حفظ إعدادات المختبر بنجاح", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ إعدادات المختبر: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveSystemSettingsAsync()
        {
            if (!CanManageSettings)
            {
                MessageBox.Show("ليس لديك صلاحية لحفظ إعدادات النظام", "تنبيه", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;
                
                SystemSettings.ModifiedBy = _authenticationService.CurrentUser?.Username ?? "System";
                
                if (!await _settingsService.ValidateSystemSettingsAsync(SystemSettings))
                {
                    MessageBox.Show("البيانات المدخلة غير صحيحة، يرجى التحقق من جميع الحقول", "خطأ في البيانات", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var updatedSettings = await _settingsService.UpdateSystemSettingsAsync(SystemSettings);
                SystemSettings = updatedSettings;
                
                HasUnsavedChanges = false;
                
                MessageBox.Show("تم حفظ إعدادات النظام بنجاح", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                    
                // Apply settings immediately
                await _settingsService.ApplySettingsAsync();
                
                if (await _settingsService.RequiresRestartAsync())
                {
                    MessageBox.Show("بعض الإعدادات تتطلب إعادة تشغيل التطبيق لتطبيقها", "تنبيه", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ إعدادات النظام: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ResetLabSettingsAsync()
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إعادة تعيين إعدادات المختبر للقيم الافتراضية؟\nسيتم فقدان جميع التعديلات الحالية.",
                "تأكيد إعادة التعيين",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                
                await _settingsService.ResetLabSettingsToDefaultAsync();
                var defaultSettings = await _settingsService.GetLabSettingsAsync();
                LabSettings = defaultSettings;
                
                HasUnsavedChanges = false;
                
                MessageBox.Show("تم إعادة تعيين إعدادات المختبر للقيم الافتراضية", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إعادة تعيين إعدادات المختبر: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ResetSystemSettingsAsync()
        {
            var result = MessageBox.Show(
                "هل أنت متأكد من إعادة تعيين إعدادات النظام للقيم الافتراضية؟\nسيتم فقدان جميع التعديلات الحالية.",
                "تأكيد إعادة التعيين",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                
                await _settingsService.ResetSystemSettingsToDefaultAsync();
                var defaultSettings = await _settingsService.GetSystemSettingsAsync();
                SystemSettings = defaultSettings;
                
                HasUnsavedChanges = false;
                
                MessageBox.Show("تم إعادة تعيين إعدادات النظام للقيم الافتراضية", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إعادة تعيين إعدادات النظام: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Backup Methods
        private async Task LoadBackupsAsync()
        {
            try
            {
                var backups = await _backupService.GetAllBackupsAsync();
                Backups.Clear();
                foreach (var backup in backups)
                {
                    Backups.Add(backup);
                }
                
                OnPropertyChanged(nameof(BackupsCount));
                OnPropertyChanged(nameof(HasBackups));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل قائمة النسخ الاحتياطية: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CreateBackupAsync()
        {
            try
            {
                IsLoading = true;
                
                var backup = await _backupService.CreateBackupAsync(
                    "نسخة احتياطية يدوية من الإعدادات", 
                    "Manual", 
                    _authenticationService.CurrentUser?.Username ?? "System");
                
                Backups.Insert(0, backup);
                OnPropertyChanged(nameof(BackupsCount));
                OnPropertyChanged(nameof(HasBackups));
                
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

        private async Task RestoreBackupAsync()
        {
            if (SelectedBackup == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من استعادة النسخة الاحتياطية '{SelectedBackup.FileName}'؟\n" +
                "سيتم إنشاء نسخة احتياطية من البيانات الحالية قبل الاستعادة.\n" +
                "هذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الاستعادة",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                
                await _backupService.RestoreBackupAsync(
                    SelectedBackup.BackupId, 
                    _authenticationService.CurrentUser?.Username ?? "System");
                
                MessageBox.Show("تم استعادة النسخة الاحتياطية بنجاح\nسيتم إعادة تحميل البيانات", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Reload all data
                await LoadSettingsAsync();
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
                $"هل أنت متأكد من حذف النسخة الاحتياطية '{SelectedBackup.FileName}'؟\nهذا الإجراء لا يمكن التراجع عنه.",
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
                
                OnPropertyChanged(nameof(BackupsCount));
                OnPropertyChanged(nameof(HasBackups));
                
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

        // Other Methods
        private async Task ExportSettingsAsync()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
            saveFileDialog.FileName = $"OgraLab_Settings_{DateTime.Now:yyyyMMdd}.json";
            
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    
                    var success = await _settingsService.ExportSettingsAsync(saveFileDialog.FileName);
                    
                    if (success)
                    {
                        MessageBox.Show("تم تصدير الإعدادات بنجاح", "نجح", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في تصدير الإعدادات", "خطأ", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في تصدير الإعدادات: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ImportSettingsAsync()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
            
            if (openFileDialog.ShowDialog() == true)
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من استيراد الإعدادات؟\nسيتم استبدال الإعدادات الحالية.",
                    "تأكيد الاستيراد",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    IsLoading = true;
                    
                    var success = await _settingsService.ImportSettingsAsync(openFileDialog.FileName);
                    
                    if (success)
                    {
                        await LoadSettingsAsync();
                        MessageBox.Show("تم استيراد الإعدادات بنجاح", "نجح", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في استيراد الإعدادات", "خطأ", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في استيراد الإعدادات: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private void BrowseLogo()
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|All Files (*.*)|*.*";
            
            if (openFileDialog.ShowDialog() == true)
            {
                LabSettings.LogoPath = openFileDialog.FileName;
                OnPropertyChanged(nameof(LabSettings));
                HasUnsavedChanges = true;
                ((RelayCommand)SaveLabSettingsCommand).RaiseCanExecuteChanged();
            }
        }

        private void BrowseBackupPath()
        {
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderDialog.Description = "اختر مجلد النسخ الاحتياطية";
            
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                SystemSettings.BackupPath = folderDialog.SelectedPath;
                OnPropertyChanged(nameof(SystemSettings));
                HasUnsavedChanges = true;
                ((RelayCommand)SaveSystemSettingsCommand).RaiseCanExecuteChanged();
            }
        }

        // Navigation Methods
        private void OpenTestTypesManagement()
        {
            try
            {
                var testTypesWindow = new TestTypesManagementWindow();
                testTypesWindow.Owner = Application.Current.MainWindow;
                testTypesWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إدارة أنواع التحاليل: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenUserManagement()
        {
            try
            {
                var userManagementWindow = new UserManagementWindow();
                userManagementWindow.Owner = Application.Current.MainWindow;
                userManagementWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إدارة المستخدمين: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenLoginLogs()
        {
            try
            {
                var loginLogWindow = new LoginLogWindow();
                loginLogWindow.Owner = Application.Current.MainWindow;
                loginLogWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة سجل الدخول: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenSystemStats()
        {
            try
            {
                var systemStatsWindow = new SystemStatsWindow();
                systemStatsWindow.Owner = Application.Current.MainWindow;
                systemStatsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إحصائيات النظام: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Command Conditions
        private bool CanSaveLabSettings()
        {
            return CanManageSettings && LabSettings != null && !string.IsNullOrWhiteSpace(LabSettings.LabName);
        }

        private bool CanSaveSystemSettings()
        {
            return CanManageSettings && SystemSettings != null;
        }

        private bool CanResetSettings()
        {
            return CanManageSettings;
        }

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

        private bool CanExportSettings()
        {
            return CanManageSettings;
        }

        private bool CanImportSettings()
        {
            return CanManageSettings;
        }

        private bool CanManageTestTypes()
        {
            return _authenticationService.CurrentUser?.Role != null && 
                   (_authenticationService.CurrentUser.Role == "SystemUser" || 
                    _authenticationService.CurrentUser.Role == "AdminUser");
        }

        private bool CanManageUsers()
        {
            return _authenticationService.CurrentUser?.Role == "SystemUser";
        }

        private bool CanViewLoginLogs()
        {
            return _authenticationService.CurrentUser?.Role != null && 
                   (_authenticationService.CurrentUser.Role == "SystemUser" || 
                    _authenticationService.CurrentUser.Role == "AdminUser");
        }

        private bool CanViewSystemStats()
        {
            return _authenticationService.CurrentUser?.Role != null && 
                   (_authenticationService.CurrentUser.Role == "SystemUser" || 
                    _authenticationService.CurrentUser.Role == "AdminUser");
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
