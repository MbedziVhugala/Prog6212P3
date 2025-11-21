using System.Text.Json;
using Prog6212.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prog6212.Services
{
    public class DataService : IDataService
    {
        private readonly string _dataPath;
        private readonly JsonSerializerOptions _options;

        private List<User> _users = new();
        private List<LecturerClaim> _claims = new();

        public DataService(IWebHostEnvironment environment)
        {
            _dataPath = Path.Combine(environment.ContentRootPath, "App_Data");
            _options = new JsonSerializerOptions { WriteIndented = true };

            Directory.CreateDirectory(_dataPath);
            InitializeDataAsync().Wait();
        }

        private async Task InitializeDataAsync()
        {
            _users = await LoadUsersAsync();
            _claims = await LoadClaimsAsync();

            // Populate navigation properties
            foreach (var claim in _claims)
            {
                claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
                if (claim.ApprovedBy.HasValue)
                {
                    claim.Approver = _users.FirstOrDefault(u => u.Id == claim.ApprovedBy.Value);
                }
            }
        }

        // ================================
        // USER METHODS
        // ================================
        private async Task<List<User>> LoadUsersAsync()
        {
            var filePath = Path.Combine(_dataPath, "users.json");
            if (!File.Exists(filePath))
            {
                var defaultUsers = new List<User>
                {
                    new User { Id = 1, Email = "hr@iie.ac.za", Password = "hr123",
                              FullName = "HR Administrator", Role = "HR", HourlyRate = 0 },
                    new User { Id = 2, Email = "lecturer@iie.ac.za", Password = "lecturer123",
                              FullName = "Dr. John Smith", Role = "Lecturer", HourlyRate = 250 },
                    new User { Id = 3, Email = "coordinator@iie.ac.za", Password = "coordinator123",
                              FullName = "Ms. Sarah Wilson", Role = "Coordinator", HourlyRate = 0 },
                    new User { Id = 4, Email = "manager@iie.ac.za", Password = "manager123",
                              FullName = "Mr. David Brown", Role = "Manager", HourlyRate = 0 },
                    new User { Id = 5, Email = "lecturer2@iie.ac.za", Password = "lecturer123",
                              FullName = "Prof. Emily Johnson", Role = "Lecturer", HourlyRate = 280 }
                };
                await SaveUsersAsync(defaultUsers);
                return defaultUsers;
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
        }

        public async Task SaveUsersAsync(List<User> users)
        {
            var filePath = Path.Combine(_dataPath, "users.json");
            var json = JsonSerializer.Serialize(users, _options);
            await File.WriteAllTextAsync(filePath, json);
            _users = users; // Update local cache
        }

        // ================================
        // CLAIM METHODS
        // ================================
        private async Task<List<LecturerClaim>> LoadClaimsAsync()
        {
            var filePath = Path.Combine(_dataPath, "claims.json");
            if (!File.Exists(filePath))
            {
                var defaultClaims = new List<LecturerClaim>
                {
                    new LecturerClaim
                    {
                        Id = 1, UserId = 2, HoursWorked = 40, HourlyRate = 250,
                        AdditionalNotes = "Monthly teaching hours", Status = "Approved",
                        SubmissionDate = DateTime.Now.AddDays(-10), ApprovalDate = DateTime.Now.AddDays(-5),
                        ApprovedBy = 3
                    },
                    new LecturerClaim
                    {
                        Id = 2, UserId = 2, HoursWorked = 35, HourlyRate = 250,
                        AdditionalNotes = "Student consultations", Status = "Pending",
                        SubmissionDate = DateTime.Now.AddDays(-3)
                    },
                    new LecturerClaim
                    {
                        Id = 3, UserId = 5, HoursWorked = 45, HourlyRate = 280,
                        AdditionalNotes = "Research supervision", Status = "Pending",
                        SubmissionDate = DateTime.Now.AddDays(-1)
                    }
                };
                await SaveClaimsAsync(defaultClaims);
                return defaultClaims;
            }

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<LecturerClaim>>(json) ?? new List<LecturerClaim>();
        }

        public async Task SaveClaimsAsync(List<LecturerClaim> claims)
        {
            var filePath = Path.Combine(_dataPath, "claims.json");
            var json = JsonSerializer.Serialize(claims, _options);
            await File.WriteAllTextAsync(filePath, json);
            _claims = claims; // Update local cache
        }

        // ================================
        // PUBLIC INTERFACE IMPLEMENTATION
        // ================================
        public async Task<List<User>> GetUsersAsync() => _users;

        public async Task<List<LecturerClaim>> GetClaimsAsync() => _claims;

        public async Task<User> GetUserAsync(int id) => _users.FirstOrDefault(u => u.Id == id);

        public async Task<User> GetUserByEmailAsync(string email) =>
            _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        public async Task<User> AddUserAsync(User user)
        {
            user.Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1;
            _users.Add(user);
            await SaveUsersAsync(_users);
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                _users.Remove(existingUser);
                _users.Add(user);
                await SaveUsersAsync(_users);
            }
            return user;
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                _users.Remove(user);
                await SaveUsersAsync(_users);
            }
        }

        public async Task<LecturerClaim> GetClaimAsync(int id) => _claims.FirstOrDefault(c => c.Id == id);

        public async Task<LecturerClaim> AddClaimAsync(LecturerClaim claim)
        {
            claim.Id = _claims.Any() ? _claims.Max(c => c.Id) + 1 : 1;
            claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
            _claims.Add(claim);
            await SaveClaimsAsync(_claims);
            return claim;
        }

        public async Task<LecturerClaim> UpdateClaimAsync(LecturerClaim updatedClaim)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.Id == updatedClaim.Id);
            if (existingClaim != null)
            {
                _claims.Remove(existingClaim);
                updatedClaim.User = _users.FirstOrDefault(u => u.Id == updatedClaim.UserId);
                if (updatedClaim.ApprovedBy.HasValue)
                {
                    updatedClaim.Approver = _users.FirstOrDefault(u => u.Id == updatedClaim.ApprovedBy.Value);
                }
                _claims.Add(updatedClaim);
                await SaveClaimsAsync(_claims);
            }
            return updatedClaim;
        }

        public async Task DeleteClaimAsync(int claimId)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == claimId);
            if (claim != null)
            {
                _claims.Remove(claim);
                await SaveClaimsAsync(_claims);
            }
        }

        public async Task<List<LecturerClaim>> GetClaimsByUserIdAsync(int userId)
        {
            var userClaims = _claims.Where(c => c.UserId == userId).ToList();

            // Populate navigation properties
            foreach (var claim in userClaims)
            {
                claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
                if (claim.ApprovedBy.HasValue)
                {
                    claim.Approver = _users.FirstOrDefault(u => u.Id == claim.ApprovedBy.Value);
                }
            }

            return userClaims.OrderByDescending(c => c.SubmissionDate).ToList();
        }

        public async Task<List<LecturerClaim>> GetPendingClaimsAsync()
        {
            var pendingClaims = _claims.Where(c => c.Status == "Pending").ToList();

            // Populate navigation properties
            foreach (var claim in pendingClaims)
            {
                claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
            }

            return pendingClaims.OrderBy(c => c.SubmissionDate).ToList();
        }

        public async Task<int> GetUserMonthlyHoursAsync(int userId, int month, int year)
        {
            var monthlyClaims = _claims.Where(c =>
                c.UserId == userId &&
                c.SubmissionDate.Month == month &&
                c.SubmissionDate.Year == year).ToList();

            var totalHours = (int)monthlyClaims.Sum(c => c.HoursWorked);
            return totalHours;
        }

        public async Task<object> GetClaimsReportAsync()
        {
            var claims = _claims;
            var users = _users;

            // LINQ queries for reports
            var claimsByStatus = claims
                .GroupBy(c => c.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(c => c.HoursWorked * c.HourlyRate)
                })
                .ToList();

            var usersByRole = users
                .GroupBy(u => u.Role)
                .Select(g => new
                {
                    Role = g.Key,
                    Count = g.Count()
                })
                .ToList();

            var monthlyClaims = claims
                .GroupBy(c => new { c.SubmissionDate.Year, c.SubmissionDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    TotalAmount = g.Sum(c => c.HoursWorked * c.HourlyRate)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ToList();

            return new
            {
                ClaimsByStatus = claimsByStatus,
                UsersByRole = usersByRole,
                MonthlyClaims = monthlyClaims,
                TotalClaims = claims.Count,
                TotalUsers = users.Count,
                TotalAmountApproved = claims.Where(c => c.Status == "Approved").Sum(c => c.HoursWorked * c.HourlyRate),
                PendingClaims = claims.Count(c => c.Status == "Pending")
            };
        }
    }
}