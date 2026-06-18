using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Models;
using LiftAndShift.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LiftAndShift.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }

    public async Task<UserOnboardingDto?> GetUserOnboardingAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        return new UserOnboardingDto
        {
            IsOnboarded = user.IsOnboarded,
            PreferredUnit = user.PreferredUnit.Name,
            BodyWeight = user.BodyWeight,
            AlternatingLift = user.AlternatingLift.Name,
            SquatStartingWeight = user.SquatStartingWeight,
            BenchPressStartingWeight = user.BenchPressStartingWeight,
            OverheadPressStartingWeight = user.OverheadPressStartingWeight,
            DeadliftStartingWeight = user.DeadliftStartingWeight,
            AlternatingLiftStartingWeight = user.AlternatingLiftStartingWeight
        };
    }

    public async Task<Result> SaveUserOnboardingAsync(string userId, UserOnboardingDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return Result.Failure(["User not found."]);

        user.IsOnboarded = true;
        user.PreferredUnit = WeightUnit.FromName(dto.PreferredUnit);
        user.BodyWeight = dto.BodyWeight;
        user.AlternatingLift = AlternatingLiftType.FromName(dto.AlternatingLift);
        user.SquatStartingWeight = dto.SquatStartingWeight;
        user.BenchPressStartingWeight = dto.BenchPressStartingWeight;
        user.OverheadPressStartingWeight = dto.OverheadPressStartingWeight;
        user.DeadliftStartingWeight = dto.DeadliftStartingWeight;
        user.AlternatingLiftStartingWeight = dto.AlternatingLiftStartingWeight;

        var result = await _userManager.UpdateAsync(user);
        return result.ToApplicationResult();
    }
}
