using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OGRALAB.Services;
using OGRALAB.Models;
using OGRALAB.Views;
using OGRALAB.Helpers;

namespace OGRALAB.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authenticationService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _showPassword = false;
        private bool _isLoading = false;
        private string _errorMessage = string.Empty;

        public LoginViewModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
            LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
            CancelCommand = new RelayCommand(Cancel);
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ErrorMessage = string.Empty;
                    ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool ShowPassword
        {
            get => _showPassword;
            set => SetProperty(ref _showPassword, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<User>? LoginSuccessful;
        public event Action? LoginCancelled;

        private bool CanLogin()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private async Task LoginAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var user = await _authenticationService.AuthenticateAsync(Username.Trim(), Password);

                if (user != null)
                {
                    LoginSuccessful?.Invoke(user);
                }
                else
                {
                    ErrorMessage = "اسم المستخدم أو كلمة المرور غير صحيحة";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تسجيل الدخول: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void Cancel()
        {
            LoginCancelled?.Invoke();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }
