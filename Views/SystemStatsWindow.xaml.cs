using System;
using System.Windows;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class SystemStatsWindow : Window
    {
        private SystemStatsViewModel _viewModel;

        public SystemStatsWindow()
        {
            InitializeComponent();
            LoadViewModel();
        }

        public SystemStatsWindow(SystemStatsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void LoadViewModel()
        {
            try
            {
                if (Application.Current.Properties.ContainsKey("ServiceProvider"))
                {
                    var serviceProvider = (IServiceProvider)Application.Current.Properties["ServiceProvider"];
                    _viewModel = serviceProvider.GetRequiredService<SystemStatsViewModel>();
                    DataContext = _viewModel;
                }
                else
                {
                    MessageBox.Show("خطأ في تحميل خدمات النظام", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل نموذج البيانات: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize data when window is loaded
            if (_viewModel?.LoadStatsCommand?.CanExecute(null) == true)
            {
                _viewModel.LoadStatsCommand.Execute(null);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Clean up resources if needed
            _viewModel = null;
            DataContext = null;
            base.OnClosed(e);
        }
    }
}
