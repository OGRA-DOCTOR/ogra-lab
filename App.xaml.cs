using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OGRALAB.Data;
using OGRALAB.Services;
using OGRALAB.Views;
using OGRALAB.ViewModels;
using System;
using System.IO;
using System.Windows;

namespace OGRALAB
{
    public partial class App : Application
    {
        private IHost? _host;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Create host builder
                var hostBuilder = Host.CreateDefaultBuilder()
                    .ConfigureServices(ConfigureServices);

                _host = hostBuilder.Build();

                // Initialize database
                using (var scope = _host.Services.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<OgraLabDbContext>();
                    context.Database.EnsureCreated();
                }

                // Start the host
                _host.Start();

                // Show login window
                ShowLoginWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء بدء التطبيق: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        public void ShowLoginWindow()
        {
            var authService = GetService<IAuthenticationService>();
            var loginViewModel = new LoginViewModel(authService);
            var loginWindow = new LoginWindow(loginViewModel);

            var result = loginWindow.ShowDialog();
            
            if (result == true && authService.IsAuthenticated)
            {
                // Show main window
                var mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                // User cancelled login, exit application
                Shutdown();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add DbContext
            services.AddDbContext<OgraLabDbContext>(options =>
            {
                var dataDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                if (!Directory.Exists(dataDirectory))
                {
                    Directory.CreateDirectory(dataDirectory);
                }
                var connectionString = Path.Combine(dataDirectory, "OgraLab.db");
                options.UseSqlite($"Data Source={connectionString}");
            });

            // Add services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPatientService, PatientService>();
            services.AddScoped<ITestService, TestService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();

            // Add ViewModels
            services.AddTransient<LoginViewModel>();
            services.AddTransient<MainViewModel>();
            services.AddTransient<LoginLogViewModel>();
            services.AddTransient<UserManagementViewModel>();
            services.AddTransient<PatientEditViewModel>();
            services.AddTransient<PatientListViewModel>();
            services.AddTransient<TestResultEntryViewModel>();
            services.AddTransient<SearchEditViewModel>();
            services.AddTransient<ResultEntryControlViewModel>();
            services.AddTransient<TestTypesManagementViewModel>();
            services.AddTransient<ReportViewViewModel>();
            services.AddTransient<TestGroupsManagementViewModel>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _host?.Dispose();
            base.OnExit(e);
        }

        public static T GetService<T>() where T : class
        {
            var app = (App)Current;
            return app._host?.Services.GetRequiredService<T>() 
                ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
        }
    }
}
