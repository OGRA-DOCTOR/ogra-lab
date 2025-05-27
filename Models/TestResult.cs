using System;
using OGRALAB.Helpers;

namespace OGRALAB.Models
{
    public class TestResult
    {
        [Key]
        public int TestResultId { get; set; }

        [Required]
        public int PatientTestId { get; set; }

        [Required]
        public int TestTypeId { get; set; }

        [StringLength(Constants.DefaultPageSize)]
        // TODO: This method is 62 lines long. Consider breaking it into smaller methods.
        public string? ParameterName { get; set; } // اسم المعامل (في حالة التحاليل المتعددة المعاملات)

        [StringLength(Constants.CompletePercentage)]
        public string? ResultValue { get; set; } // قيمة النتيجة

        public decimal? NumericValue { get; set; } // القيمة الرقمية للنتيجة

        [StringLength(Constants.DefaultPageSize)]
        public string? Unit { get; set; } // وحدة القياس

        [StringLength(Constants.MaxPatientNameLength)]
        public string? ReferenceRange { get; set; } // النطاق المرجعي

        [StringLength(20)]
        public string? Status { get; set; } // Normal, High, Low, Critical

        [StringLength(20)]
        public string? Flag { get; set; } // H (High), L (Low), C (Critical), * (Abnormal)

        [StringLength(Constants.MaxRecordsPerQuery)]
        public string? Comments { get; set; } // تعليقات على النتيجة

        [StringLength(Constants.MaxRecordsPerQuery)]
        public string? Interpretation { get; set; } // تفسير النتيجة

        public DateTime ResultDate { get; set; } = DateTime.Now;

        public DateTime? VerificationDate { get; set; } // تاريخ التحقق من النتيجة

        public int? VerifiedByUserId { get; set; } // المستخدم الذي تحقق من النتيجة

        public bool IsVerified { get; set; } = false;

        public bool IsCritical { get; set; } = false; // نتيجة حرجة تتطلب انتباه فوري

        public bool IsRepeated { get; set; } = false; // نتيجة معادة

        [StringLength(Constants.MaxPageSize)]
        public string? RepeatReason { get; set; } // سبب الإعادة

        public int? OriginalResultId { get; set; } // ربط بالنتيجة الأصلية في حالة الإعادة

        [StringLength(Constants.MaxPatientNameLength)]
        public string? MethodUsed { get; set; } // الطريقة المستخدمة في التحليل

        [StringLength(Constants.CompletePercentage)]
        public string? InstrumentUsed { get; set; } // الجهاز المستخدم

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public int CreatedByUserId { get; set; }

        public int? UpdatedByUserId { get; set; }

        // Navigation properties
        public virtual PatientTest PatientTest { get; set; } = null!;
        public virtual TestType TestType { get; set; } = null!;
        public virtual TestResult? OriginalResult { get; set; }
        public virtual ICollection<TestResult> RepeatedResults { get; set; } = new List<TestResult>();
    }
}
