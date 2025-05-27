using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Helpers;
using OGRALAB.Services;

namespace OGRALAB.ViewModels
{
    public class TestDataManagementViewModel : INotifyPropertyChanged
    {
        private readonly OgraLabDbContext _context;
        private readonly ITestDataService _testDataService;
        private readonly ILoggingService _loggingService;
        private readonly IAuthenticationService _authenticationService;

        #region Private Fields

        private bool _isLoading;
        private string _loadingMessage = "";
        private int _totalUsers;
        private int _totalPatients;
        private int _totalTestTypes;
        private int _totalTestGroups;
        private int _totalPatientTests;
        private int _totalTestResults;
        private int _sampleDataCount;
        private DateTime _lastUpdateTime;
        private bool _hasMessages;

        #endregion

        #region Public Properties

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public string LoadingMessage
        {
            get => _loadingMessage;
            set
            {
                _loadingMessage = value;
                OnPropertyChanged();
            }
        }

        public int TotalUsers
        {
            get => _totalUsers;
            set
            {
                _totalUsers = value;
                OnPropertyChanged();
            }
        }

        public int TotalPatients
        {
            get => _totalPatients;
            set
            {
                _totalPatients = value;
                OnPropertyChanged();
            }
        }

        public int TotalTestTypes
        {
            get => _totalTestTypes;
            set
            {
                _totalTestTypes = value;
                OnPropertyChanged();
            }
        }

        public int TotalTestGroups
        {
            get => _totalTestGroups;
            set
            {
                _totalTestGroups = value;
                OnPropertyChanged();
            }
        }

        public int TotalPatientTests
        {
            get => _totalPatientTests;
            set
            {
                _totalPatientTests = value;
                OnPropertyChanged();
            }
        }

        public int TotalTestResults
        {
            get => _totalTestResults;
            set
            {
                _totalTestResults = value;
                OnPropertyChanged();
            }
        }

        public int SampleDataCount
        {
            get => _sampleDataCount;
            set
            {
                _sampleDataCount = value;
                OnPropertyChanged();
            }
        }

        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set
            {
                _lastUpdateTime = value;
                OnPropertyChanged();
            }
        }

        public bool HasMessages
        {
            get => _hasMessages;
            set
            {
                _hasMessages = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<StatusMessage> Messages { get; set; }

        #endregion

        #region Commands

        public ICommand CreateSampleUsersCommand { get; }
        public ICommand CreateSampleTestTypesCommand { get; }
        public ICommand CreateSampleTestGroupsCommand { get; }
        public ICommand CreateSamplePatientsCommand { get; }
        public ICommand CreateSamplePatientTestsCommand { get; }
        public ICommand CreateSampleTestResultsCommand { get; }
        public ICommand CreateFullSampleDataCommand { get; }
        public ICommand CreateDemoScenarioCommand { get; }
        public ICommand CreatePerformanceTestDataCommand { get; }
        public ICommand CreateReportingTestDataCommand { get; }
        public ICommand ValidateDataIntegrityCommand { get; }
        public ICommand GetDataSummaryCommand { get; }
        public ICommand RefreshStatisticsCommand { get; }
        public ICommand ClearSampleDataCommand { get; }
        public ICommand ClearAllDataCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand SaveLogCommand { get; }
        public ICommand ClearMessagesCommand { get; }

        #endregion

        #region Constructor

        public TestDataManagementViewModel(
            OgraLabDbContext context,
            ITestDataService testDataService,
            ILoggingService loggingService,
            IAuthenticationService authenticationService)
        {
            _context = context;
            _testDataService = testDataService;
            _loggingService = loggingService;
            _authenticationService = authenticationService;

            Messages = new ObservableCollection<StatusMessage>();

            // Initialize commands
            CreateSampleUsersCommand = new RelayCommand(async () => await CreateSampleDataAsync("Users"));
            CreateSampleTestTypesCommand = new RelayCommand(async () => await CreateSampleDataAsync("TestTypes"));
            CreateSampleTestGroupsCommand = new RelayCommand(async () => await CreateSampleDataAsync("TestGroups"));
            CreateSamplePatientsCommand = new RelayCommand(async () => await CreateSampleDataAsync("Patients"));
            CreateSamplePatientTestsCommand = new RelayCommand(async () => await CreateSampleDataAsync("PatientTests"));
            CreateSampleTestResultsCommand = new RelayCommand(async () => await CreateSampleDataAsync("TestResults"));
            CreateFullSampleDataCommand = new RelayCommand(async () => await CreateSampleDataAsync("Full"));
            CreateDemoScenarioCommand = new RelayCommand(async () => await CreateSampleDataAsync("Demo"));
            CreatePerformanceTestDataCommand = new RelayCommand(async () => await CreateSampleDataAsync("Performance"));
            CreateReportingTestDataCommand = new RelayCommand(async () => await CreateSampleDataAsync("Reporting"));
            ValidateDataIntegrityCommand = new RelayCommand(async () => await ValidateDataIntegrityAsync());
            GetDataSummaryCommand = new RelayCommand(async () => await GetDataSummaryAsync());
            RefreshStatisticsCommand = new RelayCommand(async () => await LoadStatisticsAsync());
            ClearSampleDataCommand = new RelayCommand(async () => await ClearSampleDataAsync());
            ClearAllDataCommand = new RelayCommand(async () => await ClearAllDataAsync());
            RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
            SaveLogCommand = new RelayCommand(async () => await SaveLogAsync());
            ClearMessagesCommand = new RelayCommand(() => ClearMessages());

            LastUpdateTime = DateTime.Now;
        }

        #endregion

        #region Public Methods

        public async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "جاري تحميل البيانات...";

                await LoadStatisticsAsync();
                
                LastUpdateTime = DateTime.Now;
                AddMessage("✅ تم تحميل البيانات بنجاح", MessageType.Success);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحميل بيانات إدارة البيانات التجريبية", ex, "TestDataManagement");
                AddMessage($"❌ خطأ في تحميل البيانات: {ex.Message}", MessageType.Error);
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = "";
            }
        }

        public void Cleanup()
        {
            // Cleanup resources if needed
        }

        #endregion

        #region Private Methods

        private async Task LoadStatisticsAsync()
        {
            TotalUsers = await _context.Users.CountAsync();
            TotalPatients = await _context.Patients.CountAsync();
            TotalTestTypes = await _context.TestTypes.CountAsync();
            TotalTestGroups = await _context.TestGroups.CountAsync();
            TotalPatientTests = await _context.PatientTests.CountAsync();
            TotalTestResults = await _context.TestResults.CountAsync();

            // Count sample data (created by TestDataService)
            var sampleUsers = await _context.Users.CountAsync(u => u.CreatedBy == "TestDataService");
            var samplePatients = await _context.Patients.CountAsync(p => p.CreatedBy == "TestDataService");
            var sampleTestTypes = await _context.TestTypes.CountAsync(tt => tt.CreatedBy == "TestDataService");
            var sampleTestGroups = await _context.TestGroups.CountAsync(tg => tg.CreatedBy == "TestDataService");
            var samplePatientTests = await _context.PatientTests.CountAsync(pt => pt.CreatedBy == "TestDataService");
            var sampleTestResults = await _context.TestResults.CountAsync(tr => tr.CreatedBy == "TestDataService");

            SampleDataCount = sampleUsers + samplePatients + sampleTestTypes + 
                              sampleTestGroups + samplePatientTests + sampleTestResults;
        }

