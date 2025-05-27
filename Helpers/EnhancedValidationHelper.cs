using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

namespace OGRALAB.Helpers
{
    /// <summary>
    /// Enhanced validation helper with comprehensive validation rules
    /// </summary>
    public static class EnhancedValidationHelper
    {
        #region Patient Validation
        
        /// <summary>
        /// Validate patient data comprehensively
        /// </summary>
        /// <param name="patient">Patient to validate</param>
        /// <returns>Validation results</returns>
        public static ValidationResult ValidatePatient(Models.Patient patient)
        {
            var errors = new List<string>();
            
            // Name validation
            if (string.IsNullOrWhiteSpace(patient.FullName))
                errors.Add("Patient name is required");
            else if (patient.FullName.Length > Constants.MaxPatientNameLength)
                errors.Add($"Patient name cannot exceed {Constants.MaxPatientNameLength} characters");
            else if (!IsValidName(patient.FullName))
                errors.Add("Patient name contains invalid characters");
                
            // Age validation
            if (patient.Age < Constants.MinPatientAge || patient.Age > Constants.MaxPatientAge)
                errors.Add($"Patient age must be between {Constants.MinPatientAge} and {Constants.MaxPatientAge}");
                
            // Phone validation
            if (!string.IsNullOrEmpty(patient.PhoneNumber) && !IsValidPhoneNumber(patient.PhoneNumber))
                errors.Add("Invalid phone number format");
                
            // Email validation
            if (!string.IsNullOrEmpty(patient.Email) && !IsValidEmail(patient.Email))
                errors.Add("Invalid email address format");
                
            // Medical Record Number validation
            if (!string.IsNullOrEmpty(patient.MedicalRecordNumber) && !IsValidMedicalRecordNumber(patient.MedicalRecordNumber))
                errors.Add("Invalid medical record number format");
                
            return new ValidationResult(errors);
        }
        
        #endregion
        
        #region Test Validation
        
        /// <summary>
        /// Validate test type data
        /// </summary>
        /// <param name="testType">Test type to validate</param>
        /// <returns>Validation results</returns>
        public static ValidationResult ValidateTestType(Models.TestType testType)
        {
            var errors = new List<string>();
            
            // Test name validation
            if (string.IsNullOrWhiteSpace(testType.TestName))
                errors.Add("Test name is required");
            else if (testType.TestName.Length > Constants.MaxTestNameLength)
                errors.Add($"Test name cannot exceed {Constants.MaxTestNameLength} characters");
                
            // Test code validation
            if (string.IsNullOrWhiteSpace(testType.TestCode))
                errors.Add("Test code is required");
            else if (!IsValidTestCode(testType.TestCode))
                errors.Add("Test code must be alphanumeric and 3-10 characters long");
                
            // Unit validation
            if (string.IsNullOrWhiteSpace(testType.Unit))
                errors.Add("Test unit is required");
                
            // Range validation
            if (testType.NormalRangeMin >= testType.NormalRangeMax)
                errors.Add("Normal range minimum must be less than maximum");
                
            return new ValidationResult(errors);
        }
        
        /// <summary>
        /// Validate test result data
        /// </summary>
        /// <param name="testResult">Test result to validate</param>
        /// <returns>Validation results</returns>
        public static ValidationResult ValidateTestResult(Models.TestResult testResult)
        {
            var errors = new List<string>();
            
            // Result value validation
            if (string.IsNullOrWhiteSpace(testResult.ResultValue))
                errors.Add("Test result value is required");
            else if (!IsValidResultValue(testResult.ResultValue))
                errors.Add("Invalid result value format");
                
            // Date validation
            if (testResult.ResultDate > DateTime.Now)
                errors.Add("Result date cannot be in the future");
                
            if (testResult.ResultDate < DateTime.Now.AddYears(-10))
                errors.Add("Result date seems too old (more than 10 years ago)");
                
            return new ValidationResult(errors);
        }
        
        #endregion
        
        #region User Validation
        
        /// <summary>
        /// Validate user data
        /// </summary>
        /// <param name="user">User to validate</param>
        /// <returns>Validation results</returns>
        public static ValidationResult ValidateUser(Models.User user)
        {
            var errors = new List<string>();
            
            // Username validation
            if (string.IsNullOrWhiteSpace(user.Username))
                errors.Add("Username is required");
            else if (!IsValidUsername(user.Username))
                errors.Add("Username must be 3-50 characters, alphanumeric with dots and underscores allowed");
                
            // Full name validation
            if (string.IsNullOrWhiteSpace(user.FullName))
                errors.Add("Full name is required");
            else if (!IsValidName(user.FullName))
                errors.Add("Full name contains invalid characters");
                
            // Email validation
            if (!string.IsNullOrEmpty(user.Email) && !IsValidEmail(user.Email))
                errors.Add("Invalid email address format");
                
            // Role validation
            if (string.IsNullOrWhiteSpace(user.Role))
                errors.Add("User role is required");
            else if (!IsValidRole(user.Role))
                errors.Add("Invalid user role");
                
            return new ValidationResult(errors);
        }
        
