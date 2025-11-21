using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Prog6212.Models;
using Prog6212.Services;
using Prog6212.ViewModels;

namespace Prog6212.Controllers
{
    [Authorize(Roles = "Coordinator,Manager")]
    public class CoordinatorController : Controller
    {
        private readonly IDataService _dataService;

        public CoordinatorController(IDataService dataService)
        {
            _dataService = dataService;
        }

        // GET: /Coordinator/PendingClaims
        // GET: /Coordinator/PendingClaims
        public async Task<IActionResult> PendingClaims()
        {
            try
            {
                var pendingClaims = await _dataService.GetPendingClaimsAsync();

                var viewModel = pendingClaims.Select(c => new ClaimViewModel
                {
                    Id = c.Id,
                    LecturerName = c.User?.FullName ?? "Unknown",
                    HoursWorked = c.HoursWorked,
                    HourlyRate = c.HourlyRate,
                    // TotalAmount is computed automatically - don't set it
                    AdditionalNotes = c.AdditionalNotes,
                    SupportingDocumentPath = c.SupportingDocument,
                    Status = c.Status,
                    SubmissionDate = c.SubmissionDate
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading pending claims: " + ex.Message;
                return View(new List<ClaimViewModel>());
            }
        }

        // GET: /Coordinator/ReviewClaim/{id}
        public async Task<IActionResult> ReviewClaim(int id)
        {
            try
            {
                var claim = await _dataService.GetClaimAsync(id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction("PendingClaims");
                }

                var viewModel = new ClaimViewModel
                {
                    Id = claim.Id,
                    LecturerName = claim.User?.FullName ?? "Unknown",
                    HoursWorked = claim.HoursWorked,
                    HourlyRate = claim.HourlyRate,
                    // TotalAmount is computed automatically - don't set it
                    AdditionalNotes = claim.AdditionalNotes,
                    SupportingDocumentPath = claim.SupportingDocument,
                    Status = claim.Status,
                    SubmissionDate = claim.SubmissionDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading claim: " + ex.Message;
                return RedirectToAction("PendingClaims");
            }
        }

        // GET: /Coordinator/ApprovedClaims
        public async Task<IActionResult> ApprovedClaims()
        {
            try
            {
                var allClaims = await _dataService.GetClaimsAsync();
                var approvedClaims = allClaims.Where(c => c.Status == "Approved").ToList();

                var viewModel = approvedClaims.Select(c => new ClaimViewModel
                {
                    Id = c.Id,
                    LecturerName = c.User?.FullName ?? "Unknown",
                    HoursWorked = c.HoursWorked,
                    HourlyRate = c.HourlyRate,
                    // TotalAmount is computed automatically - don't set it
                    AdditionalNotes = c.AdditionalNotes,
                    SupportingDocumentPath = c.SupportingDocument,
                    Status = c.Status,
                    SubmissionDate = c.SubmissionDate,
                    ApprovalDate = c.ApprovalDate,
                    ApprovedByName = c.Approver?.FullName ?? "Unknown"
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading approved claims: " + ex.Message;
                return View(new List<ClaimViewModel>());
            }
        }

        // POST: /Coordinator/RejectClaim/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectClaim(int id, string rejectionReason)
        {
            try
            {
                var claim = await _dataService.GetClaimAsync(id);
                if (claim == null)
                {
                    TempData["ErrorMessage"] = "Claim not found.";
                    return RedirectToAction("PendingClaims");
                }

                if (string.IsNullOrWhiteSpace(rejectionReason))
                {
                    TempData["ErrorMessage"] = "Please provide a reason for rejection.";
                    return RedirectToAction("ReviewClaim", new { id });
                }

                var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                claim.Status = "Rejected";
                claim.ApprovalDate = DateTime.Now;
                claim.ApprovedBy = approverId;
                claim.AdditionalNotes += $"\n\nRejection Reason: {rejectionReason}";

                await _dataService.UpdateClaimAsync(claim);

                TempData["SuccessMessage"] = $"Claim #{id} rejected successfully.";
                return RedirectToAction("PendingClaims");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error rejecting claim: " + ex.Message;
                return RedirectToAction("PendingClaims");
            }
        }

        // GET: /Coordinator/ApprovedClaims

        // CLAIM APPROVAL WORKFLOW
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            var claim = await _dataService.GetClaimAsync(id);
            var approverId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            claim.Status = "Approved";
            claim.ApprovalDate = DateTime.Now;
            claim.ApprovedBy = approverId;

            await _dataService.UpdateClaimAsync(claim);
            TempData["SuccessMessage"] = $"Claim #{id} approved successfully!";
            return RedirectToAction("PendingClaims");
        }

    }
}