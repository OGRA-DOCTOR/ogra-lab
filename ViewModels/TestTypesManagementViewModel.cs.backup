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

namespace OGRALAB.ViewModels
{
    public class TestTypesManagementViewModel : INotifyPropertyChanged
    {
        private readonly ITestService _testService;
        private readonly IAuthenticationService _authenticationService;
        
        private ObservableCollection<TestType> _testTypes;
        private TestType? _selectedTestType;
        private TestType? _editingTestType;
        private bool _isLoading;
        private bool _isEditMode;
        private string _searchText = string.Empty;
        private string _errorMessage = string.Empty;

        // Form Properties
        private string _testName = string.Empty;
        private string _testCode = string.Empty;
        private string _unit = string.Empty;
        private string _category = string.Empty;
        private double? _minNormalValue;
        private double? _maxNormalValue;
        private string _normalRange = string.Empty;
        private string _description = string.Empty;
        private bool _isActive = true;

        public TestTypesManagementViewModel(ITestService testService, IAuthenticationService authenticationService)
        {
            _testService = testService;
            _authenticationService = authenticationService;
            
            _testTypes = new ObservableCollection<TestType>();
            
            // Commands
            AddTestTypeCommand = new RelayCommand(AddTestType, CanModifyTestTypes);
            EditTestTypeCommand = new RelayCommand(EditTestType, CanEditTestType);
            SaveTestTypeCommand = new RelayCommand(async () => await SaveTestTypeAsync(), CanSaveTestType);
            CancelEditCommand = new RelayCommand(CancelEdit);
            DeleteTestTypeCommand = new RelayCommand(async () => await DeleteTestTypeAsync(), CanDeleteTestType);
            SearchCommand = new RelayCommand(async () => await SearchTestTypesAsync());
            RefreshCommand = new RelayCommand(async () => await LoadTestTypesAsync());
            
            // Categories for dropdown
            Categories = new[] { "كيميائية", "هرمونية", "مناعية", "خلوية", "ميكروبيولوجية", "جزيئية", "أخرى" };
            
            // Load initial data
            _ = LoadTestTypesAsync();
        }

        // Properties
        public ObservableCollection<TestType> TestTypes
        {
            get => _testTypes;
            set => SetProperty(ref _testTypes, value);
        }

