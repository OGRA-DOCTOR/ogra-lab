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
    public class TestResultEntryViewModel : INotifyPropertyChanged
    {
        private readonly ITestService _testService;
        private readonly IPatientService _patientService;
        private readonly IAuthenticationService _authenticationService;
        
        private Patient? _selectedPatient;
        private PatientTest? _selectedPatientTest;
        private ObservableCollection<PatientTest> _pendingTests;
        private ObservableCollection<PatientTest> _completedTests;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private string _resultValue = string.Empty;
        private string _notes = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isOutOfRange;

        public TestResultEntryViewModel(ITestService testService, IPatientService patientService, IAuthenticationService authenticationService)
        {
            _testService = testService;
            _patientService = patientService;
            _authenticationService = authenticationService;
            
            _pendingTests = new ObservableCollection<PatientTest>();
            _completedTests = new ObservableCollection<PatientTest>();
            
            // Commands
            SearchPatientCommand = new RelayCommand(async () => await SearchPatientAsync());
            SelectPatientCommand = new RelayCommand<Patient>(SelectPatient);
            SelectTestCommand = new RelayCommand<PatientTest>(SelectTest);
            SaveResultCommand = new RelayCommand(async () => await SaveResultAsync(), CanSaveResult);
            ClearResultCommand = new RelayCommand(ClearResult);
            LoadPendingTestsCommand = new RelayCommand(async () => await LoadPendingTestsAsync());
            RefreshCommand = new RelayCommand(async () => await RefreshDataAsync());
            MarkInProgressCommand = new RelayCommand(async () => await MarkTestInProgressAsync(), CanMarkInProgress);
            
            // Load initial data
            _ = LoadPendingTestsAsync();
        }

        // Properties
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
                    ClearResult();
                }
            }
        }

        public PatientTest? SelectedPatientTest
        {
            get => _selectedPatientTest;
            set
            {
                if (SetProperty(ref _selectedPatientTest, value))
                {
                    LoadTestDetails();
                    ((RelayCommand)SaveResultCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)MarkInProgressCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<PatientTest> PendingTests
        {
            get => _pendingTests;
            set => SetProperty(ref _pendingTests, value);
        }

        public ObservableCollection<PatientTest> CompletedTests
        {
            get => _completedTests;
            set => SetProperty(ref _completedTests, value);
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

        public string ResultValue
        {
            get => _resultValue;
            set
            {
                if (SetProperty(ref _resultValue, value))
                {
                    CheckNormalRange();
                    ErrorMessage = string.Empty;
                    ((RelayCommand)SaveResultCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsOutOfRange
        {
            get => _isOutOfRange;
            set => SetProperty(ref _isOutOfRange, value);
        }

        // Display Properties
        public string TestName => SelectedPatientTest?.TestType?.TestName ?? string.Empty;
        public string TestUnit => SelectedPatientTest?.TestType?.Unit ?? string.Empty;
        public string NormalRange => SelectedPatientTest?.TestType?.NormalRange ?? string.Empty;
        public string PatientInfo => SelectedPatient != null ? 
            $"{SelectedPatient.FullName} - {SelectedPatient.PatientNumber}" : string.Empty;

        // Permissions
        public bool CanEnterResults => _authenticationService.CurrentUser?.Role != null && 
                                      (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                       _authenticationService.CurrentUser.Role == "AdminUser" || 
                                       _authenticationService.CurrentUser.Role == "RegularUser");

        // Commands
        public ICommand SearchPatientCommand { get; }
        public ICommand SelectPatientCommand { get; }
        public ICommand SelectTestCommand { get; }
        public ICommand SaveResultCommand { get; }
        public ICommand ClearResultCommand { get; }
        public ICommand LoadPendingTestsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand MarkInProgressCommand { get; }

        // Methods
        private async Task SearchPatientAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                MessageBox.Show("الرجاء إدخال نص للبحث", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                IsLoading = true;
                var patients = await _patientService.SearchPatientsAsync(SearchText);
                
                if (patients.Any())
                {
                    // If only one patient found, select automatically
                    if (patients.Count() == 1)
                    {
                        SelectedPatient = patients.First();
                    }
                    else
                    {
                        // Show selection window (could be implemented later)
                        SelectedPatient = patients.First();
                    }
                }
                else
                {
                    MessageBox.Show($"لم يتم العثور على أي مريض يحتوي على '{SearchText}'", "نتائج البحث", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في البحث: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void SelectPatient(Patient? patient)
        {
            SelectedPatient = patient;
        }

        private void SelectTest(PatientTest? test)
        {
            SelectedPatientTest = test;
        }

        private async Task LoadPatientTestsAsync(int patientId)
        {
            try
            {
                IsLoading = true;
                var tests = await _testService.GetPatientTestsByPatientIdAsync(patientId);
                
                PendingTests.Clear();
                CompletedTests.Clear();
                
                foreach (var test in tests.OrderBy(t => t.OrderDate))
                {
                    if (test.Status == "Pending" || test.Status == "InProgress")
                    {
                        PendingTests.Add(test);
                    }
                    else if (test.Status == "Completed")
                    {
                        CompletedTests.Add(test);
                    }
                }

                OnPropertyChanged(nameof(PatientInfo));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل تحاليل المريض: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadTestDetails()
        {
            if (SelectedPatientTest != null)
            {
                // Load existing result if available
                var existingResult = SelectedPatientTest.TestResults?.FirstOrDefault();
                if (existingResult != null)
                {
                    ResultValue = existingResult.Value ?? string.Empty;
                    Notes = existingResult.Notes ?? string.Empty;
                }
                else
                {
                    ResultValue = string.Empty;
                    Notes = string.Empty;
                }
                
                OnPropertyChanged(nameof(TestName));
                OnPropertyChanged(nameof(TestUnit));
                OnPropertyChanged(nameof(NormalRange));
                
                CheckNormalRange();
            }
        }

        private void CheckNormalRange()
        {
            IsOutOfRange = false;
            
            if (SelectedPatientTest?.TestType != null && !string.IsNullOrWhiteSpace(ResultValue))
            {
                var testType = SelectedPatientTest.TestType;
                
                // Try to parse as numeric value
                if (double.TryParse(ResultValue, out double numericValue))
                {
                    if (testType.MinNormalValue.HasValue && numericValue < testType.MinNormalValue.Value)
                    {
                        IsOutOfRange = true;
                    }
                    else if (testType.MaxNormalValue.HasValue && numericValue > testType.MaxNormalValue.Value)
                    {
                        IsOutOfRange = true;
                    }
                }
            }
        }

        private async Task SaveResultAsync()
        {
            if (!ValidateResult())
                return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var existingResult = SelectedPatientTest!.TestResults?.FirstOrDefault();
                
                if (existingResult != null)
                {
                    // Update existing result
                    existingResult.Value = ResultValue.Trim();
                    existingResult.Notes = Notes.Trim();
                    existingResult.IsNormal = !IsOutOfRange;
                    existingResult.EnteredBy = _authenticationService.CurrentUser?.Username ?? "Unknown";
                    existingResult.EnteredDate = DateTime.Now;
                    
                    await _testService.UpdateTestResultAsync(existingResult);
                }
                else
                {
                    // Create new result
                    var testResult = new TestResult
                    {
                        PatientTestId = SelectedPatientTest!.PatientTestId,
                        TestTypeId = SelectedPatientTest.TestTypeId,
                        Value = ResultValue.Trim(),
                        Notes = Notes.Trim(),
                        IsNormal = !IsOutOfRange,
                        EnteredBy = _authenticationService.CurrentUser?.Username ?? "Unknown",
                        EnteredDate = DateTime.Now,
                        ResultDate = DateTime.Now
                    };
                    
                    await _testService.CreateTestResultAsync(testResult);
                }

                // Update test status to completed
                SelectedPatientTest.Status = "Completed";
                SelectedPatientTest.CompletedDate = DateTime.Now;
                await _testService.UpdatePatientTestAsync(SelectedPatientTest);

                MessageBox.Show("تم حفظ النتيجة بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Refresh the test lists
                if (SelectedPatient != null)
                {
                    await LoadPatientTestsAsync(SelectedPatient.PatientId);
                }
                
                ClearResult();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في حفظ النتيجة: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateResult()
        {
            if (SelectedPatientTest == null)
            {
                ErrorMessage = "لم يتم اختيار تحليل";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ResultValue))
            {
                ErrorMessage = "قيمة النتيجة مطلوبة";
                return false;
            }

            return true;
        }

        private bool CanSaveResult()
        {
            return SelectedPatientTest != null && 
                   !string.IsNullOrWhiteSpace(ResultValue) && 
                   CanEnterResults;
        }

        private void ClearResult()
        {
            ResultValue = string.Empty;
            Notes = string.Empty;
            ErrorMessage = string.Empty;
            IsOutOfRange = false;
            SelectedPatientTest = null;
        }

        private async Task LoadPendingTestsAsync()
        {
            try
            {
                IsLoading = true;
                var allTests = await _testService.GetAllPatientTestsAsync();
                var pendingTests = allTests.Where(t => t.Status == "Pending" || t.Status == "InProgress")
                                          .OrderBy(t => t.OrderDate)
                                          .ToList();

                PendingTests.Clear();
                foreach (var test in pendingTests)
                {
                    PendingTests.Add(test);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل التحاليل المعلقة: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task RefreshDataAsync()
        {
            if (SelectedPatient != null)
            {
                await LoadPatientTestsAsync(SelectedPatient.PatientId);
            }
            else
            {
                await LoadPendingTestsAsync();
            }
        }

        private async Task MarkTestInProgressAsync()
        {
            if (SelectedPatientTest == null) return;

            try
            {
                SelectedPatientTest.Status = "InProgress";
                await _testService.UpdatePatientTestAsync(SelectedPatientTest);
                
                MessageBox.Show("تم تحديث حالة التحليل إلى 'جاري'", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحديث حالة التحليل: {ex.Message}";
            }
        }

        private bool CanMarkInProgress()
        {
            return SelectedPatientTest != null && 
                   SelectedPatientTest.Status == "Pending" && 
                   CanEnterResults;
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
