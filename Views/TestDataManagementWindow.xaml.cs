using System;
using System.Windows;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    /// <summary>
    /// Interaction logic for TestDataManagementWindow.xaml
    /// </summary>
    public partial class TestDataManagementWindow : Window
    {
        public TestDataManagementWindow()
        {
            InitializeComponent();
            
            // Get ViewModel from DI container
            var serviceProvider = (IServiceProvider)Application.Current.Properties["ServiceProvider"];
            if (serviceProvider != null)
            {
                DataContext = serviceProvider.GetService<TestDataManagementViewModel>();
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is TestDataManagementViewModel viewModel)
            {
                await viewModel.LoadDataAsync();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Cleanup ViewModel if needed
            if (DataContext is TestDataManagementViewModel viewModel)
            {
                viewModel.Cleanup();
            }
            
            base.OnClosed(e);
        }
    }
}
