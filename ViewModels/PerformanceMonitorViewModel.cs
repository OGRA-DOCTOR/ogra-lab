using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using OGRALAB.Helpers;
using OGRALAB.Services;

namespace OGRALAB.ViewModels
{
    public class PerformanceMonitorViewModel : INotifyPropertyChanged
    {
        private readonly IPerformanceService _performanceService;
        private readonly ILoggingService _loggingService;

        #region Private Fields

        private double _memoryUsageMB;
        private double _memoryUsagePercentage;
        private double _cpuUsage;
        private double _databaseSizeMB;
        private string _uptimeString = "";
        private int _cacheHitRatio;
        private int _activeConnections;
        private int _totalRecords;
        private int _performanceScore;
        private SolidColorBrush _performanceScoreColor;
        private DateTime _lastUpdateTime;
        private bool _autoRefreshEnabled = true;
        private string _autoRefreshStatus = "نشط";
        private bool _hasOptimizationSuggestions;

        #endregion

        #region Public Properties

        public double MemoryUsageMB
        {
            get => _memoryUsageMB;
            set
            {
                _memoryUsageMB = value;
                OnPropertyChanged();
                UpdateMemoryUsagePercentage();
            }
        }

        public double MemoryUsagePercentage
        {
            get => _memoryUsagePercentage;
            set
            {
                _memoryUsagePercentage = value;
                OnPropertyChanged();
            }
        }

        public double CpuUsage
        {
            get => _cpuUsage;
            set
            {
                _cpuUsage = value;
                OnPropertyChanged();
            }
        }

        public double DatabaseSizeMB
        {
            get => _databaseSizeMB;
            set
            {
                _databaseSizeMB = value;
                OnPropertyChanged();
            }
        }

        public string UptimeString
        {
            get => _uptimeString;
            set
            {
                _uptimeString = value;
                OnPropertyChanged();
            }
        }

        public int CacheHitRatio
        {
            get => _cacheHitRatio;
            set
            {
                _cacheHitRatio = value;
                OnPropertyChanged();
            }
        }

        public int ActiveConnections
        {
            get => _activeConnections;
            set
            {
                _activeConnections = value;
                OnPropertyChanged();
            }
        }

        public int TotalRecords
        {
            get => _totalRecords;
            set
            {
                _totalRecords = value;
                OnPropertyChanged();
            }
        }

        public int PerformanceScore
        {
            get => _performanceScore;
            set
            {
                _performanceScore = value;
                OnPropertyChanged();
                UpdatePerformanceScoreColor();
            }
        }

