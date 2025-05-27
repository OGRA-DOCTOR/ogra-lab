using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace OGRALAB.Services
{
    public class ErrorHandlingService : IErrorHandlingService
    {
        private readonly ILoggingService _loggingService;
        private Window? _loadingWindow;

        public ErrorHandlingService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        #region Error Display Methods

        public void ShowError(string message, string title = "خطأ")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }

        public void ShowError(Exception exception, string context = "", string title = "خطأ")
        {
            var userMessage = GetUserFriendlyErrorMessage(exception);
            if (!string.IsNullOrEmpty(context))
            {
                userMessage = $"{context}\n\nالتفاصيل: {userMessage}";
            }

            ShowError(userMessage, title);
        }

        public void ShowWarning(string message, string title = "تنبيه")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }

        public void ShowInfo(string message, string title = "معلومات")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        public void ShowSuccess(string message, string title = "نجح")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }

        #endregion

        #region Confirmation Methods

        public bool ShowConfirmation(string message, string title = "تأكيد")
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result == MessageBoxResult.Yes;
            });
        }

        public bool ShowYesNoCancel(string message, string title = "اختيار", out bool cancelled)
        {
            var result = Application.Current.Dispatcher.Invoke(() =>
            {
                return MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            });

            cancelled = result == MessageBoxResult.Cancel;
            return result == MessageBoxResult.Yes;
        }

        public MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                return MessageBox.Show(message, title, buttons, icon);
            });
        }

        #endregion

        #region Error Handling with Logging

        public async Task HandleErrorAsync(Exception exception, string context = "", string username = "")
        {
            try
            {
                // Log the error
                await _loggingService.LogErrorAsync($"خطأ في السياق: {context}", exception, "Error", username);

                // Show user-friendly message
                ShowError(exception, context);
            }
            catch
            {
                // Fallback error display
                ShowError("حدث خطأ غير متوقع في النظام", "خطأ حرج");
            }
        }

        public async Task HandleWarningAsync(string message, string context = "", string username = "")
        {
            try
            {
                var fullMessage = string.IsNullOrEmpty(context) ? message : $"{context}: {message}";
                await _loggingService.LogWarningAsync(fullMessage, "Warning", username);
                ShowWarning(message);
            }
            catch
            {
                ShowWarning(message);
            }
        }

        public async Task HandleInfoAsync(string message, string context = "", string username = "")
        {
            try
            {
                var fullMessage = string.IsNullOrEmpty(context) ? message : $"{context}: {message}";
                await _loggingService.LogInfoAsync(fullMessage, "Info", username);
                ShowInfo(message);
            }
            catch
            {
                ShowInfo(message);
            }
        }

        #endregion

        #region User-Friendly Error Messages

        public string GetUserFriendlyErrorMessage(Exception exception)
        {
            return exception switch
            {
                SqliteException sqliteEx => GetSqliteErrorMessage(sqliteEx),
                DbUpdateException dbEx => GetDatabaseUpdateErrorMessage(dbEx),
                InvalidOperationException invalidOpEx => GetInvalidOperationErrorMessage(invalidOpEx),
                ArgumentException argEx => GetArgumentErrorMessage(argEx),
                UnauthorizedAccessException => "ليس لديك صلاحية للوصول إلى هذه الوظيفة",
                FileNotFoundException => "الملف المطلوب غير موجود",
                DirectoryNotFoundException => "المجلد المطلوب غير موجود",
                TimeoutException => "انتهت مهلة العملية، يرجى المحاولة مرة أخرى",
                OutOfMemoryException => "لا توجد ذاكرة كافية لإتمام العملية",
                _ => GetGenericErrorMessage(exception)
            };
        }

        private string GetSqliteErrorMessage(SqliteException exception)
        {
            return exception.SqliteErrorCode switch
            {
                19 => "البيانات المدخلة تتعارض مع القيود الموجودة (مثل الرقم المكرر)",
                1 => "خطأ في صيغة SQL أو قاعدة البيانات",
                14 => "قاعدة البيانات مقفلة، يرجى المحاولة لاحقاً",
                11 => "قاعدة البيانات تالفة أو غير صالحة",
                _ => "خطأ في قاعدة البيانات. يرجى المحاولة مرة أخرى أو الاتصال بالدعم الفني"
            };
        }

        private string GetDatabaseUpdateErrorMessage(DbUpdateException exception)
        {
            if (exception.InnerException is SqliteException sqliteEx)
            {
                return GetSqliteErrorMessage(sqliteEx);
            }

            return "فشل في حفظ البيانات. يرجى التحقق من صحة البيانات والمحاولة مرة أخرى";
        }

        private string GetInvalidOperationErrorMessage(InvalidOperationException exception)
        {
            if (exception.Message.Contains("connection"))
            {
                return "خطأ في الاتصال بقاعدة البيانات";
            }

            if (exception.Message.Contains("sequence"))
            {
                return "لا توجد بيانات متاحة للعملية المطلوبة";
            }

            return "العملية غير صالحة في الوضع الحالي";
        }

        private string GetArgumentErrorMessage(ArgumentException exception)
        {
            return "البيانات المدخلة غير صحيحة أو مفقودة";
        }

        private string GetGenericErrorMessage(Exception exception)
        {
            // Don't expose internal exception details to users for security reasons
            // Log the actual error for debugging but show generic message to user
            return "حدث خطأ غير متوقع. يرجى المحاولة مرة أخرى أو الاتصال بالدعم الفني";
        }

        public string GetOperationFailedMessage(string operation)
        {
            return $"فشل في تنفيذ العملية: {operation}. يرجى المحاولة مرة أخرى.";
        }

        public string GetValidationErrorMessage(string field, string issue)
        {
            return $"خطأ في الحقل '{field}': {issue}";
        }

        public string GetAccessDeniedMessage(string resource)
        {
            return $"ليس لديك صلاحية للوصول إلى: {resource}";
        }

        public string GetNotFoundMessage(string item)
        {
            return $"العنصر المطلوب غير موجود: {item}";
        }

        public string GetDuplicateErrorMessage(string item)
        {
            return $"العنصر موجود مسبقاً: {item}";
        }

        #endregion

        #region Progress and Loading

        public void ShowLoadingMessage(string message = "جاري المعالجة...")
        {
            // This is a simplified implementation
            // In a full application, you would show a proper loading dialog
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Update cursor to waiting
                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            });
        }

        public void HideLoadingMessage()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Reset cursor
                Mouse.OverrideCursor = null;
            });
        }

        public void UpdateProgress(string message, int percentage = -1)
        {
            // This would update a progress dialog if implemented
            // For now, just log the progress
            _ = _loggingService.LogInfoAsync($"تقدم العملية: {message}" + 
                (percentage >= 0 ? $" ({percentage}%)" : ""), "Progress");
        }

        #endregion
    }
}
