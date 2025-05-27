using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using OGRALAB.Services;
using OGRALAB.Models;
using OGRALAB.Views;
using OGRALAB.Helpers;
using OGRALAB.UserControls;
using System.Windows;

namespace OGRALAB.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IPatientService _patientService;
        private readonly ITestService _testService;
        private User? _currentUser;
        private int _totalPatientsCount;
        private int _activeTestsCount;
        private int _pendingTestsCount;
        private int _completedTodayCount;
        private string _currentSection = string.Empty;
        private UserControl? _currentSectionContent;
        private string _statusMessage = "جاهز";
        private string _databaseStatus = "متصل";

        public MainViewModel(
            IAuthenticationService authenticationService,
            IPatientService patientService,
            ITestService testService)
        {
            _authenticationService = authenticationService;
            _patientService = patientService;
            _testService = testService;

            // Commands
            LogoutCommand = new RelayCommand(Logout);
            ShowLoginLogsCommand = new RelayCommand(ShowLoginLogs, CanShowLoginLogs);
            AddPatientCommand = new RelayCommand(AddPatient, CanAddPatient);
            SearchPatientCommand = new RelayCommand(SearchPatient, CanSearchPatient);
            ManageUsersCommand = new RelayCommand(ManageUsers, CanManageUsers);
            NavigateCommand = new RelayCommand<string>(Navigate);
            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            OpenTestDataManagementCommand = new RelayCommand(OpenTestDataManagement, CanViewAdminFeatures);
            OpenPerformanceMonitorCommand = new RelayCommand(OpenPerformanceMonitor, CanViewAdminFeatures);

            LoadData();
        }

        // Properties
        public User? CurrentUser
        {
            get => _currentUser ?? _authenticationService.CurrentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string CurrentUserDisplayName => CurrentUser?.FullName ?? "غير محدد";
        public string CurrentUserRole => CurrentUser?.Role ?? "غير محدد";
        
        // Role-based visibility properties
        public bool CanViewUserManagement => CurrentUser?.Role == "SystemUser";
        public bool CanViewLoginLogs => CurrentUser?.Role == "SystemUser";
        public bool CanDeletePatients => CurrentUser?.Role == "SystemUser" || CurrentUser?.Role == "AdminUser";
        public bool CanAddPatients => true; // All users can add patients
        public bool CanEditPatients => true; // All users can edit patients
        public bool CanViewReports => CurrentUser?.Role == "SystemUser" || CurrentUser?.Role == "AdminUser";
        public bool CanManageTests => true; // All users can manage tests
        public bool CanViewAdminFeatures => CurrentUser?.Role == "Manager"; // Only Manager can access admin features

        // Dashboard Statistics
        public int TotalPatientsCount
        {
            get => _totalPatientsCount;
            set => SetProperty(ref _totalPatientsCount, value);
        }

        public int ActiveTestsCount
        {
            get => _activeTestsCount;
            set => SetProperty(ref _activeTestsCount, value);
        }

        public int PendingTestsCount
        {
            get => _pendingTestsCount;
            set => SetProperty(ref _pendingTestsCount, value);
        }

        public int CompletedTodayCount
        {
            get => _completedTodayCount;
            set => SetProperty(ref _completedTodayCount, value);
        }

        public string CurrentSection
        {
            get => _currentSection;
            set => SetProperty(ref _currentSection, value);
        }

        public UserControl? CurrentSectionContent
        {
            get => _currentSectionContent;
            set => SetProperty(ref _currentSectionContent, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public string DatabaseStatus
        {
            get => _databaseStatus;
            set => SetProperty(ref _databaseStatus, value);
        }

        // Commands
        public ICommand LogoutCommand { get; }
        public ICommand ShowLoginLogsCommand { get; }
        public ICommand AddPatientCommand { get; }
        public ICommand SearchPatientCommand { get; }
        public ICommand ManageUsersCommand { get; }
        public ICommand NavigateCommand { get; }
        public ICommand LoadDataCommand { get; }
        public ICommand OpenTestDataManagementCommand { get; }
        public ICommand OpenPerformanceMonitorCommand { get; }

        private async void LoadData()
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                StatusMessage = "جاري تحميل البيانات...";
                CurrentUser = _authenticationService.CurrentUser;
                
                // Load dashboard statistics
                TotalPatientsCount = await _patientService.GetTotalPatientsCountAsync();
                ActiveTestsCount = await _testService.GetActiveTestsCountAsync();
                PendingTestsCount = await _testService.GetPendingTestsCountAsync();
                CompletedTodayCount = await _testService.GetCompletedTodayTestsCountAsync();

                // Notify all role-based properties changed
                OnPropertyChanged(nameof(CanViewUserManagement));
                OnPropertyChanged(nameof(CanViewLoginLogs));
                OnPropertyChanged(nameof(CanDeletePatients));
                OnPropertyChanged(nameof(CanAddPatients));
                OnPropertyChanged(nameof(CanEditPatients));
                OnPropertyChanged(nameof(CanViewReports));
                OnPropertyChanged(nameof(CanManageTests));
                OnPropertyChanged(nameof(CanViewAdminFeatures));
                OnPropertyChanged(nameof(CurrentUserDisplayName));
                OnPropertyChanged(nameof(CurrentUserRole));
                
                StatusMessage = "جاهز";
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في تحميل البيانات";
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Logout()
        {
            try
            {
                if (CurrentUser != null)
                {
                    await _authenticationService.LogoutAsync(CurrentUser.UserId);
                }

                // Close current window and show login
                foreach (Window window in Application.Current.Windows)
                {
                    if (window is MainWindow)
                    {
                        window.Close();
                        break;
                    }
                }

                // Show login window
                var app = (App)Application.Current;
                app.ShowLoginWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تسجيل الخروج: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowLoginLogs()
        {
            try
            {
                var loginLogWindow = new LoginLogWindow();
                loginLogWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض سجل الدخول: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanShowLoginLogs()
        {
            return CanViewLoginLogs;
        }

        private void AddPatient()
        {
            // TODO: Implement add patient window
            MessageBox.Show("سيتم تنفيذ هذه الوظيفة في المرحلة التالية", "قيد التطوير", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanAddPatient()
        {
            return CanAddPatients;
        }

        private void SearchPatient()
        {
            // TODO: Implement search patient window
            MessageBox.Show("سيتم تنفيذ هذه الوظيفة في المرحلة التالية", "قيد التطوير", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CanSearchPatient()
        {
            return true; // All users can search
        }

        private void ManageUsers()
        {
            try
            {
                var userManagementWindow = new UserManagementWindow();
                userManagementWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إدارة المستخدمين: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanManageUsers()
        {
            return CanViewUserManagement;
        }

        private void OpenTestDataManagement()
        {
            try
            {
                var testDataWindow = new Views.TestDataManagementWindow();
                testDataWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إدارة البيانات التجريبية: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenPerformanceMonitor()
        {
            try
            {
                var performanceWindow = new Views.PerformanceMonitorWindow();
                performanceWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة مراقب الأداء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanViewAdminFeatures()
        {
            return CurrentUser?.Role == "Manager";
        }

        private void Navigate(string section)
        {
            try
            {
                StatusMessage = $"التنقل إلى {GetSectionDisplayName(section)}...";
                CurrentSection = section;
                
                // Clear current content
                CurrentSectionContent = null;
                
                // Create appropriate UserControl based on section
                switch (section?.ToLower())
                {
                    case "patientmanagement":
                        CurrentSectionContent = new PatientManagementControl();
                        StatusMessage = $"تم التنقل إلى {GetSectionDisplayName(section)}";
                        break;
                        
                    case "searchedit":
                        CurrentSectionContent = new SearchEditControl();
                        StatusMessage = $"تم التنقل إلى {GetSectionDisplayName(section)}";
                        break;
                        
                    case "resultentry":
                        CurrentSectionContent = new ResultEntryControl();
                        StatusMessage = $"تم التنقل إلى {GetSectionDisplayName(section)}";
                        break;
                        
                    case "reportview":
                        CurrentSectionContent = new ReportViewControl();
                        StatusMessage = $"تم التنقل إلى {GetSectionDisplayName(section)}";
                        break;
                        
                    case "settings":
                        CurrentSectionContent = new SettingsControl();
                        StatusMessage = $"تم التنقل إلى {GetSectionDisplayName(section)}";
                        break;
                        
                    default:
                        CurrentSection = string.Empty;
                        CurrentSectionContent = null;
                        StatusMessage = "جاهز";
                        break;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "خطأ في التنقل";
                MessageBox.Show($"خطأ في التنقل: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetSectionDisplayName(string section)
        {
            return section?.ToLower() switch
            {
                "patientmanagement" => "إدارة المرضى",
                "searchedit" => "البحث والتعديل",
                "resultentry" => "إدخال النتائج",
                "reportview" => "معاينة التقارير",
                "settings" => "الإعدادات",
                _ => "الصفحة الرئيسية"
            };
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
