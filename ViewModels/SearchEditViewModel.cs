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
    public class SearchEditViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private readonly ITestService _testService;
        private readonly IAuthenticationService _authenticationService;
        
        private ObservableCollection<Patient> _searchResults;
        private Patient? _selectedPatient;
        private ObservableCollection<PatientTest> _patientTests;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private string _searchType = "الاسم";
        private DateTime? _searchDateFrom;
        private DateTime? _searchDateTo;
        private string _referralSource = string.Empty;

        public SearchEditViewModel(IPatientService patientService, ITestService testService, IAuthenticationService authenticationService)
        {
            _patientService = patientService;
            _testService = testService;
            _authenticationService = authenticationService;
            
            _searchResults = new ObservableCollection<Patient>();
            _patientTests = new ObservableCollection<PatientTest>();
            
            // Commands
            SearchCommand = new RelayCommand(async () => await SearchPatientsAsync());
            ClearSearchCommand = new RelayCommand(ClearSearch);
            SelectPatientCommand = new RelayCommand<Patient>(SelectPatient);
            EditPatientCommand = new RelayCommand(EditPatient, CanEditPatient);
            ViewPatientHistoryCommand = new RelayCommand(ViewPatientHistory, CanViewPatientHistory);
            EditTestResultCommand = new RelayCommand<PatientTest>(EditTestResult, CanEditTestResult);
            RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());
            
            // Search types
            SearchTypes = new[] { "الاسم", "رقم المريض", "رقم الهوية", "رقم الهاتف", "جهة التحويل" };
            
            // Load recent patients
            _ = LoadRecentPatientsAsync();
        }

        // Properties
        public ObservableCollection<Patient> SearchResults
        {
            get => _searchResults;
            set => SetProperty(ref _searchResults, value);
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
                        PatientTests.Clear();
                    }
                    
                    ((RelayCommand)EditPatientCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ViewPatientHistoryCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<PatientTest> PatientTests
        {
            get => _patientTests;
            set => SetProperty(ref _patientTests, value);
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

        public string SearchType
        {
            get => _searchType;
            set => SetProperty(ref _searchType, value);
        }

        public DateTime? SearchDateFrom
        {
            get => _searchDateFrom;
            set => SetProperty(ref _searchDateFrom, value);
        }

        public DateTime? SearchDateTo
        {
            get => _searchDateTo;
            set => SetProperty(ref _searchDateTo, value);
        }

        public string ReferralSource
        {
            get => _referralSource;
            set => SetProperty(ref _referralSource, value);
        }

        public string[] SearchTypes { get; }

        public int SearchResultsCount => SearchResults.Count;
        public int PatientTestsCount => PatientTests.Count;

        // Permissions
        public bool CanEditPatients => _authenticationService.CurrentUser?.Role != null && 
                                      (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                       _authenticationService.CurrentUser.Role == "AdminUser" || 
                                       _authenticationService.CurrentUser.Role == "RegularUser");

        public bool CanEditResults => _authenticationService.CurrentUser?.Role != null && 
                                     (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                      _authenticationService.CurrentUser.Role == "AdminUser");

        // Commands
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand SelectPatientCommand { get; }
        public ICommand EditPatientCommand { get; }
        public ICommand ViewPatientHistoryCommand { get; }
        public ICommand EditTestResultCommand { get; }
        public ICommand RefreshCommand { get; }

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
                    switch (SearchType)
                    {
                        case "الاسم":
                            filteredPatients = filteredPatients.Where(p => 
                                p.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                            break;
                        case "رقم المريض":
                            filteredPatients = filteredPatients.Where(p => 
                                p.PatientNumber != null && 
                                p.PatientNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                            break;
                        case "رقم الهوية":
                            filteredPatients = filteredPatients.Where(p => 
                                p.NationalId != null && 
                                p.NationalId.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                            break;
                        case "رقم الهاتف":
                            filteredPatients = filteredPatients.Where(p => 
                                p.PhoneNumber != null && 
                                p.PhoneNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                            break;
                        case "جهة التحويل":
                            filteredPatients = filteredPatients.Where(p => 
                                p.ReferralSource != null && 
                                p.ReferralSource.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                            break;
                    }
                }

                // Apply referral source filter
                if (!string.IsNullOrWhiteSpace(ReferralSource))
                {
                    filteredPatients = filteredPatients.Where(p => 
                        p.ReferralSource != null && 
                        p.ReferralSource.Contains(ReferralSource, StringComparison.OrdinalIgnoreCase));
                }

                // Apply date range filter
                if (SearchDateFrom.HasValue)
                {
                    filteredPatients = filteredPatients.Where(p => 
                        p.CreatedDate.Date >= SearchDateFrom.Value.Date);
                }

                if (SearchDateTo.HasValue)
                {
                    filteredPatients = filteredPatients.Where(p => 
                        p.CreatedDate.Date <= SearchDateTo.Value.Date);
                }

                SearchResults.Clear();
                foreach (var patient in filteredPatients.OrderByDescending(p => p.CreatedDate))
                {
                    SearchResults.Add(patient);
                }

                OnPropertyChanged(nameof(SearchResultsCount));

                if (!SearchResults.Any() && (!string.IsNullOrWhiteSpace(SearchText) || SearchDateFrom.HasValue || SearchDateTo.HasValue))
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

        private void ClearSearch()
        {
            SearchText = string.Empty;
            ReferralSource = string.Empty;
            SearchDateFrom = null;
            SearchDateTo = null;
            SearchResults.Clear();
            SelectedPatient = null;
            OnPropertyChanged(nameof(SearchResultsCount));
        }

        private void SelectPatient(Patient? patient)
        {
            SelectedPatient = patient;
        }

        private void EditPatient()
        {
            if (SelectedPatient == null) return;

            try
            {
                var editPatientWindow = new PatientEditWindow(SelectedPatient);
                editPatientWindow.Owner = Application.Current.MainWindow;
                var result = editPatientWindow.ShowDialog();
                
                if (result == true)
                {
                    _ = RefreshDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة تعديل المريض: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditPatient()
        {
            return SelectedPatient != null && CanEditPatients;
        }

        private void ViewPatientHistory()
        {
            if (SelectedPatient == null) return;

            try
            {
                var viewPatientWindow = new PatientEditWindow(SelectedPatient);
                viewPatientWindow.Owner = Application.Current.MainWindow;
                viewPatientWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض تاريخ المريض: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanViewPatientHistory()
        {
            return SelectedPatient != null;
        }

        private void EditTestResult(PatientTest? patientTest)
        {
            if (patientTest == null || !CanEditResults) return;

            try
            {
                var resultEntryWindow = new TestResultEntryWindow();
                resultEntryWindow.Owner = Application.Current.MainWindow;
                
                // Set the selected patient and test
                resultEntryWindow.ViewModel.SelectedPatient = SelectedPatient;
                resultEntryWindow.ViewModel.SelectedPatientTest = patientTest;
                
                resultEntryWindow.ShowDialog();
                
                // Refresh data after closing
                _ = RefreshDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة تعديل النتيجة: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditTestResult(PatientTest? patientTest)
        {
            return patientTest != null && CanEditResults;
        }

        private async Task LoadPatientTestsAsync(int patientId)
        {
            try
            {
                IsLoading = true;
                var tests = await _testService.GetPatientTestsByPatientIdAsync(patientId);
                
                PatientTests.Clear();
                foreach (var test in tests.OrderByDescending(t => t.OrderDate))
                {
                    PatientTests.Add(test);
                }

                OnPropertyChanged(nameof(PatientTestsCount));
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
                var recentPatients = patients.OrderByDescending(p => p.CreatedDate).Take(Constants.MaxConcurrentOperations);
                
                SearchResults.Clear();
                foreach (var patient in recentPatients)
                {
                    SearchResults.Add(patient);
                }

                OnPropertyChanged(nameof(SearchResultsCount));
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

        private async Task RefreshDataAsync()
        {
            if (!string.IsNullOrWhiteSpace(SearchText) || SearchDateFrom.HasValue || SearchDateTo.HasValue)
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
