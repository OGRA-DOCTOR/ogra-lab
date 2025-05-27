using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OGRALAB.Data;
using OGRALAB.Services;
using System;
using System.IO;
using System.Windows;

namespace OGRALAB
{
    public partial class App : Application
    {
        private IHost? _host;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء بدء التطبيق: {ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
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

            // Add ViewModels (can be added later as needed)
            // services.AddTransient<MainViewModel>();
            // services.AddTransient<LoginViewModel>();
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
