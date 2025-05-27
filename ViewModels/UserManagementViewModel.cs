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
using OGRALAB.Settings;

namespace OGRALAB.ViewModels
{
    public class UserManagementViewModel : INotifyPropertyChanged
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authenticationService;
        private ObservableCollection<User> _users;
        private User? _selectedUser;
        private bool _isLoading;
        private bool _isEditing;
        private string _searchText = string.Empty;
        
        // Form fields
        private string _username = string.Empty;
        private string _fullName = string.Empty;
        private string _email = string.Empty;
        private string _phoneNumber = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _selectedRole = "RegularUser";
        private bool _isActive = true;
        private string _errorMessage = string.Empty;

        public UserManagementViewModel(IUserService userService, IAuthenticationService authenticationService)
        {
            _userService = userService;
            _authenticationService = authenticationService;
            _users = new ObservableCollection<User>();

            // Commands
            LoadUsersCommand = new RelayCommand(async () => await LoadUsersAsync());
            AddUserCommand = new RelayCommand(AddUser);
            EditUserCommand = new RelayCommand(EditUser, CanEditUser);
            DeleteUserCommand = new RelayCommand(async () => await DeleteUserAsync(), CanDeleteUser);
            SaveUserCommand = new RelayCommand(async () => await SaveUserAsync(), CanSaveUser);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SearchCommand = new RelayCommand(async () => await SearchUsersAsync());
            ResetPasswordCommand = new RelayCommand(async () => await ResetPasswordAsync(), CanResetPassword);

            // Available roles
            AvailableRoles = new[] { "SystemUser", "AdminUser", "RegularUser" };

            // Load initial data
            _ = LoadUsersAsync();
        }

