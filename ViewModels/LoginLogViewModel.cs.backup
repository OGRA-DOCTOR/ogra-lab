using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Helpers;

namespace OGRALAB.ViewModels
{
    public class LoginLogViewModel : INotifyPropertyChanged
    {
        private readonly OgraLabDbContext _context;
        private readonly IAuthenticationService _authenticationService;
        private ObservableCollection<LoginLog> _loginLogs;
        private LoginLog? _selectedLoginLog;
        private bool _isLoading;
        private DateTime _fromDate;
        private DateTime _toDate;

        public LoginLogViewModel(OgraLabDbContext context, IAuthenticationService authenticationService)
        {
            _context = context;
            _authenticationService = authenticationService;
            _loginLogs = new ObservableCollection<LoginLog>();
            
            // Set default date range (last 30 days)
            _toDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
            _fromDate = DateTime.Now.Date.AddDays(-30);

            // Commands
            LoadLogsCommand = new RelayCommand(async () => await LoadLogsAsync());
            RefreshCommand = new RelayCommand(async () => await LoadLogsAsync());
            ClearLogsCommand = new RelayCommand(async () => await ClearLogsAsync(), CanClearLogs);
            ExportLogsCommand = new RelayCommand(ExportLogs);

            // Load initial data
            _ = LoadLogsAsync();
        }

        public ObservableCollection<LoginLog> LoginLogs
        {
            get => _loginLogs;
            set => SetProperty(ref _loginLogs, value);
        }

        public LoginLog? SelectedLoginLog
        {
            get => _selectedLoginLog;
            set => SetProperty(ref _selectedLoginLog, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public DateTime FromDate
        {
            get => _fromDate;
            set => SetProperty(ref _fromDate, value);
        }

        public DateTime ToDate
        {
            get => _toDate;
            set => SetProperty(ref _toDate, value);
        }

        public bool CanDeleteLogs => _authenticationService.CurrentUser?.Role == "SystemUser";

        public ICommand LoadLogsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ClearLogsCommand { get; }
        public ICommand ExportLogsCommand { get; }

        private async Task LoadLogsAsync()
        {
            try
            {
                IsLoading = true;
                
                var logs = await _context.LoginLogs
                    .Include(l => l.User)
                    .Where(l => l.ActionDate >= FromDate && l.ActionDate <= ToDate)
                    .OrderByDescending(l => l.ActionDate)
                    .ToListAsync();

                LoginLogs.Clear();
                foreach (var log in logs)
                {
                    LoginLogs.Add(log);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل سجل الدخول: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ClearLogsAsync()
        {
            try
            {
                var result = MessageBox.Show(
                    "هل أنت متأكد من حذف جميع سجلات الدخول؟\nهذا الإجراء لا يمكن التراجع عنه.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    IsLoading = true;

                    var logsToDelete = await _context.LoginLogs
                        .Where(l => l.ActionDate >= FromDate && l.ActionDate <= ToDate)
                        .ToListAsync();

                    _context.LoginLogs.RemoveRange(logsToDelete);
                    await _context.SaveChangesAsync();

                    await LoadLogsAsync();

                    MessageBox.Show($"تم حذف {logsToDelete.Count} سجل بنجاح", "تم الحذف", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في حذف السجلات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanClearLogs()
        {
            return CanDeleteLogs && !IsLoading;
        }

        private void ExportLogs()
        {
            try
            {
                // TODO: Implement export functionality
                MessageBox.Show("سيتم تنفيذ خاصية التصدير في المرحلة التالية", "قيد التطوير", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تصدير السجلات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
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
