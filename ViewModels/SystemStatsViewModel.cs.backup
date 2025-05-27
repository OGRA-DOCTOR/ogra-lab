using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using OGRALAB.Services;
using OGRALAB.Helpers;

namespace OGRALAB.ViewModels
{
    public class SystemStatsViewModel : INotifyPropertyChanged
    {
        private readonly IStatsService _statsService;
        private readonly IAuthenticationService _authenticationService;
        
        private bool _isLoading;
        private Dictionary<string, object> _dashboardStats;
        private Dictionary<string, int> _statusStats;
        private Dictionary<string, int> _testTypeStats;
        private Dictionary<string, int> _userActivityStats;
        private Dictionary<string, int> _monthlyPatientsStats;
        private Dictionary<string, int> _monthlyTestsStats;

        // Date range for filtering
        private DateTime _fromDate = DateTime.Today.AddDays(-30);
        private DateTime _toDate = DateTime.Today;

        public SystemStatsViewModel(IStatsService statsService, IAuthenticationService authenticationService)
        {
            _statsService = statsService;
            _authenticationService = authenticationService;
            
            _dashboardStats = new Dictionary<string, object>();
            _statusStats = new Dictionary<string, int>();
            _testTypeStats = new Dictionary<string, int>();
            _userActivityStats = new Dictionary<string, int>();
            _monthlyPatientsStats = new Dictionary<string, int>();
            _monthlyTestsStats = new Dictionary<string, int>();
            
            // Commands
            LoadStatsCommand = new RelayCommand(async () => await LoadStatsAsync());
            RefreshStatsCommand = new RelayCommand(async () => await RefreshStatsAsync());
            ExportStatsReportCommand = new RelayCommand(async () => await ExportStatsReportAsync(), CanExportStats);
            ExportStatsJsonCommand = new RelayCommand(async () => await ExportStatsJsonAsync(), CanExportStats);
            ExportStatsCsvCommand = new RelayCommand(async () => await ExportStatsCsvAsync(), CanExportStats);
            
            // Load initial data
            _ = LoadStatsAsync();
        }

        // Properties
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set
            {
                if (SetProperty(ref _fromDate, value))
                {
                    _ = RefreshStatsAsync();
                }
            }
        }

        public DateTime ToDate
        {
            get => _toDate;
            set
            {
                if (SetProperty(ref _toDate, value))
                {
                    _ = RefreshStatsAsync();
                }
            }
        }

        // Dashboard Stats Properties
        public int TotalPatients => GetIntValue("TotalPatients");
        public int TotalTests => GetIntValue("TotalTests");
        public int PendingTests => GetIntValue("PendingTests");
        public int CompletedTests => GetIntValue("CompletedTests");
        public int TodayPatients => GetIntValue("TodayPatients");
        public int TodayTests => GetIntValue("TodayTests");
        public int TodayCompleted => GetIntValue("TodayCompleted");
        public int MonthlyPatients => GetIntValue("MonthlyPatients");
        public int MonthlyTests => GetIntValue("MonthlyTests");
        public double AvgProcessingTime => GetDoubleValue("AvgProcessingTime");
        public DateTime LastBackupDate => GetDateTimeValue("LastBackupDate");

        // Calculated Properties
        public string CompletionRate
        {
            get
            {
                if (TotalTests == 0) return "0%";
                var rate = (CompletedTests * 100.0) / TotalTests;
                return $"{rate:F1}%";
            }
        }

        public string TodayCompletionRate
        {
            get
            {
                if (TodayTests == 0) return "0%";
                var rate = (TodayCompleted * 100.0) / TodayTests;
                return $"{rate:F1}%";
            }
        }

        public string AvgProcessingTimeFormatted
        {
            get
            {
                if (AvgProcessingTime <= 0) return "غير متوفر";
                if (AvgProcessingTime < 1)
                    return $"{AvgProcessingTime * 60:F0} دقيقة";
                else if (AvgProcessingTime < 24)
                    return $"{AvgProcessingTime:F1} ساعة";
                else
                    return $"{AvgProcessingTime / 24:F1} يوم";
            }
        }

        public string LastBackupFormatted
        {
            get
            {
                if (LastBackupDate == DateTime.MinValue)
                    return "لا توجد نسخ احتياطية";
                
                var diff = DateTime.Now - LastBackupDate;
                if (diff.TotalDays > 7)
                    return $"منذ {diff.TotalDays:F0} يوم";
                else if (diff.TotalHours > 24)
                    return $"منذ {diff.TotalDays:F0} يوم";
                else if (diff.TotalHours > 1)
                    return $"منذ {diff.TotalHours:F0} ساعة";
                else
                    return "اليوم";
            }
        }

        // Collection Properties
        public Dictionary<string, int> StatusStats
        {
            get => _statusStats;
            set => SetProperty(ref _statusStats, value);
        }

        public Dictionary<string, int> TestTypeStats
        {
            get => _testTypeStats;
            set => SetProperty(ref _testTypeStats, value);
        }

        public Dictionary<string, int> UserActivityStats
        {
            get => _userActivityStats;
            set => SetProperty(ref _userActivityStats, value);
        }

        public Dictionary<string, int> MonthlyPatientsStats
        {
            get => _monthlyPatientsStats;
            set => SetProperty(ref _monthlyPatientsStats, value);
        }

        public Dictionary<string, int> MonthlyTestsStats
        {
            get => _monthlyTestsStats;
            set => SetProperty(ref _monthlyTestsStats, value);
        }

        // Commands
        public ICommand LoadStatsCommand { get; }
        public ICommand RefreshStatsCommand { get; }
        public ICommand ExportStatsReportCommand { get; }
        public ICommand ExportStatsJsonCommand { get; }
        public ICommand ExportStatsCsvCommand { get; }

        // Methods
        private async Task LoadStatsAsync()
        {
            try
            {
                IsLoading = true;
                
                // Load dashboard stats
                _dashboardStats = await _statsService.GetDashboardStatsAsync();
                
                // Load other stats
                await RefreshDetailedStatsAsync();
                
                // Notify all properties changed
                NotifyAllStatsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل الإحصائيات: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshStatsAsync()
        {
            try
            {
                IsLoading = true;
                await RefreshDetailedStatsAsync();
                NotifyAllStatsChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحديث الإحصائيات: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDetailedStatsAsync()
        {
            // Load status statistics
            StatusStats = await _statsService.GetTestsCountByAllStatusesAsync();
            
            // Load test type statistics
            TestTypeStats = await _statsService.GetTestTypesUsageAsync(FromDate, ToDate);
            
            // Load user activity statistics
            UserActivityStats = await _statsService.GetUserLoginStatsAsync(FromDate, ToDate);
            
            // Load monthly statistics
            MonthlyPatientsStats = await _statsService.GetMonthlyPatientsStatsAsync(DateTime.Now.Year);
            MonthlyTestsStats = await _statsService.GetMonthlyTestsStatsAsync(DateTime.Now.Year);
        }

        private async Task ExportStatsReportAsync()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            saveFileDialog.FileName = $"OgraLab_Statistics_Report_{DateTime.Now:yyyyMMdd}.txt";
            
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    
                    var report = await _statsService.GenerateStatsReportAsync(FromDate, ToDate);
                    await System.IO.File.WriteAllTextAsync(saveFileDialog.FileName, report);
                    
                    MessageBox.Show("تم تصدير التقرير بنجاح", "نجح", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في تصدير التقرير: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ExportStatsJsonAsync()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*";
            saveFileDialog.FileName = $"OgraLab_Statistics_{DateTime.Now:yyyyMMdd}.json";
            
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    
                    var success = await _statsService.ExportStatsToJsonAsync(saveFileDialog.FileName, FromDate, ToDate);
                    
                    if (success)
                    {
                        MessageBox.Show("تم تصدير البيانات بنجاح", "نجح", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في تصدير البيانات", "خطأ", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في تصدير البيانات: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private async Task ExportStatsCsvAsync()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
            saveFileDialog.FileName = $"OgraLab_Statistics_{DateTime.Now:yyyyMMdd}.csv";
            
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsLoading = true;
                    
                    var success = await _statsService.ExportStatsToCsvAsync(saveFileDialog.FileName, FromDate, ToDate);
                    
                    if (success)
                    {
                        MessageBox.Show("تم تصدير البيانات بنجاح", "نجح", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في تصدير البيانات", "خطأ", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في تصدير البيانات: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        // Helper Methods
        private int GetIntValue(string key)
        {
            if (_dashboardStats.TryGetValue(key, out var value) && value is int intValue)
                return intValue;
            return 0;
        }

        private double GetDoubleValue(string key)
        {
            if (_dashboardStats.TryGetValue(key, out var value))
            {
                if (value is double doubleValue)
                    return doubleValue;
                if (value is int intValue)
                    return intValue;
            }
            return 0.0;
        }

        private DateTime GetDateTimeValue(string key)
        {
            if (_dashboardStats.TryGetValue(key, out var value) && value is DateTime dateTimeValue)
                return dateTimeValue;
            return DateTime.MinValue;
        }

        private void NotifyAllStatsChanged()
        {
            OnPropertyChanged(nameof(TotalPatients));
            OnPropertyChanged(nameof(TotalTests));
            OnPropertyChanged(nameof(PendingTests));
            OnPropertyChanged(nameof(CompletedTests));
            OnPropertyChanged(nameof(TodayPatients));
            OnPropertyChanged(nameof(TodayTests));
            OnPropertyChanged(nameof(TodayCompleted));
            OnPropertyChanged(nameof(MonthlyPatients));
            OnPropertyChanged(nameof(MonthlyTests));
            OnPropertyChanged(nameof(AvgProcessingTime));
            OnPropertyChanged(nameof(LastBackupDate));
            
            OnPropertyChanged(nameof(CompletionRate));
            OnPropertyChanged(nameof(TodayCompletionRate));
            OnPropertyChanged(nameof(AvgProcessingTimeFormatted));
            OnPropertyChanged(nameof(LastBackupFormatted));
        }

        // Command Conditions
        private bool CanExportStats()
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
