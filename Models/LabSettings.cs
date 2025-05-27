using System;
using OGRALAB.Helpers;

namespace OGRALAB.Models
{
    public class LabSettings
    {
        [Key]
        public int LabSettingsId { get; set; }

        [Required]
        [MaxLength(Constants.CompletePercentage)]
        public string LabName { get; set; } = "مختبر أوجرا للتحاليل الطبية";

        [MaxLength(Constants.MaxPatientNameLength)]
        public string LabNameEnglish { get; set; } = "OGRA Medical Laboratory";

        [MaxLength(Constants.MaxPageSize)]
        public string Address { get; set; } = "المنطقة الطبية، مدينة الرعاية الصحية";

        [MaxLength(Constants.MaxPageSize)]
        public string AddressEnglish { get; set; } = "Medical District, Healthcare City";

        [MaxLength(Constants.DefaultPageSize)]
        public string Phone { get; set; } = "+966 11 234 5678";

        [MaxLength(Constants.DefaultPageSize)]
        public string Mobile { get; set; } = "+966 Constants.DefaultPageSize 123 4567";

        [MaxLength(Constants.CompletePercentage)]
        public string Email { get; set; } = "info@ogralab.com";

        [MaxLength(Constants.CompletePercentage)]
        public string Website { get; set; } = "www.ogralab.com";

        [MaxLength(Constants.CompletePercentage)]
        public string LicenseNumber { get; set; } = "LAB-2024-001";

        [MaxLength(Constants.CompletePercentage)]
        public string DirectorName { get; set; } = "د. أحمد محمد الطبيب";

        [MaxLength(Constants.CompletePercentage)]
        public string DirectorTitle { get; set; } = "أ.د";

        [MaxLength(Constants.MaxPageSize)]
        public string LogoPath { get; set; } = "";

        public bool ShowLogoInReports { get; set; } = true;

        public bool ShowHeaderInReports { get; set; } = true;

        public bool ShowFooterInReports { get; set; } = true;

        [MaxLength(Constants.MaxRecordsPerQuery)]
        public string ReportFooterText { get; set; } = "جميع النتائج تم مراجعتها من قبل أخصائي المختبر المعتمد";

        [MaxLength(Constants.MaxRecordsPerQuery)]
        public string ReportFooterTextEnglish { get; set; } = "All results have been reviewed by certified laboratory specialist";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        [MaxLength(Constants.DefaultPageSize)]
        public string? ModifiedBy { get; set; }
    }
}
