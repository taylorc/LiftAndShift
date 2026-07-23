using LiftAndShift.Application.Workouts.Queries.GetWorkoutHistory;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.FunctionalTests.Workouts.Queries;

public class GetWorkoutHistoryTests : TestBase
{
    [Test]
    public async Task ShouldRequireAuthenticatedUser()
    {
        await Should.ThrowAsync<UnauthorizedAccessException>(
            () => TestApp.SendAsync(new GetWorkoutHistoryQuery()));
    }

    [Test]
    public async Task ShouldReturnOnlyCurrentUsersWorkouts()
    {
        var userId = await TestApp.RunAsDefaultUserAsync();

        // Add a workout for the current user
        var session = new WorkoutSession
        {
            UserId = userId,
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Completed
        };
        await TestApp.AddAsync(session);

        // Add a workout for another user
        var otherSession = new WorkoutSession
        {
            UserId = "other-user-id",
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Completed
        };
        await TestApp.AddAsync(otherSession);

        var result = await TestApp.SendAsync(new GetWorkoutHistoryQuery());

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(session.Id);
    }

    [Test]
    public async Task ShouldReturnEmptyList_WhenNoWorkouts()
    {
        await TestApp.RunAsDefaultUserAsync();

        var result = await TestApp.SendAsync(new GetWorkoutHistoryQuery());

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
}
