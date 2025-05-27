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
    public class PatientEditViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private readonly ITestService _testService;
        private Patient? _currentPatient;
        private bool _isLoading;
        private bool _isEditing;
        private string _errorMessage = string.Empty;
        
        // Patient fields
        private string _patientCode = string.Empty;
        private string _title = string.Empty;
        private string _patientName = string.Empty;
        private int _age = 0;
        private string _ageUnit = "Years";
        private string _sex = "Unknown";
        private string _doctorTitle = string.Empty;
        private string _doctorName = string.Empty;
        private bool _printEnabled = true;
        
        // Available collections
        private ObservableCollection<TestType> _availableTests;
        private ObservableCollection<TestType> _selectedTests;
        private ObservableCollection<TestGroup> _customGroups;
        private bool _showRoutineTests = true;

        public PatientEditViewModel(IPatientService patientService, ITestService testService)
        {
            _patientService = patientService;
            _testService = testService;
            
            _availableTests = new ObservableCollection<TestType>();
            _selectedTests = new ObservableCollection<TestType>();
            _customGroups = new ObservableCollection<TestGroup>();
            
            // Commands
            SavePatientCommand = new RelayCommand(async () => await SavePatientAsync(), CanSavePatient);
            CancelCommand = new RelayCommand(Cancel);
            AddTestCommand = new RelayCommand<TestType>(AddTest);
            RemoveTestCommand = new RelayCommand<TestType>(RemoveTest);
            AddSelectedTestCommand = new RelayCommand(AddSelectedTest);
            RemoveSelectedTestCommand = new RelayCommand(RemoveSelectedTest);
            AddAllTestsCommand = new RelayCommand(AddAllTests);
            RemoveAllTestsCommand = new RelayCommand(RemoveAllTests);
            ToggleTestViewCommand = new RelayCommand(ToggleTestView);
            
            // Available options
            AvailableTitles = new[] { "السيد", "السيدة", "الدكتور", "الدكتورة", "المهندس", "المهندسة", "الأستاذ", "الأستاذة" };
            AgeUnits = new[] { "Years", "Months", "Days" };
            SexOptions = new[] { "Male", "Female", "Unknown" };
            DoctorTitles = new[] { "د.", "أ.د.", "أ.م.د.", "طبيب", "استشاري" };
            
            // Load initial data
            _ = LoadTestDataAsync();
        }

        // Properties
        public Patient? CurrentPatient
        {
            get => _currentPatient;
            set
            {
                if (SetProperty(ref _currentPatient, value))
                {
                    LoadPatientData();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Patient Properties
        public string PatientCode
        {
            get => _patientCode;
            set => SetProperty(ref _patientCode, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string PatientName
        {
            get => _patientName;
            set
            {
                if (SetProperty(ref _patientName, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)SavePatientCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public int Age
        {
            get => _age;
            set
            {
                if (SetProperty(ref _age, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)SavePatientCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string AgeUnit
        {
            get => _ageUnit;
            set => SetProperty(ref _ageUnit, value);
        }

        public string Sex
        {
            get => _sex;
            set => SetProperty(ref _sex, value);
        }

        public string DoctorTitle
        {
            get => _doctorTitle;
            set => SetProperty(ref _doctorTitle, value);
        }

        public string DoctorName
        {
            get => _doctorName;
            set => SetProperty(ref _doctorName, value);
        }

        public bool PrintEnabled
        {
            get => _printEnabled;
            set => SetProperty(ref _printEnabled, value);
        }

        // Test Collections
        public ObservableCollection<TestType> AvailableTests
        {
            get => _availableTests;
            set => SetProperty(ref _availableTests, value);
        }

        public ObservableCollection<TestType> SelectedTests
        {
            get => _selectedTests;
            set => SetProperty(ref _selectedTests, value);
        }

        public ObservableCollection<TestGroup> CustomGroups
        {
            get => _customGroups;
            set => SetProperty(ref _customGroups, value);
        }

        public bool ShowRoutineTests
        {
            get => _showRoutineTests;
            set
            {
                if (SetProperty(ref _showRoutineTests, value))
                {
                    _ = LoadTestDataAsync();
                }
            }
        }

        // Available Options
        public string[] AvailableTitles { get; }
        public string[] AgeUnits { get; }
        public string[] SexOptions { get; }
        public string[] DoctorTitles { get; }

        // Commands
        public ICommand SavePatientCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand AddTestCommand { get; }
        public ICommand RemoveTestCommand { get; }
        public ICommand AddSelectedTestCommand { get; }
        public ICommand RemoveSelectedTestCommand { get; }
        public ICommand AddAllTestsCommand { get; }
        public ICommand RemoveAllTestsCommand { get; }
        public ICommand ToggleTestViewCommand { get; }

        public string WindowTitle => CurrentPatient == null ? "إضافة مريض جديد" : "تعديل بيانات المريض";

        // Methods
        private async Task LoadTestDataAsync()
        {
            try
            {
                IsLoading = true;
                
                if (ShowRoutineTests)
                {
                    var allTests = await _testService.GetAllTestTypesAsync();
                    // Optimize: Avoid ToList() and use direct enumeration
                    var routineTests = allTests.Where(t => !t.IsCustom);
                    
                    AvailableTests.Clear();
                    foreach (var test in routineTests)
                    {
                        // Optimize: Check existence more efficiently
                        if (!SelectedTests.Any(st => st.TestTypeId == test.TestTypeId))
                        {
                            AvailableTests.Add(test);
                        }
                    }
                }
                else
                {
                    var groups = await _testService.GetAllTestGroupsAsync();
                    CustomGroups.Clear();
                    foreach (var group in groups)
                    {
                        CustomGroups.Add(group);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل التحاليل: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadPatientData()
        {
            if (CurrentPatient != null)
            {
                IsEditing = true;
                PatientCode = CurrentPatient.PatientNumber ?? string.Empty;
                Title = CurrentPatient.Title ?? string.Empty;
                PatientName = CurrentPatient.FullName;
                Age = CurrentPatient.Age;
                AgeUnit = CurrentPatient.AgeUnit ?? "Years";
                Sex = CurrentPatient.Gender ?? "Unknown";
                DoctorTitle = CurrentPatient.DoctorTitle ?? string.Empty;
                DoctorName = CurrentPatient.DoctorName ?? string.Empty;
                PrintEnabled = CurrentPatient.IsPrintEnabled;
                
                // Load patient's tests
                LoadPatientTests();
            }
            else
            {
                IsEditing = false;
                ClearForm();
            }
        }

        private async void LoadPatientTests()
        {
            if (CurrentPatient == null) return;
            
            try
            {
                var patientTests = await _testService.GetPatientTestsAsync(CurrentPatient.PatientId);
                
                SelectedTests.Clear();
                foreach (var patientTest in patientTests)
                {
                    if (patientTest.TestType != null)
                    {
                        SelectedTests.Add(patientTest.TestType);
                    }
                }
                
                await LoadTestDataAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل تحاليل المريض: {ex.Message}";
            }
        }

        private void ClearForm()
        {
            PatientCode = string.Empty;
            Title = string.Empty;
            PatientName = string.Empty;
            Age = 0;
            AgeUnit = "Years";
            Sex = "Unknown";
            DoctorTitle = string.Empty;
            DoctorName = string.Empty;
            PrintEnabled = true;
            SelectedTests.Clear();
            ErrorMessage = string.Empty;
        }

        private async Task SavePatientAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                
                if (!ValidateForm())
                    return;

                IsLoading = true;

                var patient = CurrentPatient ?? new Patient();
                
                // Set patient properties
                patient.Title = Title;
                patient.FullName = PatientName.Trim();
                patient.Age = Age;
                patient.AgeUnit = AgeUnit;
                patient.Gender = Sex;
                patient.DoctorTitle = DoctorTitle;
                patient.DoctorName = DoctorName.Trim();
                patient.IsPrintEnabled = PrintEnabled;
                patient.CreatedDate = CurrentPatient?.CreatedDate ?? DateTime.Now;
                patient.UpdatedDate = DateTime.Now;

                Patient savedPatient;
                if (CurrentPatient == null)
                {
                    // Generate patient code
                    patient.PatientNumber = await _patientService.GeneratePatientNumberAsync();
                    savedPatient = await _patientService.CreatePatientAsync(patient);
                }
                else
                {
                    savedPatient = await _patientService.UpdatePatientAsync(patient);
                }

                // Save selected tests
                await SavePatientTestsAsync(savedPatient);

                // Update display
                CurrentPatient = savedPatient;
                PatientCode = savedPatient.PatientNumber ?? string.Empty;

                MessageBox.Show(
                    CurrentPatient == null ? "تم إضافة المريض بنجاح" : "تم تحديث بيانات المريض بنجاح",
                    "نجح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Close window
                CloseWindow?.Invoke();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في حفظ المريض: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }



        private async Task SavePatientTestsAsync(Patient patient)
        {
            try
            {
                // Remove existing tests
                var existingTests = await _testService.GetPatientTestsAsync(patient.PatientId);
                foreach (var existingTest in existingTests)
                {
                    await _testService.DeletePatientTestAsync(existingTest.PatientTestId);
                }

                // Add selected tests
                foreach (var testType in SelectedTests)
                {
                    var patientTest = new PatientTest
                    {
                        PatientId = patient.PatientId,
                        TestTypeId = testType.TestTypeId,
                        OrderDate = DateTime.Now,
                        Status = "Pending"
                    };
                    
                    await _testService.CreatePatientTestAsync(patientTest);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطأ في حفظ تحاليل المريض: {ex.Message}");
            }
        }

        private bool ValidateForm()
        {
            // Security validation first
            if (!SecurityHelper.IsSafeInput(PatientName))
            {
                ErrorMessage = "اسم المريض يحتوي على رموز غير مسموحة";
                return false;
            }

            if (!SecurityHelper.IsSafeInput(DoctorName))
            {
                ErrorMessage = "اسم الطبيب يحتوي على رموز غير مسموحة";
                return false;
            }

            // Required field validation
            if (string.IsNullOrWhiteSpace(PatientName))
            {
                ErrorMessage = "اسم المريض مطلوب";
                return false;
            }

            // Name format validation
            if (!ValidationHelper.IsValidName(PatientName))
            {
                ErrorMessage = "اسم المريض يجب أن يحتوي على أحرف عربية أو إنجليزية فقط";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(DoctorName) && !ValidationHelper.IsValidName(DoctorName))
            {
                ErrorMessage = "اسم الطبيب يجب أن يحتوي على أحرف عربية أو إنجليزية فقط";
                return false;
            }

            // Age validation
            if (Age <= 0)
            {
                ErrorMessage = "السن يجب أن يكون أكبر من صفر";
                return false;
            }

            if (AgeUnit == "Years" && Age > 150)
            {
                ErrorMessage = "السن غير صحيح";
                return false;
            }

            if (AgeUnit == "Months" && Age > 1800)
            {
                ErrorMessage = "السن بالشهور غير صحيح";
                return false;
            }

            if (AgeUnit == "Days" && Age > 36500)
            {
                ErrorMessage = "السن بالأيام غير صحيح";
                return false;
            }

            if (SelectedTests.Count == 0)
            {
                ErrorMessage = "يجب اختيار تحليل واحد على الأقل";
                return false;
            }

            return true;
        }

        private bool CanSavePatient()
        {
            return !string.IsNullOrWhiteSpace(PatientName) && Age > 0 && SelectedTests.Count > 0;
        }

        private void Cancel()
        {
            CloseWindow?.Invoke();
        }

        private void AddTest(TestType? test)
        {
            if (test != null && !SelectedTests.Any(st => st.TestTypeId == test.TestTypeId))
            {
                SelectedTests.Add(test);
                AvailableTests.Remove(test);
                ((RelayCommand)SavePatientCommand).RaiseCanExecuteChanged();
            }
        }

        private void RemoveTest(TestType? test)
        {
            if (test != null && SelectedTests.Contains(test))
            {
                SelectedTests.Remove(test);
                if (ShowRoutineTests && !test.IsCustom)
                {
                    AvailableTests.Add(test);
                }
                ((RelayCommand)SavePatientCommand).RaiseCanExecuteChanged();
            }
        }

        private void AddSelectedTest()
        {
            // Implementation for Add Selected Test button
        }

        private void RemoveSelectedTest()
        {
            // Implementation for Delete button
        }

        private void AddAllTests()
        {
            var testsToAdd = AvailableTests.ToList();
            foreach (var test in testsToAdd)
            {
                AddTest(test);
            }
        }

        private void RemoveAllTests()
        {
            var testsToRemove = SelectedTests.ToList();
            foreach (var test in testsToRemove)
            {
                RemoveTest(test);
            }
        }

        private void ToggleTestView()
        {
            ShowRoutineTests = !ShowRoutineTests;
        }

        public Action? CloseWindow { get; set; }

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
