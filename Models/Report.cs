using System.ComponentModel.DataAnnotations;

namespace PROG6212_POE.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        public string ReportType { get; set; } = string.Empty; // "Monthly", "Lecturer", "Department"

        [Required]
        public string Period { get; set; } = string.Empty; // "August 2025"

        [DataType(DataType.DateTime)]
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        public int GeneratedByUserId { get; set; }

        // Report data (could be JSON or file path in production)
        public string ReportData { get; set; } = string.Empty;

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Total Claims")]
        public int TotalClaims { get; set; }

        [Display(Name = "File Path")]
        public string? FilePath { get; set; }
    }
}