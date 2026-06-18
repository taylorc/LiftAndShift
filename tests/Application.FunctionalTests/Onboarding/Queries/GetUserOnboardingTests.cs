using LiftAndShift.Application.Onboarding.Commands.SaveUserOnboarding;
using LiftAndShift.Application.Onboarding.Queries.GetUserOnboarding;

namespace LiftAndShift.Application.FunctionalTests.Onboarding.Queries;

public class GetUserOnboardingTests : TestBase
{
    [Test]
    public async Task ShouldRequireAuthenticatedUser()
    {
        await Should.ThrowAsync<UnauthorizedAccessException>(() =>
            TestApp.SendAsync(new GetUserOnboardingQuery()));
    }

    [Test]
    public async Task ShouldReturnDefaults_ForNewUser()
    {
        await TestApp.RunAsDefaultUserAsync();

        var result = await TestApp.SendAsync(new GetUserOnboardingQuery());

        result.ShouldNotBeNull();
        result.IsOnboarded.ShouldBeFalse();
        result.PreferredUnit.ShouldBe("Lbs");
        result.AlternatingLift.ShouldBe("PowerClean");
        result.BodyWeight.ShouldBeNull();
        result.SquatStartingWeight.ShouldBeNull();
        result.BenchPressStartingWeight.ShouldBeNull();
        result.OverheadPressStartingWeight.ShouldBeNull();
        result.DeadliftStartingWeight.ShouldBeNull();
        result.AlternatingLiftStartingWeight.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnSavedData_AfterOnboarding()
    {
        await TestApp.RunAsDefaultUserAsync();

        var command = new SaveUserOnboardingCommand
        {
            PreferredUnit = "Kgs",
            BodyWeight = 80m,
            AlternatingLift = "PendlayRow",
            SquatStartingWeight = 60m,
            BenchPressStartingWeight = 40m,
            OverheadPressStartingWeight = 30m,
            DeadliftStartingWeight = 70m,
            AlternatingLiftStartingWeight = 50m
        };

        await TestApp.SendAsync(command);

        var result = await TestApp.SendAsync(new GetUserOnboardingQuery());

        result.IsOnboarded.ShouldBeTrue();
        result.PreferredUnit.ShouldBe("Kgs");
        result.BodyWeight.ShouldBe(80m);
        result.AlternatingLift.ShouldBe("PendlayRow");
        result.SquatStartingWeight.ShouldBe(60m);
        result.BenchPressStartingWeight.ShouldBe(40m);
        result.OverheadPressStartingWeight.ShouldBe(30m);
        result.DeadliftStartingWeight.ShouldBe(70m);
        result.AlternatingLiftStartingWeight.ShouldBe(50m);
    }

    [Test]
    public async Task ShouldReflectUpdatedData_AfterReonboarding()
    {
        await TestApp.RunAsDefaultUserAsync();

        await TestApp.SendAsync(new SaveUserOnboardingCommand
        {
            PreferredUnit = "Lbs",
            BodyWeight = 180m,
            AlternatingLift = "PowerClean",
            SquatStartingWeight = 135m,
            BenchPressStartingWeight = 95m,
            OverheadPressStartingWeight = 65m,
            DeadliftStartingWeight = 155m,
            AlternatingLiftStartingWeight = 95m
        });

        await TestApp.SendAsync(new SaveUserOnboardingCommand
        {
            PreferredUnit = "Kgs",
            BodyWeight = 90m,
            AlternatingLift = "PendlayRow",
            SquatStartingWeight = 70m,
            BenchPressStartingWeight = 50m,
            OverheadPressStartingWeight = 35m,
            DeadliftStartingWeight = 80m,
            AlternatingLiftStartingWeight = 55m
        });

        var result = await TestApp.SendAsync(new GetUserOnboardingQuery());

        result.PreferredUnit.ShouldBe("Kgs");
        result.BodyWeight.ShouldBe(90m);
        result.AlternatingLift.ShouldBe("PendlayRow");
        result.SquatStartingWeight.ShouldBe(70m);
    }
}
