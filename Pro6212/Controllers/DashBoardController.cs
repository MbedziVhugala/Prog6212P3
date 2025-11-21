using Microsoft.AspNetCore.Mvc;
using Prog6212.Models;
using Prog6212.Services;
using Prog6212.ViewModels;
using System.Security.Claims;

namespace Prog6212.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDataService _dataService;

        public DashboardController(IDataService dataService)
        {
            _dataService = dataService;
        }

        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;

            var claims = await _dataService.GetClaimsAsync();
            var users = await _dataService.GetUsersAsync();

            var viewModel = new DashboardViewModel
            {
                UserName = userName,
                UserRole = userRole
            };

            if (userRole == "Lecturer")
            {
                var lecturerClaims = await _dataService.GetClaimsByUserIdAsync(userId);
                viewModel.RecentClaims = lecturerClaims
                    .Take(5)
                    .Select(c => new ClaimSummaryViewModel
                    {
                        Id = c.Id,
                        LecturerName = c.User?.FullName,
                        HoursWorked = c.HoursWorked,
                        HourlyRate = c.HourlyRate,
                        TotalAmount = c.HoursWorked * c.HourlyRate,
                        Status = c.Status,
                        SubmissionDate = c.SubmissionDate,
                        HasDocument = !string.IsNullOrEmpty(c.SupportingDocument),
                        SupportingDocument = c.SupportingDocument
                    })
                    .ToList();

                viewModel.Stats = new DashboardStats
                {
                    TotalClaims = lecturerClaims.Count,
                    PendingClaims = lecturerClaims.Count(c => c.Status == "Pending"),
                    ApprovedClaims = lecturerClaims.Count(c => c.Status == "Approved"),
                    RejectedClaims = lecturerClaims.Count(c => c.Status == "Rejected"),
                    TotalAmountApproved = lecturerClaims
                        .Where(c => c.Status == "Approved")
                        .Sum(c => c.HoursWorked * c.HourlyRate)
                };
            }
            else if (userRole == "Coordinator" || userRole == "Manager")
            {
                var pendingClaims = await _dataService.GetPendingClaimsAsync();

                viewModel.PendingApprovals = pendingClaims
                    .Take(5)
                    .Select(c => new ClaimSummaryViewModel
                    {
                        Id = c.Id,
                        LecturerName = c.User?.FullName,
                        HoursWorked = c.HoursWorked,
                        HourlyRate = c.HourlyRate,
                        TotalAmount = c.HoursWorked * c.HourlyRate,
                        Status = c.Status,
                        SubmissionDate = c.SubmissionDate,
                        HasDocument = !string.IsNullOrEmpty(c.SupportingDocument),
                        SupportingDocument = c.SupportingDocument
                    })
                    .ToList();

                viewModel.Stats = new DashboardStats
                {
                    TotalClaims = claims.Count,
                    PendingClaims = pendingClaims.Count,
                    ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                    RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                    TotalAmountApproved = claims
                        .Where(c => c.Status == "Approved")
                        .Sum(c => c.HoursWorked * c.HourlyRate)
                };
            }
            else if (userRole == "HR")
            {
                var allUsers = await _dataService.GetUsersAsync();
                var pendingClaims = await _dataService.GetPendingClaimsAsync();

                viewModel.Stats = new DashboardStats
                {
                    TotalClaims = claims.Count,
                    PendingClaims = pendingClaims.Count,
                    ApprovedClaims = claims.Count(c => c.Status == "Approved"),
                    RejectedClaims = claims.Count(c => c.Status == "Rejected"),
                    TotalUsers = allUsers.Count,
                    TotalAmountApproved = claims
                        .Where(c => c.Status == "Approved")
                        .Sum(c => c.HoursWorked * c.HourlyRate)
                };

                // HR can see all pending claims (read-only)
                viewModel.PendingApprovals = pendingClaims
                    .Take(5)
                    .Select(c => new ClaimSummaryViewModel
                    {
                        Id = c.Id,
                        LecturerName = c.User?.FullName,
                        HoursWorked = c.HoursWorked,
                        HourlyRate = c.HourlyRate,
                        TotalAmount = c.HoursWorked * c.HourlyRate,
                        Status = c.Status,
                        SubmissionDate = c.SubmissionDate,
                        HasDocument = !string.IsNullOrEmpty(c.SupportingDocument),
                        SupportingDocument = c.SupportingDocument
                    })
                    .ToList();
            }

            return View(viewModel); // MOVED THIS LINE - Was inside HR block
        }
    }
}