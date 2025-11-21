using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Prog6212.Models;
using Prog6212.Services;
using Prog6212.ViewModels;

namespace Prog6212.Controllers
{
    [Authorize]
    public class ClaimsController : Controller
    {
        private readonly IDataService _dataService;

        public ClaimsController(IDataService dataService)
        {
            _dataService = dataService;
        }

        // GET: /Claims/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _dataService.GetUserAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                ViewBag.UserHourlyRate = user.HourlyRate;
                ViewBag.UserName = user.FullName;
                return View(new CreateClaimViewModel());
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading claim form: " + ex.Message;
                return RedirectToAction("Index", "Dashboard");
            }
        }

        // POST: /Claims/Create - SIMPLIFIED PARAMETERS
        // POST: /Claims/Create - FIXED VERSION
        // POST: /Claims/Create - FIXED DATABASE SAVE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateClaimViewModel model, IFormFile SupportingDocument)
        {
            try
            {
                // Get current user
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _dataService.GetUserAsync(userId);

                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found. Please login again.";
                    return RedirectToAction("Login", "Account");
                }

                // Set view data
                ViewBag.UserHourlyRate = user.HourlyRate;
                ViewBag.UserName = user.FullName;

                // Manual model validation
                if (model.HoursWorked <= 0 || model.HoursWorked > 180)
                {
                    ModelState.AddModelError("HoursWorked", "Hours worked must be between 0.1 and 180");
                }

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = "Please fix the validation errors below.";
                    return View(model);
                }

                // Check monthly hours limit
                var currentMonthHours = await _dataService.GetUserMonthlyHoursAsync(userId, DateTime.Now.Month, DateTime.Now.Year);
                if (currentMonthHours + (int)model.HoursWorked > 180)
                {
                    ModelState.AddModelError("HoursWorked",
                        $"Cannot exceed 180 hours per month. You've already worked {currentMonthHours} hours this month.");
                    TempData["ErrorMessage"] = $"Monthly limit exceeded. You've worked {currentMonthHours} hours this month.";
                    return View(model);
                }

                // Create and save the claim - SIMPLIFIED
                var claim = new LecturerClaim
                {
                    UserId = userId,
                    HoursWorked = model.HoursWorked,
                    HourlyRate = user.HourlyRate,
                    AdditionalNotes = model.AdditionalNotes,
                    Status = "Pending",
                    SubmissionDate = DateTime.Now
                };

                // Handle file upload if provided
                if (SupportingDocument != null && SupportingDocument.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var fileName = $"{Guid.NewGuid()}_{SupportingDocument.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await SupportingDocument.CopyToAsync(stream);
                    }

                    claim.SupportingDocument = fileName;
                }

                // DEBUG: Check claim values before saving
                Console.WriteLine($"Creating claim - UserId: {claim.UserId}, Hours: {claim.HoursWorked}, Rate: {claim.HourlyRate}");

                // Save the claim using the service
                var createdClaim = await _dataService.AddClaimAsync(claim);

                TempData["SuccessMessage"] = $"Claim #{createdClaim.Id} submitted successfully for R{claim.TotalAmount:N2}!";
                return RedirectToAction("Index", "Claims");
            }
            catch (Exception ex)
            {
                // Get the inner exception for more details
                var errorMessage = ex.Message;
                if (ex.InnerException != null)
                {
                    errorMessage += " | Inner: " + ex.InnerException.Message;
                }

                TempData["ErrorMessage"] = "Error submitting claim: " + errorMessage;

                // Re-populate view data
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _dataService.GetUserAsync(userId);
                ViewBag.UserHourlyRate = user?.HourlyRate ?? 0;
                ViewBag.UserName = user?.FullName ?? "User";

                return View(model);
            }
        }

        // GET: /Claims/Index (My Claims)
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var claims = await _dataService.GetClaimsByUserIdAsync(userId);

                var viewModel = claims.Select(c => new ClaimSummaryViewModel
                {
                    Id = c.Id,
                    LecturerName = c.User?.FullName ?? "Unknown",
                    HoursWorked = c.HoursWorked,
                    HourlyRate = c.HourlyRate,
                    TotalAmount = c.TotalAmount,
                    Status = c.Status,
                    SubmissionDate = c.SubmissionDate,
                    HasDocument = !string.IsNullOrEmpty(c.SupportingDocument),
                    SupportingDocument = c.SupportingDocument
                }).ToList();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading claims: " + ex.Message;
                return View(new List<ClaimSummaryViewModel>());
            }
        }

        // GET: /Claims/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var claim = await _dataService.GetClaimAsync(id);
                if (claim == null)
                    return NotFound();

                // Check if user owns this claim
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (claim.UserId != userId)
                    return Forbid();

                // Convert to your existing ClaimViewModel
                var viewModel = new ClaimViewModel
                {
                    Id = claim.Id,
                    HoursWorked = claim.HoursWorked,
                    HourlyRate = claim.HourlyRate,
                    AdditionalNotes = claim.AdditionalNotes,
                    SupportingDocumentPath = claim.SupportingDocument,
                    LecturerName = claim.User?.FullName ?? "Unknown",
                    Status = claim.Status,
                    SubmissionDate = claim.SubmissionDate
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error loading claim details: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
    }
}