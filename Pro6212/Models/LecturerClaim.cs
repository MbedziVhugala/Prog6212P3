using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Prog6212.Models
{
    public class LecturerClaim
    {


        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Add this line
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 200, ErrorMessage = "Hours worked must be between 1 and 200")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Hourly rate must be between 0 and 1000")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        [NotMapped]
        public decimal TotalAmount => HoursWorked * HourlyRate;

        [StringLength(1000)]
        [Display(Name = "Additional Notes")]
        public string AdditionalNotes { get; set; }

        public string SupportingDocument { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime SubmissionDate { get; set; } = DateTime.Now;
        public DateTime? ApprovalDate { get; set; }
        public int? ApprovedBy { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("ApprovedBy")]
        public User Approver { get; set; }
    }
}