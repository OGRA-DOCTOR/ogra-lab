using System.Windows;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class LoginLogWindow : Window
    {
        public LoginLogWindow()
        {
            InitializeComponent();
            
            // Create and set the view model
            var viewModel = App.GetService<LoginLogViewModel>();
            DataContext = viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
