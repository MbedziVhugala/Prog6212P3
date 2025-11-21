using System;

namespace Prog6212.ViewModels
{
    public class ClaimSummaryViewModel
    {
        public int Id { get; set; }
        public string LecturerName { get; set; }
        public decimal HoursWorked { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime SubmissionDate { get; set; }
        public bool HasDocument { get; set; }
        public string SupportingDocument { get; set; }

        public string SupportingDocumentPath { get; set; }
    }
}