using System;
using System.ComponentModel.DataAnnotations;

namespace OGRALAB.Models
{
    public class LabSettings
    {
        [Key]
        public int LabSettingsId { get; set; }

        [Required]
        [MaxLength(100)]
        public string LabName { get; set; } = "مختبر أوجرا للتحاليل الطبية";

        [MaxLength(200)]
        public string LabNameEnglish { get; set; } = "OGRA Medical Laboratory";

        [MaxLength(500)]
        public string Address { get; set; } = "المنطقة الطبية، مدينة الرعاية الصحية";

        [MaxLength(500)]
        public string AddressEnglish { get; set; } = "Medical District, Healthcare City";

        [MaxLength(50)]
        public string Phone { get; set; } = "+966 11 234 5678";

        [MaxLength(50)]
        public string Mobile { get; set; } = "+966 50 123 4567";

        [MaxLength(100)]
        public string Email { get; set; } = "info@ogralab.com";

        [MaxLength(100)]
        public string Website { get; set; } = "www.ogralab.com";

        [MaxLength(100)]
        public string LicenseNumber { get; set; } = "LAB-2024-001";

        [MaxLength(100)]
        public string DirectorName { get; set; } = "د. أحمد محمد الطبيب";

        [MaxLength(100)]
        public string DirectorTitle { get; set; } = "أ.د";

        [MaxLength(500)]
        public string LogoPath { get; set; } = "";

        public bool ShowLogoInReports { get; set; } = true;

        public bool ShowHeaderInReports { get; set; } = true;

        public bool ShowFooterInReports { get; set; } = true;

        [MaxLength(1000)]
        public string ReportFooterText { get; set; } = "جميع النتائج تم مراجعتها من قبل أخصائي المختبر المعتمد";

        [MaxLength(1000)]
        public string ReportFooterTextEnglish { get; set; } = "All results have been reviewed by certified laboratory specialist";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }
    }
}
