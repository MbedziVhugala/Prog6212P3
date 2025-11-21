using Prog6212.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prog6212.Services
{
    public class FakeDataService : IDataService
    {
        private List<User> _users = new List<User>();
        private List<LecturerClaim> _claims = new List<LecturerClaim>();
        private int _nextUserId = 1;
        private int _nextClaimId = 1;

        public FakeDataService()
        {
            // Initialize with some sample data
            InitializeSampleData();
        }

        private void InitializeSampleData()
        {
            // Add sample users
            _users.Add(new User { Id = _nextUserId++, FullName = "HR User", Email = "hr@university.com", Password = "hr123", Role = "HR", HourlyRate = 0, IsActive = true });
            _users.Add(new User { Id = _nextUserId++, FullName = "John Lecturer", Email = "john@university.com", Password = "lecturer123", Role = "Lecturer", HourlyRate = 250, IsActive = true });
            _users.Add(new User { Id = _nextUserId++, FullName = "Coordinator User", Email = "coordinator@university.com", Password = "coord123", Role = "Coordinator", HourlyRate = 0, IsActive = true });
            _users.Add(new User { Id = _nextUserId++, FullName = "Manager User", Email = "manager@university.com", Password = "manager123", Role = "Manager", HourlyRate = 0, IsActive = true });
        }

        // ================================
        // USER METHODS
        // ================================
        public async Task<List<User>> GetUsersAsync()
        {
            return await Task.FromResult(_users);
        }

        public async Task<User> GetUserAsync(int id)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Id == id));
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await Task.FromResult(_users.FirstOrDefault(u => u.Email.ToLower() == email.ToLower()));
        }

        public async Task SaveUsersAsync(List<User> users)
        {
            _users = users;
            await Task.CompletedTask;
        }

        public async Task<User> AddUserAsync(User user)
        {
            user.Id = _nextUserId++;
            _users.Add(user);
            return await Task.FromResult(user);
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            var existingUser = _users.FirstOrDefault(u => u.Id == user.Id);
            if (existingUser != null)
            {
                _users.Remove(existingUser);
                _users.Add(user);
            }
            return await Task.FromResult(user);
        }

        // ADD THE MISSING DELETE METHOD
        public async Task DeleteUserAsync(int userId)
        {
            var user = _users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                _users.Remove(user);
            }
            await Task.CompletedTask;
        }

        // ================================
        // CLAIM METHODS
        // ================================
        public async Task<List<LecturerClaim>> GetClaimsAsync()
        {
            // Populate navigation properties
            var claimsWithUsers = _claims.Select(claim =>
            {
                claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
                claim.Approver = _users.FirstOrDefault(u => u.Id == claim.ApprovedBy);
                return claim;
            }).ToList();

            return await Task.FromResult(claimsWithUsers);
        }

        public async Task<LecturerClaim> GetClaimAsync(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
                claim.Approver = _users.FirstOrDefault(u => u.Id == claim.ApprovedBy);
            }
            return await Task.FromResult(claim);
        }

        public async Task<LecturerClaim> AddClaimAsync(LecturerClaim claim)
        {
            claim.Id = _nextClaimId++;
            _claims.Add(claim);
            return await Task.FromResult(claim);
        }

        public async Task<LecturerClaim> UpdateClaimAsync(LecturerClaim updatedClaim)
        {
            var existingClaim = _claims.FirstOrDefault(c => c.Id == updatedClaim.Id);
            if (existingClaim != null)
            {
                _claims.Remove(existingClaim);
                _claims.Add(updatedClaim);
            }
            return await Task.FromResult(updatedClaim);
        }

        // ADD THE MISSING DELETE CLAIM METHOD
        public async Task DeleteClaimAsync(int claimId)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == claimId);
            if (claim != null)
            {
                _claims.Remove(claim);
            }
            await Task.CompletedTask;
        }

        public async Task SaveClaimsAsync(List<LecturerClaim> claims)
        {
            _claims = claims;
            await Task.CompletedTask;
        }

        // ================================
        // MISSING METHODS - ADD THESE
        // ================================

        public async Task<List<LecturerClaim>> GetClaimsByUserIdAsync(int userId)
        {
            var userClaims = _claims.Where(c => c.UserId == userId).ToList();

            // Populate navigation properties
            foreach (var claim in userClaims)
            {
                claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
                claim.Approver = _users.FirstOrDefault(u => u.Id == claim.ApprovedBy);
            }

            return await Task.FromResult(userClaims.OrderByDescending(c => c.SubmissionDate).ToList());
        }

        public async Task<List<LecturerClaim>> GetPendingClaimsAsync()
        {
            var pendingClaims = _claims.Where(c => c.Status == "Pending").ToList();

            // Populate navigation properties
            foreach (var claim in pendingClaims)
            {
                claim.User = _users.FirstOrDefault(u => u.Id == claim.UserId);
            }

            return await Task.FromResult(pendingClaims.OrderBy(c => c.SubmissionDate).ToList());
        }

        public async Task<int> GetUserMonthlyHoursAsync(int userId, int month, int year)
        {
            var monthlyClaims = _claims.Where(c =>
                c.UserId == userId &&
                c.SubmissionDate.Month == month &&
                c.SubmissionDate.Year == year).ToList();

            var totalHours = (int)monthlyClaims.Sum(c => c.HoursWorked);
            return await Task.FromResult(totalHours);
        }

        public async Task<object> GetClaimsReportAsync()
        {
            var claims = await GetClaimsAsync();
            var users = await GetUsersAsync();

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