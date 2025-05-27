using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using OGRALAB.Services;
using OGRALAB.Views;

namespace OGRALAB.UserControls
{
    public partial class PatientManagementControl : UserControl
    {
        private readonly IPatientService _patientService;
        private readonly ITestService _testService;
        private DispatcherTimer _refreshTimer;

        public PatientManagementControl()
        {
            InitializeComponent();
            
            _patientService = App.GetService<IPatientService>();
            _testService = App.GetService<ITestService>();
            
            Loaded += PatientManagementControl_Loaded;
            
            // Setup refresh timer
            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // Refresh every minute
            };
            _refreshTimer.Tick += RefreshTimer_Tick;
            _refreshTimer.Start();
        }

        private async void PatientManagementControl_Loaded(object sender, RoutedEventArgs e)
        {
            await RefreshStatisticsAsync();
        }

        private async void RefreshTimer_Tick(object? sender, EventArgs e)
        {
            LastUpdateText.Text = DateTime.Now.ToString("HH:mm:ss");
            await RefreshStatisticsAsync();
        }

        private async Task RefreshStatisticsAsync()
        {
            try
            {
                // Get all patients
                var allPatients = await _patientService.GetAllPatientsAsync();
                
                // Update statistics
                TotalPatientsText.Text = allPatients.Count().ToString();
                
                // New patients this month
                var thisMonth = DateTime.Now.Month;
                var thisYear = DateTime.Now.Year;
                var newThisMonth = allPatients.Count(p => p.CreatedDate.Month == thisMonth && p.CreatedDate.Year == thisYear);
                NewThisMonthText.Text = newThisMonth.ToString();
                
                // Get test statistics
                var allTests = await _testService.GetAllPatientTestsAsync();
                var activeTests = allTests.Count(t => t.Status == "Pending" || t.Status == "InProgress");
                var pendingResults = allTests.Count(t => t.Status == "Pending");
                
                ActiveTestsText.Text = activeTests.ToString();
                PendingResultsText.Text = pendingResults.ToString();
                
                // Update recent patients list
                var recentPatients = allPatients
                    .OrderByDescending(p => p.CreatedDate)
                    .Take(5)
                    .ToList();
                
                RecentPatientsListBox.ItemsSource = recentPatients;
            }
            catch (Exception ex)
            {
                // Silently handle errors to avoid disrupting UI
                System.Diagnostics.Debug.WriteLine($"Error refreshing statistics: {ex.Message}");
            }
        }

        private void AddPatient_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var addPatientWindow = new PatientEditWindow();
                addPatientWindow.Owner = Window.GetWindow(this);
                var result = addPatientWindow.ShowDialog();
                
                if (result == true)
                {
                    _ = RefreshStatisticsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إضافة المريض: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewPatients_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var patientListWindow = new PatientListWindow();
                patientListWindow.Owner = Window.GetWindow(this);
                patientListWindow.ShowDialog();
                
                _ = RefreshStatisticsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح قائمة المرضى: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void QuickSearch_Click(object sender, MouseButtonEventArgs e)
        {
            SearchTextBox.Focus();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearchAsync();
        }

        private async void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformSearchAsync();
            }
        }

        private async Task PerformSearchAsync()
        {
            var searchText = SearchTextBox.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                MessageBox.Show("الرجاء إدخال نص للبحث", "تنبيه", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var allPatients = await _patientService.GetAllPatientsAsync();
                var foundPatients = allPatients.Where(p => 
                    p.FullName.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                    (p.PatientNumber != null && p.PatientNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (p.PhoneNumber != null && p.PhoneNumber.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                ).ToList();

                if (foundPatients.Any())
                {
                    var patientListWindow = new PatientListWindow();
                    patientListWindow.Owner = Window.GetWindow(this);
                    
                    // Set initial search
                    patientListWindow.ViewModel.SearchText = searchText;
                    patientListWindow.ViewModel.SearchCommand.Execute(null);
                    
                    patientListWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show($"لم يتم العثور على أي مريض يحتوي على '{searchText}'", "نتائج البحث", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Clear();
            SearchTextBox.Focus();
        }
    }
}
