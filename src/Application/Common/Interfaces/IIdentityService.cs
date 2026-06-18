using LiftAndShift.Application.Common.Models;

namespace LiftAndShift.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<UserOnboardingDto?> GetUserOnboardingAsync(string userId);

    Task<Result> SaveUserOnboardingAsync(string userId, UserOnboardingDto dto);
}
