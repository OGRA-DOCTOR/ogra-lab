using System.Windows;
using System.Windows.Input;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class TestResultEntryWindow : Window
    {
        public TestResultEntryViewModel ViewModel { get; }

        public TestResultEntryWindow()
        {
            InitializeComponent();
            ViewModel = App.GetService<TestResultEntryViewModel>();
            DataContext = ViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.SearchPatientCommand.Execute(null);
            }
        }
    }
}
