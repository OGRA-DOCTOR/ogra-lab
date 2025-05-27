using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OGRALAB.ViewModels;
using OGRALAB.Views;
using OGRALAB.Models;

namespace OGRALAB.UserControls
{
    public partial class ResultEntryControl : UserControl
    {
        public ResultEntryControlViewModel ViewModel { get; }

        public ResultEntryControl()
        {
            InitializeComponent();
            ViewModel = App.GetService<ResultEntryControlViewModel>();
            DataContext = ViewModel;
        }

        private void OpenResultEntryWindow_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var testResultEntryWindow = new TestResultEntryWindow();
                testResultEntryWindow.Owner = Application.Current.MainWindow;
                testResultEntryWindow.ShowDialog();
                
                // Refresh data after window closes
                _ = ViewModel.RefreshCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إدخال النتائج: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenTestManagement_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var testTypesWindow = new TestTypesManagementWindow();
                testTypesWindow.Owner = Application.Current.MainWindow;
                testTypesWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إدارة التحاليل: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenQuickSearch_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Navigate to search page in main window
                var mainWindow = Application.Current.MainWindow as Views.MainWindow;
                if (mainWindow != null)
                {
                    // Switch to Search/Edit tab - this would need implementation in MainWindow
                    // For now, just show a message
                    MessageBox.Show("انتقل إلى تبويب 'البحث والتعديل' للبحث عن التحاليل", "إرشاد", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في التنقل: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PendingTest_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is PatientTest patientTest)
            {
                try
                {
                    var testResultEntryWindow = new TestResultEntryWindow();
                    testResultEntryWindow.Owner = Application.Current.MainWindow;
                    
                    // Set the selected patient and test in the window
                    testResultEntryWindow.ViewModel.SelectedPatient = patientTest.Patient;
                    testResultEntryWindow.ViewModel.SelectedPatientTest = patientTest;
                    
                    testResultEntryWindow.ShowDialog();
                    
                    // Refresh data after window closes
                    _ = ViewModel.RefreshCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في فتح نافذة إدخال النتائج: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RecentResult_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is TestResult testResult)
            {
                try
                {
                    var testResultEntryWindow = new TestResultEntryWindow();
                    testResultEntryWindow.Owner = Application.Current.MainWindow;
                    
                    // Set the selected patient and test for viewing/editing
                    testResultEntryWindow.ViewModel.SelectedPatient = testResult.PatientTest?.Patient;
                    testResultEntryWindow.ViewModel.SelectedPatientTest = testResult.PatientTest;
                    
                    testResultEntryWindow.ShowDialog();
                    
                    // Refresh data after window closes
                    _ = ViewModel.RefreshCommand.Execute(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في فتح نافذة إدخال النتائج: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void RefreshPendingTests_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.RefreshCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحديث البيانات: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
