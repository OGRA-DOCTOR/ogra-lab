using System;
using OGRALAB.Helpers;

namespace OGRALAB.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [StringLength(Constants.DefaultPageSize)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(Constants.CompletePercentage)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(Constants.CompletePercentage)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = string.Empty; // SystemUser, AdminUser, RegularUser

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastLoginDate { get; set; }

        [StringLength(Constants.CacheDurationMinutes)]
        public string? PhoneNumber { get; set; }

        // Navigation properties
        public virtual ICollection<LoginLog> LoginLogs { get; set; } = new List<LoginLog>();
    }
}