        private async Task CreateSampleDataAsync(string dataType)
        {
            if (!CheckPermission()) return;

            try
            {
                IsLoading = true;
                bool success = false;

                switch (dataType)
                {
                    case "Users":
                        LoadingMessage = "جاري إنشاء المستخدمين التجريبيين...";
                        success = await _testDataService.CreateSampleUsersAsync();
                        break;

                    case "TestTypes":
                        LoadingMessage = "جاري إنشاء أنواع التحاليل التجريبية...";
                        success = await _testDataService.CreateSampleTestTypesAsync();
                        break;

                    case "TestGroups":
                        LoadingMessage = "جاري إنشاء مجموعات التحاليل التجريبية...";
                        success = await _testDataService.CreateSampleTestGroupsAsync();
                        break;

                    case "Patients":
                        LoadingMessage = "جاري إنشاء المرضى التجريبيين...";
                        success = await _testDataService.CreateSamplePatientsAsync(Constants.DefaultPageSize);
                        break;

                    case "PatientTests":
                        LoadingMessage = "جاري إنشاء تحاليل المرضى التجريبية...";
                        success = await _testDataService.CreateSamplePatientTestsAsync(150);
                        break;

                    case "TestResults":
                        LoadingMessage = "جاري إنشاء نتائج التحاليل التجريبية...";
                        success = await _testDataService.CreateSampleTestResultsAsync();
                        break;

                    case "Full":
                        LoadingMessage = "جاري إنشاء البيانات التجريبية الكاملة...";
                        success = await _testDataService.CreateFullSampleDataAsync();
                        break;

                    case "Demo":
                        LoadingMessage = "جاري إنشاء سيناريو العرض التوضيحي...";
                        success = await _testDataService.CreateDemoScenarioAsync();
                        break;

                    case "Performance":
                        LoadingMessage = "جاري إنشاء بيانات اختبار الأداء...";
                        success = await _testDataService.CreatePerformanceTestDataAsync();
                        break;

                    case "Reporting":
                        LoadingMessage = "جاري إنشاء بيانات اختبار التقارير...";
                        success = await _testDataService.CreateReportingTestDataAsync();
                        break;
                }

                if (success)
                {
                    AddMessage($"✅ تم إنشاء {GetDataTypeArabicName(dataType)} بنجاح", MessageType.Success);
                    await LoadStatisticsAsync();
                }
                else
                {
                    AddMessage($"⚠️ فشل في إنشاء {GetDataTypeArabicName(dataType)}", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"خطأ في إنشاء البيانات التجريبية: {dataType}", ex, "TestDataManagement");
                AddMessage($"❌ خطأ في إنشاء {GetDataTypeArabicName(dataType)}: {ex.Message}", MessageType.Error);
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = "";
            }
        }

        private async Task ValidateDataIntegrityAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "جاري التحقق من سلامة البيانات...";

                bool isValid = await _testDataService.ValidateDataIntegrityAsync();

                if (isValid)
                {
                    AddMessage("✅ البيانات سليمة ولا توجد مشاكل", MessageType.Success);
                }
                else
                {
                    AddMessage("⚠️ توجد مشاكل في سلامة البيانات - راجع السجل للتفاصيل", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في التحقق من سلامة البيانات", ex, "TestDataManagement");
                AddMessage($"❌ خطأ في التحقق من سلامة البيانات: {ex.Message}", MessageType.Error);
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = "";
            }
        }

        private async Task GetDataSummaryAsync()
        {
            try
            {
                IsLoading = true;
                LoadingMessage = "جاري إنشاء ملخص البيانات...";

                string summary = await _testDataService.GetSampleDataSummaryAsync();
                
                // Show summary in a message box or separate window
                MessageBox.Show(summary, "ملخص البيانات", MessageBoxButton.OK, MessageBoxImage.Information);
                
                AddMessage("📊 تم إنشاء ملخص البيانات", MessageType.Info);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء ملخص البيانات", ex, "TestDataManagement");
                AddMessage($"❌ خطأ في إنشاء ملخص البيانات: {ex.Message}", MessageType.Error);
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = "";
            }
        }

        private async Task ClearSampleDataAsync()
        {
            if (!CheckPermission()) return;

            var result = MessageBox.Show(
                "هل أنت متأكد من رغبتك في حذف جميع البيانات التجريبية؟\n\nهذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                LoadingMessage = "جاري حذف البيانات التجريبية...";

                bool success = await _testDataService.ClearAllSampleDataAsync();

                if (success)
                {
                    AddMessage("✅ تم حذف البيانات التجريبية بنجاح", MessageType.Success);
                    await LoadStatisticsAsync();
                }
                else
                {
                    AddMessage("⚠️ فشل في حذف البيانات التجريبية", MessageType.Warning);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في حذف البيانات التجريبية", ex, "TestDataManagement");
                AddMessage($"❌ خطأ في حذف البيانات التجريبية: {ex.Message}", MessageType.Error);
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = "";
            }
        }

        private async Task ClearAllDataAsync()
        {
            if (!CheckPermission()) return;

            var result = MessageBox.Show(
                "⚠️ تحذير: هذا الإجراء سيحذف جميع البيانات في النظام!\n\n" +
                "سيتم حذف:\n" +
                "• جميع المرضى وتحاليلهم\n" +
                "• جميع النتائج والتقارير\n" +
                "• جميع المستخدمين (عدا المدير الافتراضي)\n" +
                "• جميع أنواع ومجموعات التحاليل\n\n" +
                "هل أنت متأكد من رغبتك في المتابعة؟",
                "تحذير - حذف جميع البيانات",
                MessageBoxButton.YesNo,
                MessageBoxImage.Stop);

            if (result != MessageBoxResult.Yes) return;

            // Double confirmation
            var secondResult = MessageBox.Show(
                "تأكيد نهائي: اكتب 'نعم' إذا كنت متأكداً من رغبتك في حذف جميع البيانات",
                "تأكيد نهائي",
                MessageBoxButton.YesNo,
                MessageBoxImage.Stop);

            if (secondResult != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                LoadingMessage = "جاري حذف جميع البيانات...";

                // Clear all data except the default Manager user
                await _context.TestResults.ExecuteDeleteAsync();
                await _context.PatientTests.ExecuteDeleteAsync();
                await _context.Patients.ExecuteDeleteAsync();
                await _context.TestGroups.ExecuteDeleteAsync();
                await _context.TestTypes.ExecuteDeleteAsync();
                await _context.Users.Where(u => u.Username != "Manager").ExecuteDeleteAsync();
                await _context.LoginLogs.ExecuteDeleteAsync();
                await _context.SystemLogs.ExecuteDeleteAsync();
                await _context.BackupRecords.ExecuteDeleteAsync();

                AddMessage("⚠️ تم حذف جميع البيانات - سيتم إعادة تشغيل النظام", MessageType.Warning);
                await LoadStatisticsAsync();

                // Suggest restart
                MessageBox.Show(
                    "تم حذف جميع البيانات بنجاح.\n\nيُنصح بإعادة تشغيل التطبيق لضمان العمل السليم.",
                    "تم الحذف",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في حذف جميع البيانات", ex, "TestDataManagement");
                AddMessage($"❌ خطأ في حذف جميع البيانات: {ex.Message}", MessageType.Error);
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = "";
            }
        }

        private async Task SaveLogAsync()
        {
            try
            {
                var logText = string.Join("\n", Messages.Select(m => $"{m.Timestamp:yyyy-MM-dd HH:mm:ss} - {m.Message}"));
                
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = $"TestDataManagement_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    await System.IO.File.WriteAllTextAsync(saveDialog.FileName, logText);
                    AddMessage($"💾 تم حفظ السجل: {saveDialog.FileName}", MessageType.Success);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في حفظ سجل إدارة البيانات التجريبية", ex, "TestDataManagement");
                AddMessage($"❌ خطأ في حفظ السجل: {ex.Message}", MessageType.Error);
            }
        }

        private void ClearMessages()
        {
            Messages.Clear();
            HasMessages = false;
        }

        private void AddMessage(string message, MessageType type)
        {
            var statusMessage = new StatusMessage
            {
                Message = message,
                Timestamp = DateTime.Now,
                Type = type
            };

            Messages.Insert(0, statusMessage); // Add to top
            
            // Keep only last Constants.DefaultPageSize messages
            while (Messages.Count > Constants.DefaultPageSize)
            {
                Messages.RemoveAt(Messages.Count - 1);
            }

            HasMessages = Messages.Count > 0;
        }

        private bool CheckPermission()
        {
            if (_authenticationService.CurrentUser?.Role != "Manager")
            {
                MessageBox.Show(
                    "هذه العملية تتطلب صلاحيات المدير فقط.",
                    "صلاحيات غير كافية",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private string GetDataTypeArabicName(string dataType)
        {
            return dataType switch
            {
                "Users" => "المستخدمين التجريبيين",
                "TestTypes" => "أنواع التحاليل التجريبية",
                "TestGroups" => "مجموعات التحاليل التجريبية",
                "Patients" => "المرضى التجريبيين",
                "PatientTests" => "تحاليل المرضى التجريبية",
                "TestResults" => "نتائج التحاليل التجريبية",
                "Full" => "البيانات التجريبية الكاملة",
                "Demo" => "سيناريو العرض التوضيحي",
                "Performance" => "بيانات اختبار الأداء",
                "Reporting" => "بيانات اختبار التقارير",
                _ => "البيانات التجريبية"
            };
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    #region Helper Classes

    public class StatusMessage
    {
        public string Message { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public MessageType Type { get; set; }

        public string Icon => Type switch
        {
            MessageType.Success => "✅",
            MessageType.Warning => "⚠️",
            MessageType.Error => "❌",
            MessageType.Info => "ℹ️",
            _ => "📝"
        };

        public string BackgroundColor => Type switch
        {
            MessageType.Success => "#D5EDDA",
            MessageType.Warning => "#FFF3CD",
            MessageType.Error => "#F8D7DA",
            MessageType.Info => "#D1ECF1",
            _ => "#F8F9FA"
        };

        public string TextColor => Type switch
        {
            MessageType.Success => "#155724",
            MessageType.Warning => "#856404",
            MessageType.Error => "#721C24",
            MessageType.Info => "#0C5460",
            _ => "#495057"
        };
    }

    public enum MessageType
    {
        Info,
        Success,
        Warning,
        Error
    }

    #endregion
}
