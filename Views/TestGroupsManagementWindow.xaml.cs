using System;
using System.Windows;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class TestGroupsManagementWindow : Window
    {
        public TestGroupsManagementViewModel ViewModel { get; }

        public TestGroupsManagementWindow()
        {
            InitializeComponent();
            ViewModel = App.GetService<TestGroupsManagementViewModel>();
            DataContext = ViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            // Perform any cleanup if needed
            try
            {
                // Refresh data in parent windows if necessary
                if (Owner is MainWindow mainWindow)
                {
                    // You can trigger refresh of other components here
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show to user as window is closing
                System.Diagnostics.Debug.WriteLine($"Error during window cleanup: {ex.Message}");
            }
        }
    }
}
