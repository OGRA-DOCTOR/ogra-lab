using System;
using OGRALAB.Helpers;

namespace OGRALAB.Models
{
    public class TestGroup
    {
        [Key]
        public int TestGroupId { get; set; }

        [Required]
        [StringLength(20)]
        public string GroupCode { get; set; } = string.Empty; // كود المجموعة الفريد

        [Required]
        [StringLength(Constants.CompletePercentage)]
        public string GroupName { get; set; } = string.Empty; // اسم المجموعة

        [StringLength(Constants.MaxPatientNameLength)]
        public string? Description { get; set; } // وصف المجموعة

        [Column(TypeName = "decimal(18,2)")]
        public decimal GroupPrice { get; set; } // سعر المجموعة (قد يكون أقل من مجموع الأسعار الفردية)

        [Column(TypeName = "decimal(5,2)")]
        public decimal DiscountPercentage { get; set; } = 0; // نسبة الخصم على المجموعة

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        [StringLength(Constants.MaxPageSize)]
        public string? Notes { get; set; }

        // Navigation properties
        public virtual ICollection<TestType> TestTypes { get; set; } = new List<TestType>();
    }
}
