using System.Windows;
using OGRALAB.ViewModels;

namespace OGRALAB.Views
{
    public partial class TestTypesManagementWindow : Window
    {
        public TestTypesManagementViewModel ViewModel { get; }

        public TestTypesManagementWindow()
        {
            InitializeComponent();
            ViewModel = App.GetService<TestTypesManagementViewModel>();
            DataContext = ViewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
