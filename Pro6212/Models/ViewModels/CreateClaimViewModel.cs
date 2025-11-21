using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Prog6212.ViewModels
{
    public class CreateClaimViewModel
    {
        [Required(ErrorMessage = "Hours worked is required")]
        [Range(0.1, 180, ErrorMessage = "Hours worked must be between 0.1 and 180")]
        [Display(Name = "Hours Worked")]
        public decimal HoursWorked { get; set; }

        [Display(Name = "Additional Notes")]
        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string AdditionalNotes { get; set; }

        [Display(Name = "Supporting Document")]
        public IFormFile SupportingDocument { get; set; }
    }
}