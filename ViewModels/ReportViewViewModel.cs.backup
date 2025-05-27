using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Helpers;
using OGRALAB.Views;

namespace OGRALAB.ViewModels
{
    public class ReportViewViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private readonly ITestService _testService;
        private readonly IReportService _reportService;
        private readonly IAuthenticationService _authenticationService;
        
        private ObservableCollection<Patient> _patients;
        private Patient? _selectedPatient;
        private ObservableCollection<PatientTest> _completedTests;
        private PatientTest? _selectedTest;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private DateTime? _dateFrom;
        private DateTime? _dateTo;
        private string _reportType = "نتائج المريض";

        public ReportViewViewModel(IPatientService patientService, ITestService testService, 
            IReportService reportService, IAuthenticationService authenticationService)
        {
            _patientService = patientService;
            _testService = testService;
            _reportService = reportService;
            _authenticationService = authenticationService;
            
            _patients = new ObservableCollection<Patient>();
            _completedTests = new ObservableCollection<PatientTest>();
            
            // Commands
            SearchPatientsCommand = new RelayCommand(async () => await SearchPatientsAsync());
            PreviewReportCommand = new RelayCommand(async () => await PreviewReportAsync(), CanPreviewReport);
            PrintReportCommand = new RelayCommand(async () => await PrintReportAsync(), CanPrintReport);
            RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());
            ClearSearchCommand = new RelayCommand(ClearSearch);
            
            // Report types
            ReportTypes = new[] { "نتائج المريض", "تقرير شامل", "تقرير يومي", "تقرير شهري" };
            
            // Initialize dates
            DateFrom = DateTime.Today.AddDays(-30);
            DateTo = DateTime.Today;
            
