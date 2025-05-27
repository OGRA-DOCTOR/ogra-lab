using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OGRALAB.ViewModels;
using OGRALAB.Models;

namespace OGRALAB.UserControls
{
    public partial class SearchEditControl : UserControl
    {
        public SearchEditViewModel ViewModel { get; }

        public SearchEditControl()
        {
            InitializeComponent();
            ViewModel = App.GetService<SearchEditViewModel>();
            DataContext = ViewModel;
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.SearchCommand.Execute(null);
            }
        }

        private void TestItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is PatientTest patientTest)
            {
                // You can add specific logic here if needed
                // For now, the selection is handled via data binding
            }
        }

        private void EditTestResult_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PatientTest patientTest)
            {
                ViewModel.EditTestResultCommand.Execute(patientTest);
            }
        }
    }
}
