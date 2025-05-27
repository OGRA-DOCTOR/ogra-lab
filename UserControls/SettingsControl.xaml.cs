using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using OGRALAB.ViewModels;
using OGRALAB.Views;

namespace OGRALAB.UserControls
{
    public partial class SettingsControl : UserControl
    {
        private SettingsViewModel _viewModel;

        public SettingsControl()
        {
            InitializeComponent();
            LoadViewModel();
            Loaded += UserControl_Loaded;
            Unloaded += UserControl_Unloaded;
        }

        private void LoadViewModel()
        {
            try
            {
                if (Application.Current.Properties.ContainsKey("ServiceProvider"))
                {
                    var serviceProvider = (IServiceProvider)Application.Current.Properties["ServiceProvider"];
                    _viewModel = serviceProvider.GetRequiredService<SettingsViewModel>();
                    DataContext = _viewModel;
                }
                else
                {
                    MessageBox.Show("خطأ في تحميل خدمات النظام", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل نموذج البيانات: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Event handlers for management tools
        private void ManageTestTypes_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_viewModel?.ManageTestTypesCommand?.CanExecute(null) == true)
                {
                    _viewModel.ManageTestTypesCommand.Execute(null);
                }
                else
                {
                    MessageBox.Show("ليس لديك صلاحية لإدارة أنواع التحاليل", "تنبيه", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إدارة أنواع التحاليل: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageUsers_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_viewModel?.ManageUsersCommand?.CanExecute(null) == true)
                {
                    _viewModel.ManageUsersCommand.Execute(null);
                }
                else
                {
                    MessageBox.Show("ليس لديك صلاحية لإدارة المستخدمين", "تنبيه", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إدارة المستخدمين: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewLoginLogs_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_viewModel?.ViewLoginLogsCommand?.CanExecute(null) == true)
                {
                    _viewModel.ViewLoginLogsCommand.Execute(null);
                }
                else
                {
                    MessageBox.Show("ليس لديك صلاحية لعرض سجل الدخول", "تنبيه", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة سجل الدخول: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewSystemStats_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (_viewModel?.ViewSystemStatsCommand?.CanExecute(null) == true)
                {
                    _viewModel.ViewSystemStatsCommand.Execute(null);
                }
                else
                {
                    MessageBox.Show("ليس لديك صلاحية لعرض إحصائيات النظام", "تنبيه", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إحصائيات النظام: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize ViewModel when the control is loaded
            if (_viewModel != null && _viewModel.LoadSettingsCommand?.CanExecute(null) == true)
            {
                _viewModel.LoadSettingsCommand.Execute(null);
            }
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Check for unsaved changes before unloading
            if (_viewModel?.HasUnsavedChanges == true)
            {
                var result = MessageBox.Show(
                    "لديك تغييرات غير محفوظة. هل تريد حفظها قبل الخروج؟",
                    "تغييرات غير محفوظة",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    // Save changes
                    if (_viewModel.SaveLabSettingsCommand?.CanExecute(null) == true)
                    {
                        _viewModel.SaveLabSettingsCommand.Execute(null);
                    }
                    if (_viewModel.SaveSystemSettingsCommand?.CanExecute(null) == true)
                    {
                        _viewModel.SaveSystemSettingsCommand.Execute(null);
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    // Cancel unloading (if possible)
                    e.Handled = true;
                    return;
                }
            }
        }
    }
}