        public SolidColorBrush PerformanceScoreColor
        {
            get => _performanceScoreColor;
            set
            {
                _performanceScoreColor = value;
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

        public bool AutoRefreshEnabled
        {
            get => _autoRefreshEnabled;
            set
            {
                _autoRefreshEnabled = value;
                OnPropertyChanged();
                AutoRefreshStatus = value ? "نشط" : "متوقف";
            }
        }

        public string AutoRefreshStatus
        {
            get => _autoRefreshStatus;
            set
            {
                _autoRefreshStatus = value;
                OnPropertyChanged();
            }
        }

        public bool HasOptimizationSuggestions
        {
            get => _hasOptimizationSuggestions;
            set
            {
                _hasOptimizationSuggestions = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<OptimizationSuggestionViewModel> OptimizationSuggestions { get; set; }

        #endregion

        #region Commands

        public ICommand RefreshCommand { get; }
        public ICommand OptimizeMemoryCommand { get; }
        public ICommand OptimizeDatabaseCommand { get; }
        public ICommand ClearCacheCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand ApplyOptimizationCommand { get; }

        #endregion

        #region Constructor

        public PerformanceMonitorViewModel(IPerformanceService performanceService, ILoggingService loggingService)
        {
            _performanceService = performanceService;
            _loggingService = loggingService;

            OptimizationSuggestions = new ObservableCollection<OptimizationSuggestionViewModel>();
            PerformanceScoreColor = new SolidColorBrush(Colors.Green);

            // Initialize commands
            RefreshCommand = new RelayCommand(async () => await LoadDataAsync());
            OptimizeMemoryCommand = new RelayCommand(async () => await OptimizeMemoryAsync());
            OptimizeDatabaseCommand = new RelayCommand(async () => await OptimizeDatabaseAsync());
            ClearCacheCommand = new RelayCommand(async () => await ClearCacheAsync());
            GenerateReportCommand = new RelayCommand(async () => await GenerateReportAsync());
            ApplyOptimizationCommand = new RelayCommand<string>(async (id) => await ApplyOptimizationAsync(id));

            LastUpdateTime = DateTime.Now;
        }

        #endregion

        #region Public Methods

        public async Task LoadDataAsync()
        {
            try
            {
                await RefreshMetricsAsync();
                await LoadOptimizationSuggestionsAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحميل بيانات مراقب الأداء", ex, "PerformanceMonitor");
            }
        }

        public async Task RefreshMetricsAsync()
        {
            try
            {
                var metrics = _performanceService.GetPerformanceMetrics();
                var dbStats = await _performanceService.GetDatabaseStatsAsync();

                // Update memory metrics
                MemoryUsageMB = metrics.MemoryUsage / 1024.0 / 1024.0;
                
                // Update other metrics
                CpuUsage = metrics.CpuUsage;
                DatabaseSizeMB = metrics.DatabaseSize / 1024.0 / 1024.0;
                UptimeString = FormatUptime(metrics.Uptime);
                CacheHitRatio = metrics.CacheHitRatio;
                ActiveConnections = metrics.ActiveConnections;
                TotalRecords = dbStats.RecordCounts.Values.Sum();

                // Calculate performance score
                PerformanceScore = CalculatePerformanceScore(metrics);

                LastUpdateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحديث مؤشرات الأداء", ex, "PerformanceMonitor");
            }
        }

        public void Cleanup()
        {
            // Cleanup resources if needed
        }

        #endregion

        #region Private Methods

        private async Task LoadOptimizationSuggestionsAsync()
        {
            try
            {
                var suggestions = await _performanceService.GetOptimizationSuggestionsAsync();
                
                OptimizationSuggestions.Clear();
                foreach (var suggestion in suggestions)
                {
                    OptimizationSuggestions.Add(new OptimizationSuggestionViewModel(suggestion));
                }

                HasOptimizationSuggestions = OptimizationSuggestions.Count > 0;
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحميل اقتراحات التحسين", ex, "PerformanceMonitor");
            }
        }

        private async Task OptimizeMemoryAsync()
        {
            try
            {
                _performanceService.OptimizeMemory();
                await _loggingService.LogInfoAsync("تم تحسين الذاكرة من مراقب الأداء", "PerformanceMonitor");
                MessageBox.Show("تم تحسين الذاكرة بنجاح", "تحسين الأداء", MessageBoxButton.OK, MessageBoxImage.Information);
                await RefreshMetricsAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحسين الذاكرة", ex, "PerformanceMonitor");
                MessageBox.Show($"خطأ في تحسين الذاكرة: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task OptimizeDatabaseAsync()
        {
            try
            {
                await _performanceService.OptimizeDatabaseAsync();
                await _loggingService.LogInfoAsync("تم تحسين قاعدة البيانات من مراقب الأداء", "PerformanceMonitor");
                MessageBox.Show("تم تحسين قاعدة البيانات بنجاح", "تحسين الأداء", MessageBoxButton.OK, MessageBoxImage.Information);
                await RefreshMetricsAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في تحسين قاعدة البيانات", ex, "PerformanceMonitor");
                MessageBox.Show($"خطأ في تحسين قاعدة البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ClearCacheAsync()
        {
            try
            {
                _performanceService.ClearCache();
                await _loggingService.LogInfoAsync("تم مسح التخزين المؤقت من مراقب الأداء", "PerformanceMonitor");
                MessageBox.Show("تم مسح التخزين المؤقت بنجاح", "تحسين الأداء", MessageBoxButton.OK, MessageBoxImage.Information);
                await RefreshMetricsAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في مسح التخزين المؤقت", ex, "PerformanceMonitor");
                MessageBox.Show($"خطأ في مسح التخزين المؤقت: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task GenerateReportAsync()
        {
            try
            {
                string report = await _performanceService.GeneratePerformanceReportAsync();
                
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                    FileName = $"PerformanceReport_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    await System.IO.File.WriteAllTextAsync(saveDialog.FileName, report);
                    MessageBox.Show($"تم حفظ تقرير الأداء: {saveDialog.FileName}", "تقرير الأداء", MessageBoxButton.OK, MessageBoxImage.Information);
                    await _loggingService.LogInfoAsync($"تم إنشاء تقرير أداء: {saveDialog.FileName}", "PerformanceMonitor");
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync("خطأ في إنشاء تقرير الأداء", ex, "PerformanceMonitor");
                MessageBox.Show($"خطأ في إنشاء تقرير الأداء: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ApplyOptimizationAsync(string optimizationId)
        {
            if (string.IsNullOrEmpty(optimizationId)) return;

            try
            {
                await _performanceService.ApplyOptimizationAsync(optimizationId);
                await _loggingService.LogInfoAsync($"تم تطبيق التحسين: {optimizationId}", "PerformanceMonitor");
                
                // Refresh suggestions and metrics
                await LoadOptimizationSuggestionsAsync();
                await RefreshMetricsAsync();
                
                MessageBox.Show("تم تطبيق التحسين بنجاح", "تحسين الأداء", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                await _loggingService.LogErrorAsync($"خطأ في تطبيق التحسين: {optimizationId}", ex, "PerformanceMonitor");
                MessageBox.Show($"خطأ في تطبيق التحسين: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateMemoryUsagePercentage()
        {
            // Assume 512MB as maximum reasonable usage
            const double maxReasonableUsage = 512.0;
            MemoryUsagePercentage = Math.Min(Constants.CompletePercentage, (MemoryUsageMB / maxReasonableUsage) * Constants.CompletePercentage);
        }

        private void UpdatePerformanceScoreColor()
        {
            PerformanceScoreColor = PerformanceScore switch
            {
                >= Constants.HighCompletionThreshold => new SolidColorBrush(Colors.Green),
                >= 60 => new SolidColorBrush(Colors.Orange),
                _ => new SolidColorBrush(Colors.Red)
            };
        }

        private int CalculatePerformanceScore(PerformanceMetrics metrics)
        {
            int score = Constants.CompletePercentage;

            // Memory usage penalty
            var memoryUsageMB = metrics.MemoryUsage / 1024.0 / 1024.0;
            if (memoryUsageMB > 300) score -= 20;
            else if (memoryUsageMB > Constants.MaxPatientNameLength) score -= Constants.MaxConcurrentOperations;

            // CPU usage penalty
            if (metrics.CpuUsage > Constants.HighCompletionThreshold) score -= 20;
            else if (metrics.CpuUsage > 60) score -= Constants.MaxConcurrentOperations;

            // Cache hit ratio bonus/penalty
            if (metrics.CacheHitRatio < 70) score -= Constants.CacheDurationMinutes;
            else if (metrics.CacheHitRatio > 90) score += 5;

            // Database size penalty
            var dbSizeMB = metrics.DatabaseSize / 1024.0 / 1024.0;
            if (dbSizeMB > Constants.CompletePercentage) score -= Constants.MaxConcurrentOperations;

            return Math.Max(0, Math.Min(Constants.CompletePercentage, score));
        }

        private string FormatUptime(TimeSpan uptime)
        {
            if (uptime.TotalDays >= 1)
            {
                return $"{uptime.Days} يوم {uptime.Hours}:{uptime.Minutes:D2}";
            }
            else if (uptime.TotalHours >= 1)
            {
                return $"{uptime.Hours}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
            }
            else
            {
                return $"{uptime.Minutes:D2}:{uptime.Seconds:D2}";
            }
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

    public class OptimizationSuggestionViewModel
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Impact { get; set; } = "";
        public bool IsAutoApplicable { get; set; }
        public string Category { get; set; } = "";

        public string Icon => Category switch
        {
            "Memory" or "الذاكرة" => "🧠",
            "Database" or "قاعدة البيانات" => "💾",
            "UI" or "واجهة المستخدم" => "🖥️",
            "Cache" or "التخزين المؤقت" => "📦",
            "Logs" or "السجلات" => "📝",
            _ => "⚙️"
        };

        public string BackgroundColor => Impact switch
        {
            "High" or "عالي" => "#FFEBEE",
            "Medium" or "متوسط" => "#FFF8E1",
            "Low" or "منخفض" => "#E8F5E8",
            _ => "#F5F5F5"
        };

        public string ImpactColor => Impact switch
        {
            "High" or "عالي" => "#D32F2F",
            "Medium" or "متوسط" => "#F57C00",
            "Low" or "منخفض" => "#388E3C",
            _ => "#666666"
        };

        public OptimizationSuggestionViewModel(OptimizationSuggestion suggestion)
        {
            Id = suggestion.Id;
            Title = suggestion.Title;
            Description = suggestion.Description;
            Impact = suggestion.Impact;
            IsAutoApplicable = suggestion.IsAutoApplicable;
            Category = suggestion.Category;
        }
    }

    #endregion
}