            // Load initial data
            _ = LoadRecentPatientsAsync();
        }

        // Properties
        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    if (value != null)
                    {
                        _ = LoadPatientTestsAsync(value.PatientId);
                    }
                    else
                    {
                        CompletedTests.Clear();
                    }
                    
                    ((RelayCommand)PreviewReportCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)PrintReportCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<PatientTest> CompletedTests
        {
            get => _completedTests;
            set => SetProperty(ref _completedTests, value);
        }

        public PatientTest? SelectedTest
        {
            get => _selectedTest;
            set
            {
                SetProperty(ref _selectedTest, value);
                ((RelayCommand)PreviewReportCommand).RaiseCanExecuteChanged();
                ((RelayCommand)PrintReportCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set => SetProperty(ref _dateFrom, value);
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set => SetProperty(ref _dateTo, value);
        }

        public string ReportType
        {
            get => _reportType;
            set => SetProperty(ref _reportType, value);
        }

        public string[] ReportTypes { get; }

        // Display Properties
        public int PatientsCount => Patients.Count;
        public int CompletedTestsCount => CompletedTests.Count;
        public bool HasSelectedPatient => SelectedPatient != null;
        public bool HasCompletedTests => CompletedTests.Any();

        // Permissions
        public bool CanViewReports => _authenticationService.CurrentUser?.Role != null;
        public bool CanPrintReports => _authenticationService.CurrentUser?.Role != null && 
                                      (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                       _authenticationService.CurrentUser.Role == "AdminUser" || 
                                       _authenticationService.CurrentUser.Role == "RegularUser");

        // Commands
        public ICommand SearchPatientsCommand { get; }
        public ICommand PreviewReportCommand { get; }
        public ICommand PrintReportCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearSearchCommand { get; }

        // Methods
        private async Task SearchPatientsAsync()
        {
            try
            {
                IsLoading = true;
                
                var allPatients = await _patientService.GetAllPatientsAsync();
                var filteredPatients = allPatients.AsEnumerable();

                // Apply text search
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredPatients = filteredPatients.Where(p => 
                        p.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                        (p.PatientNumber != null && p.PatientNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                }

                // Apply date range filter
                if (DateFrom.HasValue)
                {
                    filteredPatients = filteredPatients.Where(p => 
                        p.CreatedDate.Date >= DateFrom.Value.Date);
                }

                if (DateTo.HasValue)
                {
                    filteredPatients = filteredPatients.Where(p => 
                        p.CreatedDate.Date <= DateTo.Value.Date);
                }

                Patients.Clear();
                foreach (var patient in filteredPatients.OrderByDescending(p => p.CreatedDate))
                {
                    Patients.Add(patient);
                }

                OnPropertyChanged(nameof(PatientsCount));

                if (!Patients.Any())
                {
                    MessageBox.Show("لم يتم العثور على أي مريض يطابق معايير البحث", "نتائج البحث", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPatientTestsAsync(int patientId)
        {
            try
            {
                IsLoading = true;
                var tests = await _testService.GetPatientTestsByPatientIdAsync(patientId);
                var completedTests = tests.Where(t => t.Status == "Completed").ToList();
                
                CompletedTests.Clear();
                foreach (var test in completedTests.OrderByDescending(t => t.CompletedDate))
                {
                    CompletedTests.Add(test);
                }

                OnPropertyChanged(nameof(CompletedTestsCount));
                OnPropertyChanged(nameof(HasCompletedTests));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل تحاليل المريض: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadRecentPatientsAsync()
        {
            try
            {
                IsLoading = true;
                var patients = await _patientService.GetAllPatientsAsync();
                var recentPatients = patients.OrderByDescending(p => p.CreatedDate).Take(20);
                
                Patients.Clear();
                foreach (var patient in recentPatients)
                {
                    Patients.Add(patient);
                }

                OnPropertyChanged(nameof(PatientsCount));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading recent patients: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PreviewReportAsync()
        {
            if (SelectedPatient == null) return;

            try
            {
                IsLoading = true;
                
                // Get patient's completed tests with results
                var testsWithResults = await GetPatientTestsWithResultsAsync(SelectedPatient.PatientId);
                
                if (!testsWithResults.Any())
                {
                    MessageBox.Show("لا توجد نتائج مكتملة لهذا المريض", "تنبيه", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Create and show report preview window
                var reportPreviewWindow = new ReportPreviewWindow(SelectedPatient, testsWithResults);
                reportPreviewWindow.Owner = Application.Current.MainWindow;
                reportPreviewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في معاينة التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task PrintReportAsync()
        {
            if (SelectedPatient == null) return;

            try
            {
                IsLoading = true;
                
                // Get patient's completed tests with results
                var testsWithResults = await GetPatientTestsWithResultsAsync(SelectedPatient.PatientId);
                
                if (!testsWithResults.Any())
                {
                    MessageBox.Show("لا توجد نتائج مكتملة لهذا المريض", "تنبيه", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Create report preview window with print option
                var reportPreviewWindow = new ReportPreviewWindow(SelectedPatient, testsWithResults, true);
                reportPreviewWindow.Owner = Application.Current.MainWindow;
                reportPreviewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في طباعة التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task<List<PatientTest>> GetPatientTestsWithResultsAsync(int patientId)
        {
            var tests = await _testService.GetPatientTestsByPatientIdAsync(patientId);
            var testsWithResults = new List<PatientTest>();

            foreach (var test in tests.Where(t => t.Status == "Completed"))
            {
                var results = await _testService.GetTestResultsByPatientTestIdAsync(test.PatientTestId);
                if (results.Any())
                {
                    test.TestResults = results.ToList();
                    testsWithResults.Add(test);
                }
            }

            return testsWithResults;
        }

        private bool CanPreviewReport()
        {
            return SelectedPatient != null && HasCompletedTests && CanViewReports;
        }

        private bool CanPrintReport()
        {
            return SelectedPatient != null && HasCompletedTests && CanPrintReports;
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            DateFrom = DateTime.Today.AddDays(-30);
            DateTo = DateTime.Today;
            SelectedPatient = null;
            _ = LoadRecentPatientsAsync();
        }

        private async Task RefreshDataAsync()
        {
            if (!string.IsNullOrWhiteSpace(SearchText) || 
                DateFrom != DateTime.Today.AddDays(-30) || 
                DateTo != DateTime.Today)
            {
                await SearchPatientsAsync();
            }
            else
            {
                await LoadRecentPatientsAsync();
            }
            
            if (SelectedPatient != null)
            {
                await LoadPatientTestsAsync(SelectedPatient.PatientId);
            }
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
