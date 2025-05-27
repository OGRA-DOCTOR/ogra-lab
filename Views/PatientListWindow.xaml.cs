using System.Windows;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class PatientListWindow : Window
    {
        public PatientListViewModel ViewModel { get; }

        public PatientListWindow()
        {
            InitializeComponent();
            ViewModel = App.GetService<PatientListViewModel>();
            DataContext = ViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
