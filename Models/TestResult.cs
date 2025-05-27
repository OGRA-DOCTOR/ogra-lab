using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [StringLength(50)]
        public string? ParameterName { get; set; } // اسم المعامل (في حالة التحاليل المتعددة المعاملات)

        [StringLength(100)]
        public string? ResultValue { get; set; } // قيمة النتيجة

        public decimal? NumericValue { get; set; } // القيمة الرقمية للنتيجة

        [StringLength(50)]
        public string? Unit { get; set; } // وحدة القياس

        [StringLength(200)]
        public string? ReferenceRange { get; set; } // النطاق المرجعي

        [StringLength(20)]
        public string? Status { get; set; } // Normal, High, Low, Critical

        [StringLength(20)]
        public string? Flag { get; set; } // H (High), L (Low), C (Critical), * (Abnormal)

        [StringLength(1000)]
        public string? Comments { get; set; } // تعليقات على النتيجة

        [StringLength(1000)]
        public string? Interpretation { get; set; } // تفسير النتيجة

        public DateTime ResultDate { get; set; } = DateTime.Now;

        public DateTime? VerificationDate { get; set; } // تاريخ التحقق من النتيجة

        public int? VerifiedByUserId { get; set; } // المستخدم الذي تحقق من النتيجة

        public bool IsVerified { get; set; } = false;

        public bool IsCritical { get; set; } = false; // نتيجة حرجة تتطلب انتباه فوري

        public bool IsRepeated { get; set; } = false; // نتيجة معادة

        [StringLength(500)]
        public string? RepeatReason { get; set; } // سبب الإعادة

        public int? OriginalResultId { get; set; } // ربط بالنتيجة الأصلية في حالة الإعادة

        [StringLength(200)]
        public string? MethodUsed { get; set; } // الطريقة المستخدمة في التحليل

        [StringLength(100)]
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
