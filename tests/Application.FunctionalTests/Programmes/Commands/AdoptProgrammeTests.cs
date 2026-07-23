using LiftAndShift.Application.Programmes.Commands.AdoptProgramme;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.FunctionalTests.Programmes.Commands;

public class AdoptProgrammeTests : TestBase
{
    [Test]
    public async Task ShouldRequireAuthenticatedUser()
    {
        var command = new AdoptProgrammeCommand
        {
            ProgrammeTemplateId = "starting-strength",
            StartingWeights = new Dictionary<string, decimal>()
        };

        await Should.ThrowAsync<UnauthorizedAccessException>(() => TestApp.SendAsync(command));
    }

    [Test]
    public async Task ShouldCreateUserProgrammeWithStartingStrengthTemplate()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        var command = new AdoptProgrammeCommand
        {
            ProgrammeTemplateId = "starting-strength",
            StartingWeights = new Dictionary<string, decimal>
            {
                ["Squat"] = 60m,
                ["Bench Press"] = 40m,
                ["Deadlift"] = 80m
            }
        };

        var id = await TestApp.SendAsync(command);

        var programme = await TestApp.FindAsync<UserProgramme>(id);
        programme.ShouldNotBeNull();
        programme!.ProgrammeTemplateId.ShouldBe("starting-strength");
        programme.UserId.ShouldBe(userId);
        programme.Status.ShouldBe(ProgrammeStatus.Active);
    }

    [Test]
    public async Task ShouldSetWorkoutAAsFirst()
    {
        await TestApp.RunAsDefaultUserAsync();

        var command = new AdoptProgrammeCommand
        {
            ProgrammeTemplateId = "starting-strength",
            StartingWeights = new Dictionary<string, decimal>
            {
                ["Squat"] = 60m
            }
        };

        var id = await TestApp.SendAsync(command);

        var programme = await TestApp.FindAsync<UserProgramme>(id);
        programme.ShouldNotBeNull();
        programme!.CurrentWorkoutType.ShouldBe(WorkoutType.A);
    }
}
