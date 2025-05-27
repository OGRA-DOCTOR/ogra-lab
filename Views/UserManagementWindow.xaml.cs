using System.Windows;
using System.Windows.Controls;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class UserManagementWindow : Window
    {
        public UserManagementViewModel ViewModel { get; }

        public UserManagementWindow()
        {
            InitializeComponent();
            ViewModel = App.GetService<UserManagementViewModel>();
            DataContext = ViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UserManagementViewModel viewModel)
            {
                viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            }
        }
    }
}
