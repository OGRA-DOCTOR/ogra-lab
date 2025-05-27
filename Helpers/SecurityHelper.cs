using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OGRALAB.Helpers
{
    /// <summary>
    /// Security helper class for input sanitization and security validations
    /// </summary>
    public static class SecurityHelper
    {
        /// <summary>
        /// Sanitizes input string to prevent XSS attacks
        /// </summary>
        /// <param name="input">Input string to sanitize</param>
        /// <returns>Sanitized string</returns>
        public static string SanitizeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            // Remove or encode potentially dangerous characters
            var sanitized = input
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&#x27;")
                .Replace("&", "&amp;")
                .Replace("/", "&#x2F;");

            // Remove script tags and javascript protocols
            sanitized = Regex.Replace(sanitized, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"javascript:", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"vbscript:", "", RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"on\w*\s*=", "", RegexOptions.IgnoreCase);

            return sanitized.Trim();
        }

        /// <summary>
        /// Validates and sanitizes SQL-related input to prevent SQL injection
        /// </summary>
        /// <param name="input">Input to validate</param>
        /// <returns>True if input is safe</returns>
        public static bool IsSqlSafe(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            // Check for common SQL injection patterns
            var sqlInjectionPatterns = new[]
            {
                @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE)\b)",
                @"(\b(UNION|JOIN)\b)",
                @"(\b(OR|AND)\s+\d+\s*=\s*\d+)",
                @"(\b(OR|AND)\s+['""][^'""]*['""])",
                @"(--|#|/\*|\*/)",
                @"(\bxp_\w+)",
                @"(\bsp_\w+)",
                @"(;|\b(SHUTDOWN|BACKUP)\b)"
            };

            foreach (var pattern in sqlInjectionPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if the input contains potentially malicious content
        /// </summary>
        /// <param name="input">Input to check</param>
        /// <returns>True if input appears safe</returns>
        public static bool IsSafeInput(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            // Check for suspicious patterns
            var maliciousPatterns = new[]
            {
                @"<script[^>]*>",
                @"javascript:",
                @"vbscript:",
                @"on\w*\s*=",
                @"expression\s*\(",
                @"url\s*\(",
                @"@import",
                @"<iframe[^>]*>",
                @"<object[^>]*>",
                @"<embed[^>]*>",
                @"<link[^>]*>",
                @"<meta[^>]*>"
            };

            foreach (var pattern in maliciousPatterns)
            {
                if (Regex.IsMatch(input, pattern, RegexOptions.IgnoreCase))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Validates password strength
        /// </summary>
        /// <param name="password">Password to validate</param>
        /// <param name="minLength">Minimum password length</param>
        /// <param name="requireComplexity">Whether to require complex password</param>
        /// <returns>True if password meets requirements</returns>
        public static bool IsPasswordSecure(string password, int minLength = 4, bool requireComplexity = false)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            if (password.Length < minLength)
                return false;

            if (!requireComplexity)
                return true;

            // Check for complexity requirements
            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        /// <summary>
        /// Removes dangerous characters from file names
        /// </summary>
        /// <param name="fileName">File name to sanitize</param>
        /// <returns>Safe file name</returns>
        public static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return string.Empty;

            // Remove invalid characters for file names
            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var safeName = new StringBuilder();

            foreach (char c in fileName)
            {
                if (!invalidChars.Contains(c) && c != '<' && c != '>')
                {
                    safeName.Append(c);
                }
            }

            return safeName.ToString().Trim();
        }

        /// <summary>
        /// Validates that a numeric string contains only numbers
        /// </summary>
        /// <param name="input">Input to validate</param>
        /// <returns>True if input contains only numbers</returns>
        public static bool IsNumericOnly(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return Regex.IsMatch(input, @"^\d+$");
        }

        /// <summary>
        /// Validates that a decimal string contains only valid decimal format
        /// </summary>
        /// <param name="input">Input to validate</param>
        /// <returns>True if input is valid decimal format</returns>
        public static bool IsValidDecimalFormat(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return Regex.IsMatch(input, @"^\d+(\.\d+)?$");
        }

        /// <summary>
        /// Checks if user input contains only allowed characters for names
        /// </summary>
        /// <param name="name">Name to validate</param>
        /// <returns>True if name contains only safe characters</returns>
        public static bool IsValidNameFormat(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            // Allow only Arabic letters, English letters, and spaces
            return Regex.IsMatch(name, @"^[\u0600-\u06FFa-zA-Z\s]+$");
        }

        /// <summary>
        /// Generates a secure random token
        /// </summary>
        /// <param name="length">Length of token to generate</param>
        /// <returns>Secure random token</returns>
        public static string GenerateSecureToken(int length = 32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Validates that input doesn't contain path traversal attempts
        /// </summary>
        /// <param name="input">Input to validate</param>
        /// <returns>True if input is safe from path traversal</returns>
        public static bool IsPathTraversalSafe(string input)
        {
            if (string.IsNullOrEmpty(input))
                return true;

            // Check for path traversal patterns
            var pathTraversalPatterns = new[]
            {
                @"\.\./",
                @"\.\.\\",
                @"%2e%2e%2f",
                @"%252e%252e%252f",
                @"..\/",
                @"..\\"
            };

            foreach (var pattern in pathTraversalPatterns)
            {
                if (input.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                    return false;
            }

            return true;
        }
    }
}
