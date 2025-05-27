using System;
using System.ComponentModel.DataAnnotations;

namespace OGRALAB.Models
{
    public class LoginLog
    {
        [Key]
        public int LoginLogId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string ActionType { get; set; } = string.Empty; // Login, Logout, Failed

        public DateTime ActionDate { get; set; } = DateTime.Now;

        [StringLength(45)]
        public string? IpAddress { get; set; } // عنوان IP

        [StringLength(500)]
        public string? UserAgent { get; set; } // معلومات المتصفح/التطبيق

        [StringLength(100)]
        public string? DeviceName { get; set; } // اسم الجهاز

        [StringLength(200)]
        public string? Location { get; set; } // الموقع الجغرافي (اختياري)

        public bool IsSuccessful { get; set; } = true;

        [StringLength(500)]
        public string? FailureReason { get; set; } // سبب فشل تسجيل الدخول

        public DateTime? LogoutTime { get; set; } // وقت تسجيل الخروج

        public TimeSpan? SessionDuration { get; set; } // مدة الجلسة

        [StringLength(100)]
        public string? SessionId { get; set; } // معرف الجلسة

        [StringLength(1000)]
        public string? Notes { get; set; } // ملاحظات إضافية

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}
