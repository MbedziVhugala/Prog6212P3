using Prog6212.Models;

public interface IDataService
{
    // User methods
    Task<List<User>> GetUsersAsync();
    Task<User> GetUserAsync(int id);
    Task<User> GetUserByEmailAsync(string email);
    Task SaveUsersAsync(List<User> users);
    Task<User> AddUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task DeleteUserAsync(int userId); // Make sure this exists

    // Claim methods
    Task<List<LecturerClaim>> GetClaimsAsync();
    Task<LecturerClaim> GetClaimAsync(int id);
    Task<LecturerClaim> AddClaimAsync(LecturerClaim claim);
    Task<LecturerClaim> UpdateClaimAsync(LecturerClaim claim);
    Task DeleteClaimAsync(int claimId); // Make sure this exists
    Task SaveClaimsAsync(List<LecturerClaim> claims);

    // Reporting methods
    Task<List<LecturerClaim>> GetClaimsByUserIdAsync(int userId);
    Task<List<LecturerClaim>> GetPendingClaimsAsync();
    Task<int> GetUserMonthlyHoursAsync(int userId, int month, int year);
    Task<object> GetClaimsReportAsync();
}