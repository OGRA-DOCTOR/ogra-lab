using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Helpers;

namespace OGRALAB.ViewModels
{
    public class ResultEntryControlViewModel : INotifyPropertyChanged
    {
        private readonly ITestService _testService;
        private readonly IPatientService _patientService;
        
        private ObservableCollection<PatientTest> _pendingTests;
        private ObservableCollection<TestResult> _recentResults;
        private bool _isLoading;

        public ResultEntryControlViewModel(ITestService testService, IPatientService patientService)
        {
            _testService = testService;
            _patientService = patientService;
            
            _pendingTests = new ObservableCollection<PatientTest>();
            _recentResults = new ObservableCollection<TestResult>();
            
            // Commands
            RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());
            
            // Load initial data
            _ = LoadDataAsync();
        }

        // Properties
        public ObservableCollection<PatientTest> PendingTests
        {
            get => _pendingTests;
            set => SetProperty(ref _pendingTests, value);
        }

        public ObservableCollection<TestResult> RecentResults
        {
            get => _recentResults;
            set => SetProperty(ref _recentResults, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public int PendingTestsCount => PendingTests.Count;
        public int RecentResultsCount => RecentResults.Count;
        public bool HasRecentResults => RecentResults.Any();

        // Commands
        public ICommand RefreshCommand { get; }

        // Methods
        private async Task LoadDataAsync()
        {
            await Task.WhenAll(
                LoadPendingTestsAsync(),
                LoadRecentResultsAsync()
            );
        }

        private async Task LoadPendingTestsAsync()
        {
            try
            {
                IsLoading = true;
                var allTests = await _testService.GetAllPatientTestsAsync();
                var pendingTests = allTests.Where(t => t.Status == "Pending" || t.Status == "InProgress")
                                          .OrderBy(t => t.OrderDate)
                                          .Take(10) // Show only recent 10
                                          .ToList();

                PendingTests.Clear();
                foreach (var test in pendingTests)
                {
                    PendingTests.Add(test);
                }

                OnPropertyChanged(nameof(PendingTestsCount));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading pending tests: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadRecentResultsAsync()
        {
            try
            {
                IsLoading = true;
                var allResults = await _testService.GetAllTestResultsAsync();
                var recentResults = allResults.OrderByDescending(r => r.EnteredDate)
                                             .Take(10) // Show only recent 10
                                             .ToList();

                RecentResults.Clear();
                foreach (var result in recentResults)
                {
                    RecentResults.Add(result);
                }

                OnPropertyChanged(nameof(RecentResultsCount));
                OnPropertyChanged(nameof(HasRecentResults));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent results: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDataAsync()
        {
            await LoadDataAsync();
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
