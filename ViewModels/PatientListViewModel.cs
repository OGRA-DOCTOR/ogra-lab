using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using OGRALAB.Models;
using OGRALAB.Services;
using OGRALAB.Helpers;
using OGRALAB.Views;

namespace OGRALAB.ViewModels
{
    public class PatientListViewModel : INotifyPropertyChanged
    {
        private readonly IPatientService _patientService;
        private readonly IAuthenticationService _authenticationService;
        private ObservableCollection<Patient> _patients;
        private ObservableCollection<Patient> _filteredPatients;
        private Patient? _selectedPatient;
        private bool _isLoading;
        private string _searchText = string.Empty;
        private string _searchType = "الاسم";
        private DateTime? _searchDateFrom;
        private DateTime? _searchDateTo;

        public PatientListViewModel(IPatientService patientService, IAuthenticationService authenticationService)
        {
            _patientService = patientService;
            _authenticationService = authenticationService;
            _patients = new ObservableCollection<Patient>();
            _filteredPatients = new ObservableCollection<Patient>();

            // Commands
            LoadPatientsCommand = new RelayCommand(async () => await LoadPatientsAsync());
            SearchCommand = new RelayCommand(async () => await SearchPatientsAsync());
            ClearSearchCommand = new RelayCommand(ClearSearch);
            AddPatientCommand = new RelayCommand(AddPatient, CanAddPatient);
            EditPatientCommand = new RelayCommand(EditPatient, CanEditPatient);
            DeletePatientCommand = new RelayCommand(async () => await DeletePatientAsync(), CanDeletePatient);
            ViewPatientCommand = new RelayCommand(ViewPatient, CanViewPatient);
            RefreshCommand = new RelayCommand(async () => await LoadPatientsAsync());

            // Search types
            SearchTypes = new[] { "الاسم", "رقم المريض", "رقم الهوية", "رقم الهاتف" };

            // Load initial data
            _ = LoadPatientsAsync();
        }

        // Properties
        public ObservableCollection<Patient> Patients
        {
            get => _patients;
            set => SetProperty(ref _patients, value);
        }

        public ObservableCollection<Patient> FilteredPatients
        {
            get => _filteredPatients;
            set => SetProperty(ref _filteredPatients, value);
        }