        /// <summary>
        /// Validate password strength
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <returns>Validation results</returns>
        public static ValidationResult ValidatePassword(string password)
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(password))
            {
                errors.Add("Password is required");
                return new ValidationResult(errors);
            }
            
            if (password.Length < Constants.MinPasswordLength)
                errors.Add($"Password must be at least {Constants.MinPasswordLength} characters long");
                
            if (password.Length > Constants.MaxPasswordLength)
                errors.Add($"Password cannot exceed {Constants.MaxPasswordLength} characters");
                
            if (!HasLowerCase(password))
                errors.Add("Password must contain at least one lowercase letter");
                
            if (!HasUpperCase(password))
                errors.Add("Password must contain at least one uppercase letter");
                
            if (!HasDigit(password))
                errors.Add("Password must contain at least one digit");
                
            if (!HasSpecialCharacter(password))
                errors.Add("Password must contain at least one special character (!@#$%^&*)");
                
            if (IsCommonPassword(password))
                errors.Add("Password is too common, please choose a stronger password");
                
            return new ValidationResult(errors);
        }
        
        #endregion
        
        #region Format Validation Methods
        
        /// <summary>
        /// Validate name format (letters, spaces, hyphens, apostrophes)
        /// </summary>
        private static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return Regex.IsMatch(name, @"^[a-zA-Z\u0600-\u06FF\s\-'\.]+$"); // Supports Arabic characters
        }
        
        /// <summary>
        /// Validate username format
        /// </summary>
        private static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return Regex.IsMatch(username, @"^[a-zA-Z0-9._]{3,50}$");
        }
        
        /// <summary>
        /// Validate email format
        /// </summary>
        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Validate phone number format (supports international formats)
        /// </summary>
        private static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return false;
            
            // Remove common separators
            var cleaned = Regex.Replace(phoneNumber, @"[\s\-\(\)\+]", "");
            
            // Check if it's all digits and reasonable length
            return Regex.IsMatch(cleaned, @"^\d{7,15}$");
        }
        
        /// <summary>
        /// Validate medical record number format
        /// </summary>
        private static bool IsValidMedicalRecordNumber(string mrn)
        {
            if (string.IsNullOrWhiteSpace(mrn)) return false;
            
            // Medical record numbers are typically alphanumeric, 5-20 characters
            return Regex.IsMatch(mrn, @"^[A-Z0-9]{5,20}$");
        }
        
        /// <summary>
        /// Validate test code format
        /// </summary>
        private static bool IsValidTestCode(string testCode)
        {
            if (string.IsNullOrWhiteSpace(testCode)) return false;
            
            // Test codes are typically uppercase alphanumeric, 3-10 characters
            return Regex.IsMatch(testCode, @"^[A-Z0-9]{3,10}$");
        }
        
        /// <summary>
        /// Validate test result value format
        /// </summary>
        private static bool IsValidResultValue(string resultValue)
        {
            if (string.IsNullOrWhiteSpace(resultValue)) return false;
            
            // Result can be numeric or text (like "Positive", "Negative")
            return resultValue.Length <= 100 && !string.IsNullOrWhiteSpace(resultValue);
        }
        
        /// <summary>
        /// Validate user role
        /// </summary>
        private static bool IsValidRole(string role)
        {
            var validRoles = new[] { "Admin", "Doctor", "Technician", "Receptionist", "Manager" };
            return validRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
        }
        
        #endregion
        
        #region Password Strength Methods
        
        private static bool HasLowerCase(string password) => password.Any(char.IsLower);
        private static bool HasUpperCase(string password) => password.Any(char.IsUpper);
        private static bool HasDigit(string password) => password.Any(char.IsDigit);
        private static bool HasSpecialCharacter(string password) => password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));
        
        private static bool IsCommonPassword(string password)
        {
            var commonPasswords = new[]
            {
                "password", "123456", "password123", "admin", "qwerty",
                "letmein", "welcome", "monkey", "1234567890", "password1"
            };
            
            return commonPasswords.Contains(password.ToLower());
        }
        
        #endregion
        
        #region Validation Result Class
        
        /// <summary>
        /// Represents validation results
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid => !Errors.Any();
            public List<string> Errors { get; }
            public string ErrorMessage => string.Join("; ", Errors);
            
            public ValidationResult() : this(new List<string>()) { }
            
            public ValidationResult(List<string> errors)
            {
                Errors = errors ?? new List<string>();
            }
            
            public ValidationResult(string error) : this(new List<string> { error }) { }
            
            public void AddError(string error)
            {
                if (!string.IsNullOrWhiteSpace(error))
                    Errors.Add(error);
            }
            
            public void AddErrors(IEnumerable<string> errors)
            {
                if (errors != null)
                    Errors.AddRange(errors.Where(e => !string.IsNullOrWhiteSpace(e)));
            }
        }
        
        #endregion
    }
}
