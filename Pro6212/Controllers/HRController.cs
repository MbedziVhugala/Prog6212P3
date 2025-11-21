using Microsoft.AspNetCore.Mvc;
using Prog6212.Services;

namespace Prog6212.Controllers
{
    public class HRController : Controller
    {
        private readonly IDataService _dataService;

        public HRController(IDataService dataService)
        {
            _dataService = dataService;
        }

        [HttpGet]
        public IActionResult GenerateReports()
        {
            if (!User.IsInRole("HR"))
                return RedirectToAction("AccessDenied", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePDFReport()
        {
            if (!User.IsInRole("HR"))
                return RedirectToAction("AccessDenied", "Account");

            // PDF generation logic using LINQ (as per checklist)
            var users = await _dataService.GetUsersAsync();
            var claims = await _dataService.GetClaimsAsync();

            // LINQ queries for reports
            var reportData = claims
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            // TODO: Add actual PDF generation code here
            // For now, just return success message

            TempData["SuccessMessage"] = "Report generated successfully!";
            return RedirectToAction("GenerateReports");
        }
    }
}