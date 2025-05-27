using System;
using OGRALAB.Helpers;
using System.Text.RegularExpressions;

namespace OGRALAB.Helpers
{
    public static class ValidationHelper
    {
        // Email validation
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
                return emailRegex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        // Phone number validation (Saudi Arabia format)
        public static bool IsValidPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                return false;

            // Remove any spaces, dashes, or parentheses
            var cleanNumber = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            // Saudi phone number patterns
            var patterns = new[]
            {
                @"^05\d{Constants.MinPasswordLength}$",           // 05xxxxxxxx (Constants.MaxConcurrentOperations digits)
                @"^\+9665\d{Constants.MinPasswordLength}$",       // +9665xxxxxxxx
                @"^009665\d{Constants.MinPasswordLength}$",       // 009665xxxxxxxx
                @"^9665\d{Constants.MinPasswordLength}$"          // 9665xxxxxxxx
            };

            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(cleanNumber, pattern))
                    return true;
            }

            return false;
        }

        // National ID validation (Saudi Arabia format)
        public static bool IsValidNationalId(string nationalId)
        {
            if (string.IsNullOrWhiteSpace(nationalId))
                return false;

            // Remove any spaces or dashes
            var cleanId = nationalId.Replace(" ", "").Replace("-", "");

            // Must be exactly Constants.MaxConcurrentOperations digits
            if (cleanId.Length != Constants.MaxConcurrentOperations || !Regex.IsMatch(cleanId, @"^\d{Constants.MaxConcurrentOperations}$"))
                return false;

            // First digit should be 1 or 2 for Saudi nationals
            var firstDigit = int.Parse(cleanId[0].ToString());
            if (firstDigit != 1 && firstDigit != 2)
                return false;

            // Luhn algorithm check
            return IsValidLuhnNumber(cleanId);
        }

        // Age validation
        public static bool IsValidAge(DateTime dateOfBirth, int minAge = 0, int maxAge = 150)
        {
            var today = DateTime.Today;
            var age = today.Year - dateOfBirth.Year;
            
            if (dateOfBirth.Date > today.AddYears(-age))
                age--;

            return age >= minAge && age <= maxAge;
        }

        // Date validation
        public static bool IsValidDate(DateTime date, DateTime? minDate = null, DateTime? maxDate = null)
        {
            if (minDate.HasValue && date < minDate.Value)
                return false;

            if (maxDate.HasValue && date > maxDate.Value)
                return false;

            return true;
        }

        // Name validation (Arabic and English)
        public static bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            // Allow Arabic, English letters, and spaces
            var nameRegex = new Regex(@"^[\u0600-\u06FFa-zA-Z\s]+$");
            return nameRegex.IsMatch(name.Trim()) && name.Trim().Length >= 2;
        }

        // Username validation
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            // Username should be 3-Constants.DefaultPageSize characters, alphanumeric and underscore only
            var usernameRegex = new Regex(@"^[a-zA-Z0-9_]{3,Constants.DefaultPageSize}$");
            return usernameRegex.IsMatch(username);
        }

        // Luhn algorithm for National ID validation
        private static bool IsValidLuhnNumber(string number)
        {
            try
            {
                int sum = 0;
                bool alternate = false;

                for (int i = number.Length - 1; i >= 0; i--)
                {
                    int digit = int.Parse(number[i].ToString());

                    if (alternate)
                    {
                        digit *= 2;
                        if (digit > 9)
                            digit = (digit % Constants.MaxConcurrentOperations) + 1;
                    }

                    sum += digit;
                    alternate = !alternate;
                }

                return (sum % Constants.MaxConcurrentOperations) == 0;
            }
            catch
            {
                return false;
            }
        }

        // Blood type validation
        public static bool IsValidBloodType(string bloodType)
        {
            if (string.IsNullOrWhiteSpace(bloodType))
                return true; // Blood type is optional

            var validBloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };
            return Array.Exists(validBloodTypes, bt => bt.Equals(bloodType.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Decimal value validation
        public static bool IsValidDecimal(string value, decimal min = decimal.MinValue, decimal max = decimal.MaxValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (!decimal.TryParse(value, out decimal result))
                return false;

            return result >= min && result <= max;
        }

        // Integer value validation
        public static bool IsValidInteger(string value, int min = int.MinValue, int max = int.MaxValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (!int.TryParse(value, out int result))
                return false;

            return result >= min && result <= max;
        }

        // Required field validation
        public static bool IsRequired(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        // String length validation
        public static bool IsValidLength(string value, int minLength = 0, int maxLength = int.MaxValue)
        {
            if (value == null)
                return minLength == 0;

            return value.Length >= minLength && value.Length <= maxLength;
        }

        // Gender validation
        public static bool IsValidGender(string gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                return false;

            var validGenders = new[] { "Male", "Female", "ذكر", "أنثى" };
            return Array.Exists(validGenders, g => g.Equals(gender.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Test status validation
        public static bool IsValidTestStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
                return false;

            var validStatuses = new[] { "Ordered", "SampleCollected", "InProgress", "Completed", "Cancelled" };
            return Array.Exists(validStatuses, s => s.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // User role validation
        public static bool IsValidUserRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;

            var validRoles = new[] { "SystemUser", "AdminUser", "RegularUser" };
            return Array.Exists(validRoles, r => r.Equals(role.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
