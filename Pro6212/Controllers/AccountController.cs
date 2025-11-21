using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Prog6212.Models;
using Prog6212.Services;

namespace Prog6212.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDataService _dataService;

        public AccountController(IDataService dataService)
        {
            _dataService = dataService;
        }

        // GET: Login
        [HttpGet]
        public IActionResult Login() => View();

        // POST: Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _dataService.GetUserByEmailAsync(email);

            if (user == null || user.Password != password)
            {
                ViewBag.Error = "Invalid email or password.";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Index", "Dashboard");
        }

        // HR ONLY: User Management
        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            if (!User.IsInRole("HR"))
                return RedirectToAction("AccessDenied", "Account");

            var users = await _dataService.GetUsersAsync();
            return View(users);
        }

        // HR ONLY: Create User
        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!User.IsInRole("HR"))
                return RedirectToAction("AccessDenied", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string fullName, string email, string password, string role, decimal hourlyRate)
        {
            if (!User.IsInRole("HR"))
                return RedirectToAction("AccessDenied", "Account");

            var existingUser = await _dataService.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                ViewBag.Error = "Email already exists.";
                return View();
            }

            var newUser = new User
            {
                FullName = fullName,
                Email = email,
                Password = password,
                Role = role,
                HourlyRate = hourlyRate,
                IsActive = true,
                CreatedDate = System.DateTime.Now
            };

            await _dataService.AddUserAsync(newUser);

            TempData["SuccessMessage"] = "User created successfully!";
            return RedirectToAction("ManageUsers");
        }

        // GET: Edit User
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            if (!User.IsInRole("HR"))
                return RedirectToAction("AccessDenied", "Account");

            var user = await _dataService.GetUserAsync(id);
            if (user == null)
                return NotFound();

            return View(user); // This will look for EditUser.cshtml
        }

        // POST: Edit User  
        [HttpPost]
        public async Task<IActionResult> EditUser(User updatedUser)
        {
            if (!User.IsInRole("HR"))
                return RedirectToAction("AccessDenied", "Account");

            try
            {
                // Use the UpdateUserAsync method instead of SaveUsersAsync
                await _dataService.UpdateUserAsync(updatedUser);
                TempData["SuccessMessage"] = "User updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating user: " + ex.Message;
                return View(updatedUser);
            }

            return RedirectToAction("ManageUsers");
        }

        // HR ONLY: Delete User
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!User.IsInRole("HR"))
                return RedirectToAction("AccessDenied", "Account");

            await _dataService.DeleteUserAsync(id);
            TempData["SuccessMessage"] = "User deleted successfully!";

            return RedirectToAction("ManageUsers");
        }

        // GET: Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}