        // Properties
        public ObservableCollection<User> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetProperty(ref _selectedUser, value))
                {
                    ((RelayCommand)EditUserCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeleteUserCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ResetPasswordCommand).RaiseCanExecuteChanged();
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
            set
            {
                if (SetProperty(ref _isEditing, value))
                {
                    ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        // Form Properties
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetProperty(ref _confirmPassword, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)SaveUserCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string SelectedRole
        {
            get => _selectedRole;
            set => SetProperty(ref _selectedRole, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string[] AvailableRoles { get; }

        public bool CanManageUsers => _authenticationService.CurrentUser?.Role == "SystemUser";

        // Commands
        public ICommand LoadUsersCommand { get; }
        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand SaveUserCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ResetPasswordCommand { get; }

        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                var users = await _userService.GetAllUsersAsync();
                
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المستخدمين: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void AddUser()
        {
            ClearForm();
            IsEditing = true;
            SelectedUser = null;
        }

        private void EditUser()
        {
            if (SelectedUser != null)
            {
                Username = SelectedUser.Username;
                FullName = SelectedUser.FullName;
                Email = SelectedUser.Email;
                PhoneNumber = SelectedUser.PhoneNumber ?? string.Empty;
                SelectedRole = SelectedUser.Role;
                IsActive = SelectedUser.IsActive;
                Password = string.Empty;
                ConfirmPassword = string.Empty;
                IsEditing = true;
            }
        }

        private bool CanEditUser()
        {
            return SelectedUser != null && CanManageUsers;
        }

        private async Task DeleteUserAsync()
        {
            if (SelectedUser == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف المستخدم '{SelectedUser.FullName}'؟\nهذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var success = await _userService.DeleteUserAsync(SelectedUser.UserId);
                    
                    if (success)
                    {
                        await LoadUsersAsync();
                        SelectedUser = null;
                        MessageBox.Show("تم حذف المستخدم بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في حذف المستخدم", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف المستخدم: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private bool CanDeleteUser()
        {
            return SelectedUser != null && CanManageUsers && 
                   SelectedUser.UserId != _authenticationService.CurrentUser?.UserId;
        }

        private async Task SaveUserAsync()
        {
            try
            {
                ErrorMessage = string.Empty;

                // Validation
                if (!ValidateForm())
                    return;

                IsLoading = true;

                if (SelectedUser == null) // Adding new user
                {
                    var newUser = new User
                    {
                        Username = Username.Trim(),
                        FullName = FullName.Trim(),
                        Email = Email.Trim(),
                        PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
                        Role = SelectedRole,
                        IsActive = IsActive
                    };

                    await _userService.CreateUserAsync(newUser, Password);
                    MessageBox.Show("تم إضافة المستخدم بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else // Updating existing user
                {
                    SelectedUser.Username = Username.Trim();
                    SelectedUser.FullName = FullName.Trim();
                    SelectedUser.Email = Email.Trim();
                    SelectedUser.PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim();
                    SelectedUser.Role = SelectedRole;
                    SelectedUser.IsActive = IsActive;

                    await _userService.UpdateUserAsync(SelectedUser);

                    // Update password if provided
                    if (!string.IsNullOrWhiteSpace(Password))
                    {
                        await _userService.ChangePasswordAsync(SelectedUser.UserId, Password);
                    }

                    MessageBox.Show("تم تحديث المستخدم بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await LoadUsersAsync();
                CancelEdit();
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanSaveUser()
        {
            return IsEditing && !string.IsNullOrWhiteSpace(Username) && 
                   !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Email) &&
                   (SelectedUser != null || !string.IsNullOrWhiteSpace(Password));
        }

        private void CancelEdit()
        {
            ClearForm();
            IsEditing = false;
            SelectedUser = null;
            ErrorMessage = string.Empty;
        }

        private async Task SearchUsersAsync()
        {
            try
            {
                IsLoading = true;
                
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    await LoadUsersAsync();
                    return;
                }

                var allUsers = await _userService.GetAllUsersAsync();
                var filteredUsers = allUsers.Where(u => 
                    u.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.Username.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(SearchText)));

                Users.Clear();
                foreach (var user in filteredUsers)
                {
                    Users.Add(user);
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

        private async Task ResetPasswordAsync()
        {
            if (SelectedUser == null) return;

            var newPassword = SecuritySettings.DefaultPassword;
            var result = MessageBox.Show(
                $"هل تريد إعادة تعيين كلمة مرور المستخدم '{SelectedUser.FullName}' إلى كلمة المرور الافتراضية؟",
                "إعادة تعيين كلمة المرور",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var success = await _userService.ChangePasswordAsync(SelectedUser.UserId, newPassword);
                    
                    if (success)
                    {
                        MessageBox.Show("تم إعادة تعيين كلمة المرور بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في إعادة تعيين كلمة المرور", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في إعادة تعيين كلمة المرور: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private bool CanResetPassword()
        {
            return SelectedUser != null && CanManageUsers && 
                   SelectedUser.UserId != _authenticationService.CurrentUser?.UserId;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "اسم المستخدم مطلوب";
                return false;
            }

            if (!ValidationHelper.IsValidUsername(Username))
            {
                ErrorMessage = "اسم المستخدم غير صحيح. يجب أن يكون 3-50 حرف، أحرف وأرقام و _ فقط";
                return false;
            }

            if (string.IsNullOrWhiteSpace(FullName))
            {
                ErrorMessage = "الاسم الكامل مطلوب";
                return false;
            }

            if (!ValidationHelper.IsValidName(FullName))
            {
                ErrorMessage = "الاسم الكامل غير صحيح";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "البريد الإلكتروني مطلوب";
                return false;
            }

            if (!ValidationHelper.IsValidEmail(Email))
            {
                ErrorMessage = "البريد الإلكتروني غير صحيح";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(PhoneNumber) && !ValidationHelper.IsValidPhoneNumber(PhoneNumber))
            {
                ErrorMessage = "رقم الهاتف غير صحيح";
                return false;
            }

            if (SelectedUser == null) // New user
            {
                if (string.IsNullOrWhiteSpace(Password))
                {
                    ErrorMessage = "كلمة المرور مطلوبة للمستخدم الجديد";
                    return false;
                }

                if (!PasswordHelper.IsValidPassword(Password))
                {
                    ErrorMessage = "كلمة المرور ضعيفة. يجب أن تكون على الأقل 6 أحرف";
                    return false;
                }

                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين";
                    return false;
                }
            }
            else if (!string.IsNullOrWhiteSpace(Password)) // Updating password
            {
                if (!PasswordHelper.IsValidPassword(Password))
                {
                    ErrorMessage = "كلمة المرور ضعيفة. يجب أن تكون على الأقل 6 أحرف";
                    return false;
                }

                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "كلمة المرور وتأكيد كلمة المرور غير متطابقتين";
                    return false;
                }
            }

            return true;
        }

        private void ClearForm()
        {
            Username = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            SelectedRole = "RegularUser";
            IsActive = true;
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
