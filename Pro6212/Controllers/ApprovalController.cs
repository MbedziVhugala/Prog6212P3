using Prog6212.Models;
using Prog6212.Services;
using Prog6212.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Prog6212.Controllers
{
    public class ApprovalController : Controller
    {
        private readonly IDataService _dataService;

        public ApprovalController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        public async Task<IActionResult> Pending()
        {
            if (!User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                return RedirectToAction("AccessDenied", "Account");

            var pendingClaims = await _dataService.GetPendingClaimsAsync();

            var viewModels = pendingClaims.Select(c => new ClaimViewModel
            {
                Id = c.Id,
                LecturerName = c.User?.FullName,
                HoursWorked = c.HoursWorked,
                HourlyRate = c.HourlyRate,
                AdditionalNotes = c.AdditionalNotes,
                SubmissionDate = c.SubmissionDate,
                SupportingDocumentPath = c.SupportingDocument
            }).ToList();

            return View(viewModels);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (!User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                return RedirectToAction("AccessDenied", "Account");

            var claim = await _dataService.GetClaimAsync(id);
            if (claim == null)
                return NotFound();

            var viewModel = new ClaimViewModel
            {
                Id = claim.Id,
                LecturerName = claim.User?.FullName,
                HoursWorked = claim.HoursWorked,
                HourlyRate = claim.HourlyRate,
                AdditionalNotes = claim.AdditionalNotes,
                SupportingDocumentPath = claim.SupportingDocument,
                SubmissionDate = claim.SubmissionDate
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            if (!User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                return RedirectToAction("AccessDenied", "Account");

            var claim = await _dataService.GetClaimAsync(id);
            if (claim == null)
                return NotFound();

            var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            claim.Status = "Approved";
            claim.ApprovalDate = System.DateTime.Now;
            claim.ApprovedBy = approverId;

            await _dataService.UpdateClaimAsync(claim);

            TempData["SuccessMessage"] = $"Claim #{claim.Id} approved successfully!";
            return RedirectToAction("Pending");
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            if (!User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                return RedirectToAction("AccessDenied", "Account");

            var claim = await _dataService.GetClaimAsync(id);
            if (claim == null)
                return NotFound();

            var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            claim.Status = "Rejected";
            claim.ApprovalDate = System.DateTime.Now;
            claim.ApprovedBy = approverId;
            claim.AdditionalNotes += $"\n\nRejection Reason: {rejectionReason}";

            await _dataService.UpdateClaimAsync(claim);

            TempData["SuccessMessage"] = $"Claim #{claim.Id} has been rejected.";
            return RedirectToAction("Pending");
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            if (!User.IsInRole("Coordinator") && !User.IsInRole("Manager"))
                return RedirectToAction("AccessDenied", "Account");

            var claims = await _dataService.GetClaimsAsync();
            var processedClaims = claims.Where(c => c.Status == "Approved" || c.Status == "Rejected").ToList();

            var history = processedClaims.Select(c => new ApprovalHistoryViewModel
            {
                ClaimId = c.Id,
                LecturerName = c.User?.FullName ?? "Unknown",
                Action = c.Status,
                ActionBy = c.Approver?.FullName ?? "Unknown",
                ActionDate = c.ApprovalDate ?? c.SubmissionDate
            }).ToList();

            return View(history);
        }
    }

    public class ApprovalHistoryViewModel
    {
        public int ClaimId { get; set; }
        public string LecturerName { get; set; }
        public string Action { get; set; }
        public string ActionBy { get; set; }
        public System.DateTime ActionDate { get; set; }
    }
}