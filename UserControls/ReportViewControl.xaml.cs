using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OGRALAB.ViewModels;
using OGRALAB.Views;
using OGRALAB.Models;

namespace OGRALAB.UserControls
{
    public partial class ReportViewControl : UserControl
    {
        public ReportViewViewModel ViewModel { get; }

        public ReportViewControl()
        {
            InitializeComponent();
            ViewModel = App.GetService<ReportViewViewModel>();
            DataContext = ViewModel;
        }

        private void PreviewReport_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.PreviewReportCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في معاينة التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintReport_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ViewModel.PrintReportCommand.Execute(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في طباعة التقرير: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ManageTestGroups_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var testGroupsWindow = new TestGroupsManagementWindow();
                testGroupsWindow.Owner = Application.Current.MainWindow;
                testGroupsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إدارة المجموعات: {ex.Message}", "خطأ", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CompletedTest_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is PatientTest patientTest)
            {
                ViewModel.SelectedTest = patientTest;
            }
        }

        private void PreviewTestResult_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PatientTest patientTest)
            {
                try
                {
                    var patient = ViewModel.SelectedPatient;
                    if (patient != null)
                    {
                        var reportPreviewWindow = new ReportPreviewWindow(patient, new[] { patientTest });
                        reportPreviewWindow.Owner = Application.Current.MainWindow;
                        reportPreviewWindow.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في معاينة النتيجة: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void PrintTestResult_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PatientTest patientTest)
            {
                try
                {
                    var patient = ViewModel.SelectedPatient;
                    if (patient != null)
                    {
                        var reportPreviewWindow = new ReportPreviewWindow(patient, new[] { patientTest }, true);
                        reportPreviewWindow.Owner = Application.Current.MainWindow;
                        reportPreviewWindow.ShowDialog();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في طباعة النتيجة: {ex.Message}", "خطأ", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
