using LiftAndShift.Application.Common.Exceptions;
using LiftAndShift.Application.Onboarding.Commands.SaveUserOnboarding;
using LiftAndShift.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace LiftAndShift.Application.FunctionalTests.Onboarding.Commands;

public class SaveUserOnboardingTests : TestBase
{
    private static SaveUserOnboardingCommand ValidCommand() => new()
    {
        PreferredUnit = "Lbs",
        BodyWeight = 180m,
        AlternatingLift = "PowerClean",
        SquatStartingWeight = 135m,
        BenchPressStartingWeight = 95m,
        OverheadPressStartingWeight = 65m,
        DeadliftStartingWeight = 155m,
        AlternatingLiftStartingWeight = 95m
    };

    [Test]
    public async Task ShouldRequireAuthenticatedUser()
    {
        await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            TestApp.SendAsync(ValidCommand()));
    }

    [Test]
    public async Task ShouldRequireValidInput()
    {
        await TestApp.RunAsDefaultUserAsync();

        var command = new SaveUserOnboardingCommand
        {
            PreferredUnit = "Stones",
            BodyWeight = 0m,
            AlternatingLift = "Invalid",
            SquatStartingWeight = 0m,
            BenchPressStartingWeight = 0m,
            OverheadPressStartingWeight = 0m,
            DeadliftStartingWeight = 0m,
            AlternatingLiftStartingWeight = 0m
        };

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldSaveOnboardingData()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();
        var command = ValidCommand();

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeTrue();

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);

        user.ShouldNotBeNull();
        user!.IsOnboarded.ShouldBeTrue();
        user.PreferredUnit.Name.ShouldBe(command.PreferredUnit);
        user.BodyWeight.ShouldBe(command.BodyWeight);
        user.AlternatingLift.Name.ShouldBe(command.AlternatingLift);
        user.SquatStartingWeight.ShouldBe(command.SquatStartingWeight);
        user.BenchPressStartingWeight.ShouldBe(command.BenchPressStartingWeight);
        user.OverheadPressStartingWeight.ShouldBe(command.OverheadPressStartingWeight);
        user.DeadliftStartingWeight.ShouldBe(command.DeadliftStartingWeight);
        user.AlternatingLiftStartingWeight.ShouldBe(command.AlternatingLiftStartingWeight);
    }

    [Test]
    public async Task ShouldMarkUserAsOnboarded_AfterSaving()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        using var scopeBefore = FunctionalTestSetup.ScopeFactory.CreateScope();
        var userManagerBefore = scopeBefore.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var userBefore = await userManagerBefore.FindByIdAsync(userId);
        userBefore!.IsOnboarded.ShouldBeFalse();

        await TestApp.SendAsync(ValidCommand());

        using var scopeAfter = FunctionalTestSetup.ScopeFactory.CreateScope();
        var userManagerAfter = scopeAfter.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var userAfter = await userManagerAfter.FindByIdAsync(userId);
        userAfter!.IsOnboarded.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldAcceptKgsAndPendlayRow()
    {
        await TestApp.RunAsDefaultUserAsync();

        var command = ValidCommand() with
        {
            PreferredUnit = "Kgs",
            AlternatingLift = "PendlayRow"
        };

        var result = await TestApp.SendAsync(command);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldOverwritePreviousOnboardingData()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        await TestApp.SendAsync(ValidCommand());

        var updated = ValidCommand() with
        {
            PreferredUnit = "Kgs",
            BodyWeight = 90m,
            AlternatingLift = "PendlayRow"
        };

        var result = await TestApp.SendAsync(updated);

        result.Succeeded.ShouldBeTrue();

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);

        user!.PreferredUnit.Name.ShouldBe("Kgs");
        user.BodyWeight.ShouldBe(90m);
        user.AlternatingLift.Name.ShouldBe("PendlayRow");
    }
}
