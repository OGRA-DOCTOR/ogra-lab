using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OGRALAB.Models
{
    public class TestType
    {
        [Key]
        public int TestTypeId { get; set; }

        [Required]
        [StringLength(20)]
        public string TestCode { get; set; } = string.Empty; // كود التحليل الفريد

        [Required]
        [StringLength(100)]
        public string TestName { get; set; } = string.Empty; // اسم التحليل

        [StringLength(200)]
        public string? Description { get; set; } // وصف التحليل

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // فئة التحليل (مثل: كيمياء، هرمونات، إلخ)

        [StringLength(50)]
        public string? Unit { get; set; } // وحدة القياس

        public decimal? MinNormalValue { get; set; } // الحد الأدنى للقيمة الطبيعية

        public decimal? MaxNormalValue { get; set; } // الحد الأقصى للقيمة الطبيعية

        [StringLength(200)]
        public string? NormalRange { get; set; } // النطاق الطبيعي (نص)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // سعر التحليل

        public int? PreparationTime { get; set; } // وقت التحضير بالدقائق

        public int? ResultTime { get; set; } // وقت ظهور النتيجة بالساعات

        [StringLength(500)]
        public string? PreparationInstructions { get; set; } // تعليمات التحضير

        public bool IsActive { get; set; } = true;

        public bool RequiresFasting { get; set; } = false; // يتطلب صيام

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public int? TestGroupId { get; set; } // ربط بمجموعة التحاليل

        // Navigation properties
        public virtual TestGroup? TestGroup { get; set; }
        public virtual ICollection<PatientTest> PatientTests { get; set; } = new List<PatientTest>();
        public virtual ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    }
}
