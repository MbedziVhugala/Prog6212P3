using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Prog6212.ViewModels
{
    public class ClaimViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(0, 1000, ErrorMessage = "Hourly rate must be between 0 and 1000")]
        [Display(Name = "Hourly Rate (R)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Additional Notes")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string AdditionalNotes { get; set; }

        [Display(Name = "Supporting Document")]
        public IFormFile SupportingDocument { get; set; }

        public string SupportingDocumentPath { get; set; }
        public string LecturerName { get; set; }
        public string Status { get; set; }
        public DateTime SubmissionDate { get; set; }

        public DateTime? ApprovalDate { get; set; }
        public string ApprovedByName { get; set; }

        [Display(Name = "Total Amount (R)")]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        // For monthly validation message
        public string MonthlyLimitMessage { get; set; }
    }
}