        public TestType? SelectedTestType
        {
            get => _selectedTestType;
            set
            {
                if (SetProperty(ref _selectedTestType, value))
                {
                    ((RelayCommand)EditTestTypeCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteTestTypeCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        // Form Properties
        public string TestName
        {
            get => _testName;
            set
            {
                SetProperty(ref _testName, value);
                ((RelayCommand)SaveTestTypeCommand).RaiseCanExecuteChanged();
            }
        }

        public string TestCode
        {
            get => _testCode;
            set
            {
                SetProperty(ref _testCode, value);
                ((RelayCommand)SaveTestTypeCommand).RaiseCanExecuteChanged();
            }
        }

        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public double? MinNormalValue
        {
            get => _minNormalValue;
            set
            {
                SetProperty(ref _minNormalValue, value);
                UpdateNormalRange();
            }
        }

        public double? MaxNormalValue
        {
            get => _maxNormalValue;
            set
            {
                SetProperty(ref _maxNormalValue, value);
                UpdateNormalRange();
            }
        }

        public string NormalRange
        {
            get => _normalRange;
            set => SetProperty(ref _normalRange, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string[] Categories { get; }

        // Display Properties
        public string FormTitle => IsEditMode ? "تعديل نوع التحليل" : "إضافة نوع تحليل جديد";
        public int TestTypesCount => TestTypes.Count;

        // Permissions
        public bool CanModifyTestTypes => _authenticationService.CurrentUser?.Role != null && 
                                         (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                          _authenticationService.CurrentUser.Role == "AdminUser");

        // Commands
        public ICommand AddTestTypeCommand { get; }
        public ICommand EditTestTypeCommand { get; }
        public ICommand SaveTestTypeCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteTestTypeCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand RefreshCommand { get; }

        // Methods
        private async Task LoadTestTypesAsync()
        {
            try
            {
                IsLoading = true;
                var testTypes = await _testService.GetAllTestTypesAsync();
                
                TestTypes.Clear();
                foreach (var testType in testTypes.OrderBy(t => t.TestName))
                {
                    TestTypes.Add(testType);
                }

                OnPropertyChanged(nameof(TestTypesCount));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل أنواع التحاليل: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchTestTypesAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadTestTypesAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var allTestTypes = await _testService.GetAllTestTypesAsync();
                var filteredTestTypes = allTestTypes.Where(t => 
                    t.TestName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    t.TestCode.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (t.Category?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ?? false));

                TestTypes.Clear();
                foreach (var testType in filteredTestTypes.OrderBy(t => t.TestName))
                {
                    TestTypes.Add(testType);
                }

                OnPropertyChanged(nameof(TestTypesCount));
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

        private void AddTestType()
        {
            IsEditMode = true;
            _editingTestType = null;
            ClearForm();
        }

        private void EditTestType()
        {
            if (SelectedTestType == null) return;

            IsEditMode = true;
            _editingTestType = SelectedTestType;
            LoadTestTypeToForm(SelectedTestType);
        }

        private async Task SaveTestTypeAsync()
        {
            if (!ValidateForm()) return;

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                if (_editingTestType == null)
                {
                    // Create new test type
                    var testType = new TestType
                    {
                        TestName = TestName.Trim(),
                        TestCode = TestCode.Trim(),
                        Unit = Unit.Trim(),
                        Category = Category,
                        MinNormalValue = MinNormalValue,
                        MaxNormalValue = MaxNormalValue,
                        NormalRange = NormalRange.Trim(),
                        Description = Description.Trim(),
                        IsActive = IsActive,
                        CreatedDate = DateTime.Now
                    };

                    await _testService.CreateTestTypeAsync(testType);
                    MessageBox.Show("تم إضافة نوع التحليل بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Update existing test type
                    _editingTestType.TestName = TestName.Trim();
                    _editingTestType.TestCode = TestCode.Trim();
                    _editingTestType.Unit = Unit.Trim();
                    _editingTestType.Category = Category;
                    _editingTestType.MinNormalValue = MinNormalValue;
                    _editingTestType.MaxNormalValue = MaxNormalValue;
                    _editingTestType.NormalRange = NormalRange.Trim();
                    _editingTestType.Description = Description.Trim();
                    _editingTestType.IsActive = IsActive;

                    await _testService.UpdateTestTypeAsync(_editingTestType);
                    MessageBox.Show("تم تحديث نوع التحليل بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                CancelEdit();
                await LoadTestTypesAsync();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في حفظ نوع التحليل: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteTestTypeAsync()
        {
            if (SelectedTestType == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف نوع التحليل '{SelectedTestType.TestName}'؟\n\nتحذير: سيؤثر هذا على جميع التحاليل المرتبطة بهذا النوع.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                await _testService.DeleteTestTypeAsync(SelectedTestType.TestTypeId);
                MessageBox.Show("تم حذف نوع التحليل بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                await LoadTestTypesAsync();
                SelectedTestType = null;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في حذف نوع التحليل: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void CancelEdit()
        {
            IsEditMode = false;
            _editingTestType = null;
            ClearForm();
            ErrorMessage = string.Empty;
        }

        private void LoadTestTypeToForm(TestType testType)
        {
            TestName = testType.TestName;
            TestCode = testType.TestCode;
            Unit = testType.Unit ?? string.Empty;
            Category = testType.Category ?? string.Empty;
            MinNormalValue = testType.MinNormalValue;
            MaxNormalValue = testType.MaxNormalValue;
            NormalRange = testType.NormalRange ?? string.Empty;
            Description = testType.Description ?? string.Empty;
            IsActive = testType.IsActive;
        }

        private void ClearForm()
        {
            TestName = string.Empty;
            TestCode = string.Empty;
            Unit = string.Empty;
            Category = string.Empty;
            MinNormalValue = null;
            MaxNormalValue = null;
            NormalRange = string.Empty;
            Description = string.Empty;
            IsActive = true;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(TestName))
            {
                ErrorMessage = "اسم التحليل مطلوب";
                return false;
            }

            if (string.IsNullOrWhiteSpace(TestCode))
            {
                ErrorMessage = "كود التحليل مطلوب";
                return false;
            }

            if (MinNormalValue.HasValue && MaxNormalValue.HasValue && MinNormalValue > MaxNormalValue)
            {
                ErrorMessage = "الحد الأدنى للقيمة الطبيعية يجب أن يكون أقل من الحد الأعلى";
                return false;
            }

            return true;
        }

        private void UpdateNormalRange()
        {
            if (MinNormalValue.HasValue && MaxNormalValue.HasValue)
            {
                NormalRange = $"{MinNormalValue:F2} - {MaxNormalValue:F2}";
            }
            else if (MinNormalValue.HasValue)
            {
                NormalRange = $"> {MinNormalValue:F2}";
            }
            else if (MaxNormalValue.HasValue)
            {
                NormalRange = $"< {MaxNormalValue:F2}";
            }
            else
            {
                NormalRange = string.Empty;
            }
        }

        private bool CanEditTestType()
        {
            return SelectedTestType != null && CanModifyTestTypes;
        }

        private bool CanSaveTestType()
        {
            return !string.IsNullOrWhiteSpace(TestName) && 
                   !string.IsNullOrWhiteSpace(TestCode) && 
                   CanModifyTestTypes;
        }

        private bool CanDeleteTestType()
        {
            return SelectedTestType != null && CanModifyTestTypes;
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
