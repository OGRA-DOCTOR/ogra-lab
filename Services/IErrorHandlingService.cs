using System;
using System.Threading.Tasks;
using System.Windows;

namespace OGRALAB.Services
{
    public interface IErrorHandlingService
    {
        // Error Display Methods
        void ShowError(string message, string title = "خطأ");
        void ShowError(Exception exception, string context = "", string title = "خطأ");
        void ShowWarning(string message, string title = "تنبيه");
        void ShowInfo(string message, string title = "معلومات");
        void ShowSuccess(string message, string title = "نجح");

        // Confirmation Methods
        bool ShowConfirmation(string message, string title = "تأكيد");
        bool ShowYesNoCancel(string message, string title = "اختيار", out bool cancelled);
        MessageBoxResult ShowMessageBox(string message, string title, MessageBoxButton buttons, MessageBoxImage icon);

        // Error Handling with Logging
        Task HandleErrorAsync(Exception exception, string context = "", string username = "");
        Task HandleWarningAsync(string message, string context = "", string username = "");
        Task HandleInfoAsync(string message, string context = "", string username = "");

        // User-Friendly Error Messages
        string GetUserFriendlyErrorMessage(Exception exception);
        string GetOperationFailedMessage(string operation);
        string GetValidationErrorMessage(string field, string issue);
        string GetAccessDeniedMessage(string resource);
        string GetNotFoundMessage(string item);
        string GetDuplicateErrorMessage(string item);

        // Progress and Loading
        void ShowLoadingMessage(string message = "جاري المعالجة...");
        void HideLoadingMessage();
        void UpdateProgress(string message, int percentage = -1);
    }
}
