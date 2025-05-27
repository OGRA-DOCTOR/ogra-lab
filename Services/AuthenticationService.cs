using OGRALAB.Data;
using OGRALAB.Models;
using System;
using System.Threading.Tasks;

namespace OGRALAB.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly OgraLabDbContext _context;
        private User? _currentUser;

        public AuthenticationService(OgraLabDbContext context)
        {
            _context = context;
        }

        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            try
            {
                var userService = new UserService(_context);
                var user = await userService.GetUserByUsernameAsync(username);

                if (user == null || !user.IsActive)
                {
                    await LogLoginAttemptAsync(0, username, false, "مستخدم غير موجود أو غير نشط");
                    return null;
                }

                var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                if (!isPasswordValid)
                {
                    await LogLoginAttemptAsync(user.UserId, username, false, "كلمة مرور خاطئة");
                    return null;
                }

                // Update last login date
                user.LastLoginDate = DateTime.Now;
                await userService.UpdateUserAsync(user);

                // Log successful login
                await LogLoginAttemptAsync(user.UserId, username, true);

                _currentUser = user;
                return user;
            }
            catch (Exception ex)
            {
                await LogLoginAttemptAsync(0, username, false, $"خطأ في النظام: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                if (_currentUser != null && _currentUser.UserId == userId)
                {
                    // Log logout
                    var loginLog = new LoginLog
                    {
                        UserId = userId,
                        Username = _currentUser.Username,
                        ActionType = "Logout",
                        ActionDate = DateTime.Now,
                        LogoutTime = DateTime.Now,
                        IsSuccessful = true
                    };

                    _context.LoginLogs.Add(loginLog);
                    await _context.SaveChangesAsync();

                    _currentUser = null;
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LogLoginAttemptAsync(int userId, string username, bool isSuccessful, string? failureReason = null, string? ipAddress = null)
        {
            try
            {
                var loginLog = new LoginLog
                {
                    UserId = userId,
                    Username = username,
                    ActionType = isSuccessful ? "Login" : "Failed",
                    ActionDate = DateTime.Now,
                    IpAddress = ipAddress,
                    IsSuccessful = isSuccessful,
                    FailureReason = failureReason
                };

                _context.LoginLogs.Add(loginLog);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
        }

        public void ClearCurrentUser()
        {
            _currentUser = null;
        }
    }
}
