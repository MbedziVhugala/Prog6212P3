using Microsoft.EntityFrameworkCore;
using Prog6212.Data;
using Prog6212.Models;
using Prog6212.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Prog6212.Services
{
    public class EFDataService : IDataService
    {
        private readonly AppDbContext _context;

        public EFDataService(AppDbContext context)
        {
            _context = context;
        }

        // ================================
        // USER METHODS
        // ================================
        public async Task<List<User>> GetUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task SaveUsersAsync(List<User> users)
        {
            // Update existing users and add new ones (safe approach)
            foreach (var user in users)
            {
                var existingUser = await _context.Users.FindAsync(user.Id);
                if (existingUser == null)
                {
                    _context.Users.Add(user);
                }
                else
                {
                    _context.Entry(existingUser).CurrentValues.SetValues(user);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<User> AddUserAsync(User user)
        {
            // Remove manual ID assignment - let EF/database handle it
            // If your database table has Identity set, EF will ignore the ID you set
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        // ================================
        // CLAIM METHODS
        // ================================
        public async Task<List<LecturerClaim>> GetClaimsAsync()
        {
            return await _context.LecturerClaims
                .Include(c => c.User)
                .Include(c => c.Approver)
                .ToListAsync();
        }

        public async Task<LecturerClaim?> GetClaimAsync(int id)
        {
            return await _context.LecturerClaims
                .Include(c => c.User)
                .Include(c => c.Approver)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<LecturerClaim> AddClaimAsync(LecturerClaim claim)
        {
            try
            {
                Console.WriteLine($"EFDataService: Adding claim - UserId: {claim.UserId}, Hours: {claim.HoursWorked}, Rate: {claim.HourlyRate}");

                // Don't set ID manually - let database handle it
                // Remove any manual ID setting

                _context.LecturerClaims.Add(claim);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Claim saved successfully with ID: {claim.Id}");
                return claim;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EFDataService ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<LecturerClaim> UpdateClaimAsync(LecturerClaim claim)
        {
            _context.LecturerClaims.Update(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task DeleteClaimAsync(int id)
        {
            var claim = await _context.LecturerClaims.FindAsync(id);
            if (claim != null)
            {
                _context.LecturerClaims.Remove(claim);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveClaimsAsync(List<LecturerClaim> claims)
        {
            // Update existing claims and add new ones
            foreach (var claim in claims)
            {
                var existingClaim = await _context.LecturerClaims.FindAsync(claim.Id);
                if (existingClaim == null)
                {
                    _context.LecturerClaims.Add(claim);
                }
                else
                {
                    _context.Entry(existingClaim).CurrentValues.SetValues(claim);
                }
            }

            await _context.SaveChangesAsync();
        }

        // ================================
        // SPECIALIZED METHODS
        // ================================
        public async Task<List<LecturerClaim>> GetClaimsByUserIdAsync(int userId)
        {
            return await _context.LecturerClaims
                .Where(c => c.UserId == userId)
                .Include(c => c.User)
                .Include(c => c.Approver)
                .OrderByDescending(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<List<LecturerClaim>> GetPendingClaimsAsync()
        {
            return await _context.LecturerClaims
                .Where(c => c.Status == "Pending")
                .Include(c => c.User)
                .OrderBy(c => c.SubmissionDate)
                .ToListAsync();
        }

        public async Task<int> GetUserMonthlyHoursAsync(int userId, int month, int year)
        {
            var claims = await _context.LecturerClaims
                .Where(c => c.UserId == userId &&
                           c.SubmissionDate.Month == month &&
                           c.SubmissionDate.Year == year)
                .ToListAsync();

            return (int)claims.Sum(c => c.HoursWorked);
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