using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using OGRALAB.ViewModels;
using OGRALAB.Models;

namespace OGRALAB.Views
{
    public partial class LoginWindow : Window
    {
        public LoginViewModel ViewModel { get; }
        
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            ViewModel = viewModel;
            DataContext = ViewModel;
            
            // Subscribe to events
            ViewModel.LoginSuccessful += OnLoginSuccessful;
            ViewModel.LoginCancelled += OnLoginCancelled;
            
            // Set focus to username field
            Loaded += (s, e) => UsernameTextBox.Focus();
        }

        private void OnLoginSuccessful(User user)
        {
            DialogResult = true;
            Close();
        }

        private void OnLoginCancelled()
        {
            DialogResult = false;
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (e.Key == Key.Enter && ViewModel.LoginCommand.CanExecute(null))
            {
                ViewModel.LoginCommand.Execute(null);
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        // Make window draggable
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ViewModel.LoginSuccessful -= OnLoginSuccessful;
            ViewModel.LoginCancelled -= OnLoginCancelled;
            base.OnClosing(e);
        }
    }
}
