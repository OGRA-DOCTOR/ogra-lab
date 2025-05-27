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
    public class TestGroupsManagementViewModel : INotifyPropertyChanged
    {
        private readonly ITestService _testService;
        private readonly IAuthenticationService _authenticationService;
        
        private ObservableCollection<TestGroup> _testGroups;
        private TestGroup? _selectedTestGroup;
        private ObservableCollection<TestType> _availableTestTypes;
        private ObservableCollection<TestType> _selectedTestTypes;
        private TestType? _selectedAvailableTestType;
        private TestType? _selectedGroupTestType;
        private bool _isLoading;
        private string _groupName = string.Empty;
        private string _groupDescription = string.Empty;
        private bool _isEditMode;

        public TestGroupsManagementViewModel(ITestService testService, IAuthenticationService authenticationService)
        {
            _testService = testService;
            _authenticationService = authenticationService;
            
            _testGroups = new ObservableCollection<TestGroup>();
            _availableTestTypes = new ObservableCollection<TestType>();
            _selectedTestTypes = new ObservableCollection<TestType>();
            
            // Commands
            LoadDataCommand = new RelayCommand(async () => await LoadDataAsync());
            CreateGroupCommand = new RelayCommand(async () => await CreateGroupAsync(), CanCreateGroup);
            EditGroupCommand = new RelayCommand(async () => await EditGroupAsync(), CanEditGroup);
            DeleteGroupCommand = new RelayCommand(async () => await DeleteGroupAsync(), CanDeleteGroup);
            AddTestTypeCommand = new RelayCommand(AddTestTypeToGroup, CanAddTestType);
            RemoveTestTypeCommand = new RelayCommand(RemoveTestTypeFromGroup, CanRemoveTestType);
            SaveGroupCommand = new RelayCommand(async () => await SaveGroupAsync(), CanSaveGroup);
            CancelEditCommand = new RelayCommand(CancelEdit);
            ClearFormCommand = new RelayCommand(ClearForm);
            
            // Load initial data
            _ = LoadDataAsync();
        }

        // Properties
        public ObservableCollection<TestGroup> TestGroups
        {
            get => _testGroups;
            set => SetProperty(ref _testGroups, value);
        }

        public TestGroup? SelectedTestGroup
        {
            get => _selectedTestGroup;
            set
            {
                if (SetProperty(ref _selectedTestGroup, value))
                {
                    if (value != null)
                    {
                        LoadGroupForEdit(value);
                    }
                    
                    ((RelayCommand)EditGroupCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteGroupCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<TestType> AvailableTestTypes
        {
            get => _availableTestTypes;
            set => SetProperty(ref _availableTestTypes, value);
        }

        public ObservableCollection<TestType> SelectedTestTypes
        {
            get => _selectedTestTypes;
            set => SetProperty(ref _selectedTestTypes, value);
        }

        public TestType? SelectedAvailableTestType
        {
            get => _selectedAvailableTestType;
            set
            {
                SetProperty(ref _selectedAvailableTestType, value);
                ((RelayCommand)AddTestTypeCommand).RaiseCanExecuteChanged();
            }
        }

        public TestType? SelectedGroupTestType
        {
            get => _selectedGroupTestType;
            set
            {
                SetProperty(ref _selectedGroupTestType, value);
                ((RelayCommand)RemoveTestTypeCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string GroupName
        {
            get => _groupName;
            set
            {
                SetProperty(ref _groupName, value);
                ((RelayCommand)CreateGroupCommand).RaiseCanExecuteChanged();
                ((RelayCommand)SaveGroupCommand).RaiseCanExecuteChanged();
            }
        }

        public string GroupDescription
        {
            get => _groupDescription;
            set => SetProperty(ref _groupDescription, value);
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set => SetProperty(ref _isEditMode, value);
        }

        // Display Properties
        public int TestGroupsCount => TestGroups.Count;
        public int SelectedTestTypesCount => SelectedTestTypes.Count;
        public bool HasTestGroups => TestGroups.Any();
        public bool HasSelectedTestTypes => SelectedTestTypes.Any();

        // Permissions
        public bool CanManageTestGroups => _authenticationService.CurrentUser?.Role != null && 
                                          (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                           _authenticationService.CurrentUser.Role == "AdminUser");

        // Commands
        public ICommand LoadDataCommand { get; }
        public ICommand CreateGroupCommand { get; }
        public ICommand EditGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand AddTestTypeCommand { get; }
        public ICommand RemoveTestTypeCommand { get; }
        public ICommand SaveGroupCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand ClearFormCommand { get; }

        // Methods
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                
                // Load test groups
                var groups = await _testService.GetAllTestGroupsAsync();
                TestGroups.Clear();
                foreach (var group in groups.OrderBy(g => g.GroupName))
                {
                    TestGroups.Add(group);
                }

                // Load available test types
                var testTypes = await _testService.GetAllTestTypesAsync();
                AvailableTestTypes.Clear();
                foreach (var testType in testTypes.OrderBy(tt => tt.TestName))
                {
                    AvailableTestTypes.Add(testType);
                }

                OnPropertyChanged(nameof(TestGroupsCount));
                OnPropertyChanged(nameof(HasTestGroups));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateGroupAsync()
        {
            if (!CanManageTestGroups)
            {
                MessageBox.Show("ليس لديك صلاحية لإنشاء مجموعات التحاليل", "تنبيه", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate minimum test types
            if (SelectedTestTypes.Count < 2)
            {
                MessageBox.Show("لا يمكن انشاء مجموعة تتكون من نوع تحليل واحد", "تنبيه", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                var newGroup = new TestGroup
                {
                    GroupName = GroupName.Trim(),
                    Description = GroupDescription.Trim(),
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = _authenticationService.CurrentUser?.Username ?? "System"
                };

                var createdGroup = await _testService.CreateTestGroupAsync(newGroup);

                // Add test types to the group
                foreach (var testType in SelectedTestTypes)
                {
                    await _testService.AddTestTypeToGroupAsync(createdGroup.TestGroupId, testType.TestTypeId);
                }

                MessageBox.Show("تم إنشاء مجموعة التحاليل بنجاح", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                ClearForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في إنشاء مجموعة التحاليل: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task EditGroupAsync()
        {
            if (SelectedTestGroup == null) return;

            IsEditMode = true;
            GroupName = SelectedTestGroup.GroupName;
            GroupDescription = SelectedTestGroup.Description ?? string.Empty;

            // Load group's test types
            var groupTestTypes = await _testService.GetTestTypesByGroupIdAsync(SelectedTestGroup.TestGroupId);
            SelectedTestTypes.Clear();
            foreach (var testType in groupTestTypes)
            {
                SelectedTestTypes.Add(testType);
            }

            OnPropertyChanged(nameof(SelectedTestTypesCount));
            OnPropertyChanged(nameof(HasSelectedTestTypes));
        }

        private async Task SaveGroupAsync()
        {
            if (SelectedTestGroup == null || !IsEditMode) return;

            if (!CanManageTestGroups)
            {
                MessageBox.Show("ليس لديك صلاحية لتعديل مجموعات التحاليل", "تنبيه", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate minimum test types
            if (SelectedTestTypes.Count < 2)
            {
                MessageBox.Show("لا يمكن حفظ مجموعة تتكون من نوع تحليل واحد", "تنبيه", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                IsLoading = true;

                // Update group details
                SelectedTestGroup.GroupName = GroupName.Trim();
                SelectedTestGroup.Description = GroupDescription.Trim();
                SelectedTestGroup.ModifiedDate = DateTime.Now;
                SelectedTestGroup.ModifiedBy = _authenticationService.CurrentUser?.Username ?? "System";

                await _testService.UpdateTestGroupAsync(SelectedTestGroup);

                // Update group's test types
                await _testService.UpdateTestGroupTestTypesAsync(SelectedTestGroup.TestGroupId, 
                    SelectedTestTypes.Select(tt => tt.TestTypeId).ToList());

                MessageBox.Show("تم حفظ التغييرات بنجاح", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                IsEditMode = false;
                ClearForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حفظ التغييرات: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteGroupAsync()
        {
            if (SelectedTestGroup == null) return;

            if (!CanManageTestGroups)
            {
                MessageBox.Show("ليس لديك صلاحية لحذف مجموعات التحاليل", "تنبيه", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف مجموعة التحاليل '{SelectedTestGroup.GroupName}'؟\nهذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;

                await _testService.DeleteTestGroupAsync(SelectedTestGroup.TestGroupId);

                MessageBox.Show("تم حذف مجموعة التحاليل بنجاح", "نجح", 
                    MessageBoxButton.OK, MessageBoxImage.Information);

                SelectedTestGroup = null;
                ClearForm();
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حذف مجموعة التحاليل: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddTestTypeToGroup()
        {
            if (SelectedAvailableTestType == null) return;

            // Check if already added
            if (SelectedTestTypes.Any(tt => tt.TestTypeId == SelectedAvailableTestType.TestTypeId))
            {
                MessageBox.Show("هذا النوع من التحاليل مضاف بالفعل للمجموعة", "تنبيه", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedTestTypes.Add(SelectedAvailableTestType);
            SelectedAvailableTestType = null;

            OnPropertyChanged(nameof(SelectedTestTypesCount));
            OnPropertyChanged(nameof(HasSelectedTestTypes));
            ((RelayCommand)CreateGroupCommand).RaiseCanExecuteChanged();
            ((RelayCommand)SaveGroupCommand).RaiseCanExecuteChanged();
        }

        private void RemoveTestTypeFromGroup()
        {
            if (SelectedGroupTestType == null) return;

            SelectedTestTypes.Remove(SelectedGroupTestType);
            SelectedGroupTestType = null;

            OnPropertyChanged(nameof(SelectedTestTypesCount));
            OnPropertyChanged(nameof(HasSelectedTestTypes));
            ((RelayCommand)CreateGroupCommand).RaiseCanExecuteChanged();
            ((RelayCommand)SaveGroupCommand).RaiseCanExecuteChanged();
        }

        private void LoadGroupForEdit(TestGroup group)
        {
            GroupName = group.GroupName;
            GroupDescription = group.Description ?? string.Empty;
            IsEditMode = false;
            SelectedTestTypes.Clear();
            
            OnPropertyChanged(nameof(SelectedTestTypesCount));
            OnPropertyChanged(nameof(HasSelectedTestTypes));
        }

        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedTestGroup = null;
            ClearForm();
        }

        private void ClearForm()
        {
            GroupName = string.Empty;
            GroupDescription = string.Empty;
            SelectedTestTypes.Clear();
            SelectedAvailableTestType = null;
            SelectedGroupTestType = null;
            
            OnPropertyChanged(nameof(SelectedTestTypesCount));
            OnPropertyChanged(nameof(HasSelectedTestTypes));
        }

        // Command conditions
        private bool CanCreateGroup()
        {
            return !string.IsNullOrWhiteSpace(GroupName) && 
                   SelectedTestTypes.Count >= 2 && 
                   !IsEditMode &&
                   CanManageTestGroups;
        }

        private bool CanEditGroup()
        {
            return SelectedTestGroup != null && !IsEditMode && CanManageTestGroups;
        }

        private bool CanDeleteGroup()
        {
            return SelectedTestGroup != null && !IsEditMode && CanManageTestGroups;
        }

        private bool CanSaveGroup()
        {
            return !string.IsNullOrWhiteSpace(GroupName) && 
                   SelectedTestTypes.Count >= 2 && 
                   IsEditMode &&
                   CanManageTestGroups;
        }

        private bool CanAddTestType()
        {
            return SelectedAvailableTestType != null;
        }

        private bool CanRemoveTestType()
        {
            return SelectedGroupTestType != null;
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
