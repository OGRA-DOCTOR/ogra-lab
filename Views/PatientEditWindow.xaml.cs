using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OGRALAB.Models;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class PatientEditWindow : Window
    {
        public PatientEditViewModel ViewModel { get; }

        public PatientEditWindow(Patient? patient = null)
        {
            InitializeComponent();
            ViewModel = App.GetService<PatientEditViewModel>();
            ViewModel.CurrentPatient = patient;
            ViewModel.CloseWindow = () => this.Close();
            DataContext = ViewModel;
        }

        private void AvailableTest_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem listBoxItem && listBoxItem.DataContext is TestType testType)
            {
                ViewModel.AddTestCommand.Execute(testType);
            }
        }

        private void SelectedTest_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem listBoxItem && listBoxItem.DataContext is TestType testType)
            {
                ViewModel.RemoveTestCommand.Execute(testType);
            }
        }
    }
}
