using OGRALAB.Models;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public interface IAuthenticationService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<bool> LogoutAsync(int userId);
        Task<bool> LogLoginAttemptAsync(int userId, string username, bool isSuccessful, string? failureReason = null, string? ipAddress = null);
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }
        void SetCurrentUser(User user);
        void ClearCurrentUser();
    }
}