        public Patient? SelectedPatient
        {
            get => _selectedPatient;
            set
            {
                if (SetProperty(ref _selectedPatient, value))
                {
                    ((RelayCommand)EditPatientCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)DeletePatientCommand).RaiseCanExecuteChanged();
                    ((RelayCommand)ViewPatientCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string SearchType
        {
            get => _searchType;
            set => SetProperty(ref _searchType, value);
        }

        public DateTime? SearchDateFrom
        {
            get => _searchDateFrom;
            set => SetProperty(ref _searchDateFrom, value);
        }

        public DateTime? SearchDateTo
        {
            get => _searchDateTo;
            set => SetProperty(ref _searchDateTo, value);
        }

        public string[] SearchTypes { get; }

        public int TotalPatientsCount => Patients.Count;
        public int FilteredPatientsCount => FilteredPatients.Count;

        // Permissions
        public bool CanAddPatients => _authenticationService.CurrentUser?.Role != null && 
                                     (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                      _authenticationService.CurrentUser.Role == "AdminUser" || 
                                      _authenticationService.CurrentUser.Role == "RegularUser");

        public bool CanEditPatients => _authenticationService.CurrentUser?.Role != null && 
                                      (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                       _authenticationService.CurrentUser.Role == "AdminUser" || 
                                       _authenticationService.CurrentUser.Role == "RegularUser");

        public bool CanDeletePatients => _authenticationService.CurrentUser?.Role != null && 
                                        (_authenticationService.CurrentUser.Role == "SystemUser" || 
                                         _authenticationService.CurrentUser.Role == "AdminUser");

        // Commands
        public ICommand LoadPatientsCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand AddPatientCommand { get; }
        public ICommand EditPatientCommand { get; }
        public ICommand DeletePatientCommand { get; }
        public ICommand ViewPatientCommand { get; }
        public ICommand RefreshCommand { get; }

        // Methods
        private async Task LoadPatientsAsync()
        {
            try
            {
                IsLoading = true;
                var patients = await _patientService.GetAllPatientsAsync();
                
                Patients.Clear();
                FilteredPatients.Clear();
                
                foreach (var patient in patients.OrderByDescending(p => p.CreatedDate))
                {
                    Patients.Add(patient);
                    FilteredPatients.Add(patient);
                }

                OnPropertyChanged(nameof(TotalPatientsCount));
                OnPropertyChanged(nameof(FilteredPatientsCount));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل المرضى: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SearchPatientsAsync()
        {
            try
            {
                IsLoading = true;
                
                var allPatients = Patients.AsEnumerable();

                // Apply text search
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    switch (SearchType)
                    {
                        case "الاسم":
                            allPatients = allPatients.Where(p => p.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                            break;
                        case "رقم المريض":
                            allPatients = allPatients.Where(p => p.PatientNumber != null && 
                                                              p.PatientNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                            break;
                        case "رقم الهوية":
                            allPatients = allPatients.Where(p => p.NationalId != null && 
                                                              p.NationalId.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                            break;
                        case "رقم الهاتف":
                            allPatients = allPatients.Where(p => p.PhoneNumber != null && 
                                                              p.PhoneNumber.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                            break;
                    }
                }

                // Apply date range search
                if (SearchDateFrom.HasValue)
                {
                    allPatients = allPatients.Where(p => p.CreatedDate.Date >= SearchDateFrom.Value.Date);
                }

                if (SearchDateTo.HasValue)
                {
                    allPatients = allPatients.Where(p => p.CreatedDate.Date <= SearchDateTo.Value.Date);
                }

                FilteredPatients.Clear();
                foreach (var patient in allPatients.OrderByDescending(p => p.CreatedDate))
                {
                    FilteredPatients.Add(patient);
                }

                OnPropertyChanged(nameof(FilteredPatientsCount));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في البحث: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
            SearchDateFrom = null;
            SearchDateTo = null;
            
            FilteredPatients.Clear();
            foreach (var patient in Patients)
            {
                FilteredPatients.Add(patient);
            }
            
            OnPropertyChanged(nameof(FilteredPatientsCount));
        }

        private void AddPatient()
        {
            try
            {
                var addPatientWindow = new PatientEditWindow();
                addPatientWindow.Owner = Application.Current.MainWindow;
                var result = addPatientWindow.ShowDialog();
                
                if (result == true)
                {
                    _ = LoadPatientsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة إضافة المريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanAddPatient()
        {
            return CanAddPatients;
        }

        private void EditPatient()
        {
            if (SelectedPatient == null) return;

            try
            {
                var editPatientWindow = new PatientEditWindow(SelectedPatient);
                editPatientWindow.Owner = Application.Current.MainWindow;
                var result = editPatientWindow.ShowDialog();
                
                if (result == true)
                {
                    _ = LoadPatientsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في فتح نافذة تعديل المريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditPatient()
        {
            return SelectedPatient != null && CanEditPatients;
        }

        private async Task DeletePatientAsync()
        {
            if (SelectedPatient == null) return;

            var result = MessageBox.Show(
                $"هل أنت متأكد من حذف المريض '{SelectedPatient.FullName}'؟\nسيتم حذف جميع التحاليل المرتبطة بهذا المريض.\nهذا الإجراء لا يمكن التراجع عنه.",
                "تأكيد الحذف",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    IsLoading = true;
                    var success = await _patientService.DeletePatientAsync(SelectedPatient.PatientId);
                    
                    if (success)
                    {
                        await LoadPatientsAsync();
                        SelectedPatient = null;
                        MessageBox.Show("تم حذف المريض بنجاح", "نجح", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("فشل في حذف المريض", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"خطأ في حذف المريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        private bool CanDeletePatient()
        {
            return SelectedPatient != null && CanDeletePatients;
        }

        private void ViewPatient()
        {
            if (SelectedPatient == null) return;

            try
            {
                var viewPatientWindow = new PatientEditWindow(SelectedPatient);
                viewPatientWindow.Owner = Application.Current.MainWindow;
                viewPatientWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في عرض بيانات المريض: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanViewPatient()
        {
            return SelectedPatient != null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
