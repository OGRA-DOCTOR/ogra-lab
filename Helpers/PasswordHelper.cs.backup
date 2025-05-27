using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace OGRALAB.Helpers
{
    public static class PasswordHelper
    {
        private const int MinPasswordLength = 6;
        private const int MaxPasswordLength = 100;

        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public static string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            
            var result = new StringBuilder(length);
            foreach (byte b in bytes)
            {
                result.Append(validChars[b % validChars.Length]);
            }
            
            return result.ToString();
        }

        public static PasswordStrength CheckPasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return new PasswordStrength { Score = 0, Description = "كلمة المرور مطلوبة" };

            var score = 0;
            var feedback = new StringBuilder();

            // Length check
            if (password.Length < MinPasswordLength)
            {
                feedback.AppendLine($"كلمة المرور يجب أن تكون على الأقل {MinPasswordLength} أحرف");
            }
            else if (password.Length >= MinPasswordLength && password.Length < 8)
            {
                score += 1;
                feedback.AppendLine("طول كلمة المرور مقبول");
            }
            else if (password.Length >= 8 && password.Length < 12)
            {
                score += 2;
                feedback.AppendLine("طول كلمة المرور جيد");
            }
            else
            {
                score += 3;
                feedback.AppendLine("طول كلمة المرور ممتاز");
            }

            // Lowercase letters
            if (Regex.IsMatch(password, @"[a-z]"))
            {
                score += 1;
            }
            else
            {
                feedback.AppendLine("أضف حروف صغيرة");
            }

            // Uppercase letters
            if (Regex.IsMatch(password, @"[A-Z]"))
            {
                score += 1;
            }
            else
            {
                feedback.AppendLine("أضف حروف كبيرة");
            }

            // Numbers
            if (Regex.IsMatch(password, @"[0-9]"))
            {
                score += 1;
            }
            else
            {
                feedback.AppendLine("أضف أرقام");
            }

            // Special characters
            if (Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
            {
                score += 1;
            }
            else
            {
                feedback.AppendLine("أضف رموز خاصة");
            }

            // No repeated characters
            if (!HasRepeatedCharacters(password))
            {
                score += 1;
            }
            else
            {
                feedback.AppendLine("تجنب تكرار الأحرف");
            }

            // No common patterns
            if (!HasCommonPatterns(password))
            {
                score += 1;
            }
            else
            {
                feedback.AppendLine("تجنب الأنماط الشائعة");
            }

            var description = GetStrengthDescription(score);
            if (score >= 6)
            {
                feedback.Clear();
                feedback.Append("كلمة مرور قوية");
            }

            return new PasswordStrength
            {
                Score = score,
                MaxScore = 8,
                Description = description,
                Feedback = feedback.ToString().Trim()
            };
        }

        private static bool HasRepeatedCharacters(string password)
        {
            return password.GroupBy(c => c).Any(g => g.Count() > 2);
        }

        private static bool HasCommonPatterns(string password)
        {
            var commonPatterns = new[]
            {
                "123456", "654321", "abcdef", "fedcba", "qwerty", "asdfgh",
                "password", "admin", "user", "test", "123123", "abcabc"
            };

            return commonPatterns.Any(pattern => 
                password.ToLower().Contains(pattern.ToLower()));
        }

        private static string GetStrengthDescription(int score)
        {
            return score switch
            {
                0 or 1 or 2 => "ضعيف جداً",
                3 or 4 => "ضعيف",
                5 or 6 => "متوسط",
                7 => "قوي",
                8 => "قوي جداً",
                _ => "غير محدد"
            };
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < MinPasswordLength || password.Length > MaxPasswordLength)
                return false;

            var strength = CheckPasswordStrength(password);
            return strength.Score >= 4; // Minimum acceptable strength
        }
    }

    public class PasswordStrength
    {
        public int Score { get; set; }
        public int MaxScore { get; set; } = 8;
        public string Description { get; set; } = string.Empty;
        public string Feedback { get; set; } = string.Empty;
        public double Percentage => MaxScore > 0 ? (double)Score / MaxScore * 100 : 0;
    }
}
