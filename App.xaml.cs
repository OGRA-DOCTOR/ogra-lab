using Microsoft.EntityFrameworkCore;
using OGRALAB.Data;
using OGRALAB.Services;
using OGRALAB.Views;
using OGRALAB.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace OGRALAB
{
    public partial class App : Application
    {
        private IHost? _host;
        private ILoggingService? _loggingService;
        private IErrorHandlingService? _errorHandlingService;
        private IPerformanceService? _performanceService;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Create host builder
                var hostBuilder = Host.CreateDefaultBuilder()
                    .ConfigureServices(ConfigureServices);

                _host = hostBuilder.Build();

                // Initialize database
                {
                    var context = scope.ServiceProvider.GetRequiredService<OgraLabDbContext>();
                    context.Database.EnsureCreated();
                }

                // Start the host
                _host.Start();

                // Store service provider for global access
                Current.Properties["ServiceProvider"] = _host.Services;

                // Initialize core services
                _loggingService = _host.Services.GetRequiredService<ILoggingService>();
                _errorHandlingService = _host.Services.GetRequiredService<IErrorHandlingService>();
                _performanceService = _host.Services.GetRequiredService<IPerformanceService>();

                // Set up global exception handlers
                SetupExceptionHandlers();

                // Start performance monitoring
                _performanceService.StartPerformanceMonitoring();

                // Log application startup
                await _loggingService.LogInfoAsync("تم بدء تشغيل OGRA LAB بنجاح", "Application");

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
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<IBackupService, BackupService>();
            services.AddScoped<IStatsService, StatsService>();
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IErrorHandlingService, ErrorHandlingService>();
            services.AddSingleton<IPerformanceService, PerformanceService>();
            services.AddScoped<ITestDataService, TestDataService>();
            services.AddScoped<IDatabaseOptimizationService, DatabaseOptimizationService>();
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
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<SystemStatsViewModel>();
            services.AddTransient<BackupManagementViewModel>();
            services.AddTransient<TestDataManagementViewModel>();
            services.AddTransient<PerformanceMonitorViewModel>();
        }

        private void SetupExceptionHandlers()
        {
            // Handle unhandled exceptions in main UI thread
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            // Handle unhandled exceptions in background threads
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            // Handle unhandled exceptions in tasks
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private async void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                await _loggingService?.LogErrorAsync("خطأ غير معالج في واجهة المستخدم", e.Exception, "Application");
                _errorHandlingService?.ShowError(e.Exception, "حدث خطأ غير متوقع في النظام");
                e.Handled = true; // Prevent application crash
            }
            catch
            {
                // Fallback error handling
                MessageBox.Show("حدث خطأ حرج في النظام", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                if (e.ExceptionObject is Exception exception)
                {
                    await _loggingService?.LogErrorAsync("خطأ غير معالج في النطاق", exception, "Application");
                    
                    if (!e.IsTerminating)
                    {
                        _errorHandlingService?.ShowError(exception, "حدث خطأ غير متوقع");
                    }
                }
            }
            catch
            {
                // Silent fail for shutdown errors
            }
        }

        private async void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                await _loggingService?.LogErrorAsync("خطأ غير معالج في مهمة خلفية", e.Exception, "Application");
                e.SetObserved(); // Prevent application crash
            }
            catch
            {
                // Silent fail
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                // Log application shutdown
                _loggingService?.LogInfoAsync("إغلاق OGRA LAB", "Application");
                
                // Stop performance monitoring
                _performanceService?.StopPerformanceMonitoring();
                
                // Cleanup resources
                _performanceService?.CleanupResources();
                _performanceService?.Dispose();
            }
            catch
            {
                // Silent fail during shutdown
            }
            finally
            {
                _host?.Dispose();
                base.OnExit(e);
            }
        }

        public static T GetService<T>() where T : class
        {
            var app = (App)Current;
            return app._host?.Services.GetRequiredService<T>() 
                ?? throw new InvalidOperationException($"Service {typeof(T).Name} not found");
        }
    }
}
