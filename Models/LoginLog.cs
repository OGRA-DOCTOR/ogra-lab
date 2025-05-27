using System;
using OGRALAB.Helpers;

namespace OGRALAB.Models
{
    public class LoginLog
    {
        [Key]
        public int LoginLogId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(Constants.DefaultPageSize)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ActionType { get; set; } = string.Empty; // Login, Logout, Failed

        public DateTime ActionDate { get; set; } = DateTime.Now;

        [StringLength(45)]
        public string? IpAddress { get; set; } // عنوان IP

        [StringLength(Constants.MaxPageSize)]
        public string? UserAgent { get; set; } // معلومات المتصفح/التطبيق

        [StringLength(Constants.CompletePercentage)]
        public string? DeviceName { get; set; } // اسم الجهاز

        [StringLength(Constants.MaxPatientNameLength)]
        public string? Location { get; set; } // الموقع الجغرافي (اختياري)

        public bool IsSuccessful { get; set; } = true;

        [StringLength(Constants.MaxPageSize)]
        public string? FailureReason { get; set; } // سبب فشل تسجيل الدخول

        public DateTime? LogoutTime { get; set; } // وقت تسجيل الخروج

        public TimeSpan? SessionDuration { get; set; } // مدة الجلسة

        [StringLength(Constants.CompletePercentage)]
        public string? SessionId { get; set; } // معرف الجلسة

        [StringLength(Constants.MaxRecordsPerQuery)]
        public string? Notes { get; set; } // ملاحظات إضافية

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
