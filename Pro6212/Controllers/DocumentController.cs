using Microsoft.AspNetCore.Mvc;
using Prog6212.Models;
using Prog6212.Services;
using Prog6212.ViewModels;
using ClaimModel = Prog6212.Models.LecturerClaim;
using System.Security.Claims;


namespace Prog6212.Controllers


{
    public class DocumentController : Controller
    {
        private readonly IDataService _dataService;
        private readonly IWebHostEnvironment _environment;

        public DocumentController(IDataService dataService, IWebHostEnvironment environment)
        {
            _dataService = dataService;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Upload(int claimId)
        {
            var claims = await _dataService.GetClaimsAsync();
            var claim = claims.FirstOrDefault(c => c.Id == claimId);

            if (claim == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            if (claim.UserId != userId)
                return RedirectToAction("AccessDenied", "Account");

            var viewModel = new ClaimViewModel
            {
                Id = claim.Id,
                HoursWorked = claim.HoursWorked,
                HourlyRate = claim.HourlyRate,
                SubmissionDate = claim.SubmissionDate
            };


            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(int claimId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction("Upload", new { claimId });
            }

            try
            {
                var claims = await _dataService.GetClaimsAsync();
                var claim = claims.FirstOrDefault(c => c.Id == claimId);

                if (claim == null)
                    return NotFound();

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (claim.UserId != userId)
                    return RedirectToAction("AccessDenied", "Account");

                // Save the file
                var fileName = await SaveDocumentAsync(file);
                claim.SupportingDocument = fileName;

                await _dataService.UpdateClaimAsync(claim);

                TempData["SuccessMessage"] = "Document uploaded successfully!";
                return RedirectToAction("Details", "Claim", new { id = claimId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error uploading document: {ex.Message}";
                return RedirectToAction("Upload", new { claimId });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Download(int claimId)
        {
            var claims = await _dataService.GetClaimsAsync();
            var claim = claims.FirstOrDefault(c => c.Id == claimId);

            if (claim == null || string.IsNullOrEmpty(claim.SupportingDocument))
                return NotFound();

            var filePath = Path.Combine(_environment.WebRootPath, "uploads", claim.SupportingDocument);
            if (!System.IO.File.Exists(filePath))
                return NotFound();

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            var contentType = GetContentType(filePath);
            return File(memory, contentType, claim.SupportingDocument);
        }

        private async Task<string> SaveDocumentAsync(IFormFile file)
        {
            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                throw new InvalidOperationException("Invalid file type. Allowed: PDF, DOCX, XLSX, JPG, PNG");

            if (file.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("File size exceeds 10MB limit");

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        private string GetContentType(string path)
        {
            var types = new Dictionary<string, string>
            {
                { ".pdf", "application/pdf" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" }
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types.ContainsKey(ext) ? types[ext] : "application/octet-stream";
        }
    }
}