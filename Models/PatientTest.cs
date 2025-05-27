using System;
using OGRALAB.Helpers;

namespace OGRALAB.Models
{
    public class PatientTest
    {
        [Key]
        public int PatientTestId { get; set; }

        [Required]
        public int PatientId { get; set; }

        [Required]
        public int TestTypeId { get; set; }

        [Required]
        [StringLength(20)]
        public string OrderNumber { get; set; } = string.Empty; // رقم الطلب الفريد

        public DateTime OrderDate { get; set; } = DateTime.Now; // تاريخ الطلب

        public DateTime? SampleCollectionDate { get; set; } // تاريخ أخذ العينة

        public DateTime? ResultDate { get; set; } // تاريخ ظهور النتيجة

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Ordered"; // Ordered, SampleCollected, InProgress, Completed, Cancelled

        [StringLength(Constants.DefaultPageSize)]
        public string? SampleType { get; set; } // نوع العينة (دم، بول، إلخ)

        [StringLength(Constants.CompletePercentage)]
        public string? OrderedBy { get; set; } // الطبيب أو الجهة الطالبة

        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; } // المبلغ المدفوع

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } // إجمالي المبلغ

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0; // نسبة الخصم

        [StringLength(20)]
        public string PaymentStatus { get; set; } = "Pending"; // Paid, Partial, Pending

        [StringLength(20)]
        public string Priority { get; set; } = "Normal"; // Urgent, High, Normal, Low

        public bool IsEmergency { get; set; } = false;

        [StringLength(Constants.MaxPageSize)]
        public string? Notes { get; set; }

        [StringLength(Constants.MaxPageSize)]
        public string? CancellationReason { get; set; }

        public DateTime? CancellationDate { get; set; }

        public int CreatedByUserId { get; set; } // المستخدم الذي أنشأ الطلب

        public int? ProcessedByUserId { get; set; } // المستخدم الذي عالج العينة

        public int? ApprovedByUserId { get; set; } // المستخدم الذي وافق على النتيجة

        // Navigation properties
        public virtual Patient Patient { get; set; } = null!;
        public virtual TestType TestType { get; set; } = null!;
        public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
