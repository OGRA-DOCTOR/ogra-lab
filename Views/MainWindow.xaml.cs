using System;
using System.Windows;
using System.Windows.Threading;

namespace OGRALAB.Views
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer? _timer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // Set current date
            CurrentDateLabel.Text = $"التاريخ: {DateTime.Now.ToString("dd/MM/yyyy")}";
            
            // Initialize timer for status bar
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // Set initial status
            StatusLabel.Text = "جاهز";
            DatabaseStatusLabel.Text = "متصل بقاعدة البيانات";
            
            LoadDashboardData();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            TimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void LoadDashboardData()
        {
            try
            {
                // TODO: Load actual data from database
                // For now, set sample data
                TotalPatientsLabel.Text = "0";
                ActiveTestsLabel.Text = "0";
                PendingTestsLabel.Text = "0";
                CompletedTodayLabel.Text = "0";
                
                TodayPatientsCount.Text = "0";
                CompletedTestsCount.Text = "0";
                PendingTestsCount.Text = "0";
                
                StatusLabel.Text = "تم تحميل البيانات بنجاح";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabel.Text = "خطأ في تحميل البيانات";
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("هل تريد الخروج من البرنامج؟", "تأكيد الخروج", 
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer?.Stop();
            base.OnClosed(e);
        }
    }
}
