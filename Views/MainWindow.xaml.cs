using System;
using System.Windows;
using System.Windows.Threading;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer _clockTimer;
        private readonly DispatcherTimer _statusTimer;

        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = App.GetService<MainViewModel>();
            DataContext = ViewModel;

            // Initialize timers
            _clockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _clockTimer.Tick += ClockTimer_Tick;
            _clockTimer.Start();

            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _statusTimer.Tick += StatusTimer_Tick;
            _statusTimer.Start();

            // Set initial time
            UpdateClock();
            UpdateStatusTime();

            // Set initial status
            ViewModel.StatusMessage = "جاهز";
            ViewModel.DatabaseStatus = "متصل";

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load initial data
            ViewModel.LoadDataCommand?.Execute(null);
        }

        private void ClockTimer_Tick(object? sender, EventArgs e)
        {
            UpdateClock();
        }

        private void StatusTimer_Tick(object? sender, EventArgs e)
        {
            UpdateStatusTime();
        }

        private void UpdateClock()
        {
            ClockLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void UpdateStatusTime()
        {
            StatusTimeLabel.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        }

        protected override void OnClosed(EventArgs e)
        {
            _clockTimer?.Stop();
            _statusTimer?.Stop();
            base.OnClosed(e);
        }
    }
}
