using System;
using System.Configuration;

namespace OGRALAB.Settings
{
    public static class SecuritySettings
    {
        /// <summary>
        /// Gets the default password for password reset functionality
        /// This should be configurable via app settings or environment variables
        /// </summary>
        public static string DefaultPassword => 
            ConfigurationManager.AppSettings["DefaultPassword"] ?? 
            Environment.GetEnvironmentVariable("OGRA_LAB_DEFAULT_PASSWORD") ?? 
            "TempPassword123!"; // More secure fallback than hardcoded "0000"
        
        /// <summary>
        /// Minimum password length requirement
        /// </summary>
        public static int MinPasswordLength => 4;
        
        /// <summary>
        /// Maximum number of failed login attempts before account lockout
        /// </summary>
        public static int MaxFailedLoginAttempts => 5;
        
        /// <summary>
        /// Account lockout duration in minutes
        /// </summary>
        public static int AccountLockoutDurationMinutes => 30;
        
        /// <summary>
        /// Password complexity requirements
        /// </summary>
        public static bool RequireComplexPassword => false; // Can be enabled later
    }
}
