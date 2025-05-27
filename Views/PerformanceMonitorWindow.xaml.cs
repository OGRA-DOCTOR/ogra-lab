using System;
using System.Windows;
using System.Windows.Threading;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    /// <summary>
    /// Interaction logic for PerformanceMonitorWindow.xaml
    /// </summary>
    public partial class PerformanceMonitorWindow : Window
    {
        private DispatcherTimer _refreshTimer;
        private PerformanceMonitorViewModel _viewModel;

        public PerformanceMonitorWindow()
        {
            InitializeComponent();
            
            // Get ViewModel from DI container
            var serviceProvider = (IServiceProvider)Application.Current.Properties["ServiceProvider"];
            if (serviceProvider != null)
            {
                _viewModel = serviceProvider.GetService<PerformanceMonitorViewModel>();
                DataContext = _viewModel;
            }

            // Set up auto-refresh timer
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5) // Refresh every 5 seconds
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                await _viewModel.LoadDataAsync();
            }
        }

        private async void RefreshTimer_Tick(object sender, EventArgs e)
        {
            if (_viewModel != null && _viewModel.AutoRefreshEnabled)
            {
                await _viewModel.RefreshMetricsAsync();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Stop timer and cleanup
            _refreshTimer?.Stop();
            _refreshTimer = null;
            
            // Cleanup ViewModel if needed
            _viewModel?.Cleanup();
            
            base.OnClosed(e);
        }
    }
}
