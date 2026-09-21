using System.Net.Http.Json;
using LiftAndShift.Infrastructure.Data;
using LiftAndShift.Web.FamilyMembers;
using LiftAndShift.Web.WorkoutSessions;
using LiftAndShift.Web.WorkWeights;

namespace LiftAndShift.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class WorkWeightProgressionApiEndpointsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();
  private static readonly DateOnly Day1 = new(2026, 9, 1);
  private static readonly DateOnly Day2 = new(2026, 9, 8);
  private static readonly DateOnly Day3 = new(2026, 9, 15);

  private async Task<int> CreateFamilyMemberAsync(string name, string pin)
  {
    var response = await _client.PostAsJsonAsync(CreateFamilyMemberRequest.Route, new { Name = name, Pin = pin });
    response.EnsureSuccessStatusCode();
    var created = await response.Content.ReadFromJsonAsync<CreateFamilyMemberResponse>();
    return created!.Id;
  }

  private async Task RampAsync(int familyMemberId, string lift, int finalSetNumber = 3)
  {
    var response = await _client.PostAsJsonAsync(CompleteRampRequest.BuildRoute(familyMemberId, lift), new { FinalSetNumber = finalSetNumber });
    response.EnsureSuccessStatusCode();
  }

  private async Task RampWorkoutALiftsAsync(int familyMemberId)
  {
    await RampAsync(familyMemberId, "Squat");
    await RampAsync(familyMemberId, "Press");
    await RampAsync(familyMemberId, "Deadlift");
  }

  private async Task<decimal> GetCurrentWorkWeightAsync(int familyMemberId, string lift)
  {
    var workWeight = await _client.GetAndDeserializeAsync<WorkWeightRecord>(GetWorkWeightRequest.BuildRoute(familyMemberId, lift));
    return workWeight.WeightKg;
  }

  // Press and Deadlift are always logged as a clean success here, so - like Squat once it succeeds -
  // their Work Weight climbs after every call. Fetch each fresh rather than assuming they're still at
  // their Ramped value once a test logs more than one session.
  private async Task<WorkoutSessionRecord> LogWorkoutASessionAsync(int familyMemberId, DateOnly performedOn, decimal squatWeightKg, int squatFinalRep)
  {
    var pressWeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Press");
    var deadliftWeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Deadlift");

    var response = await _client.PostAsJsonAsync(LogWorkoutSessionRequest.BuildRoute(familyMemberId), new
    {
      Workout = "A",
      PerformedOn = performedOn,
      LoggedLifts = new object[]
      {
        new { Lift = "Squat", WeightKg = squatWeightKg, RepsPerSet = new[] { 5, 5, squatFinalRep } },
        new { Lift = "Press", WeightKg = pressWeightKg, RepsPerSet = new[] { 5, 5, 5 } },
        new { Lift = "Deadlift", WeightKg = deadliftWeightKg, RepsPerSet = new[] { 5 } }
      }
    });
    response.EnsureSuccessStatusCode();
    return (await response.Content.ReadFromJsonAsync<WorkoutSessionRecord>(TestContext.Current.CancellationToken))!;
  }

  [Fact]
  public async Task SuccessfulSessionIncreasesWorkWeightByTheProgressionIncrement()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Omar", "1111");
    await RampWorkoutALiftsAsync(familyMemberId); // Squat opens at 30kg after Ramp

    var session = await LogWorkoutASessionAsync(familyMemberId, Day1, squatWeightKg: 30, squatFinalRep: 5);

    var squatOutcome = session.LiftOutcomes.Single(o => o.Lift == "Squat");
    squatOutcome.Successful.ShouldBeTrue();
    squatOutcome.Deloaded.ShouldBeFalse();
    squatOutcome.NewWeightKg.ShouldBe(35); // 30 + Squat's 5kg progression increment

    var workWeight = await _client.GetAndDeserializeAsync<WorkWeightRecord>(GetWorkWeightRequest.BuildRoute(familyMemberId, "Squat"));
    workWeight.WeightKg.ShouldBe(35);
    workWeight.ConsecutiveFailures.ShouldBe(0);
  }

  [Fact]
  public async Task FirstFailureRecordsTheStreakWithoutChangingWorkWeight()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Priya", "2222");
    await RampWorkoutALiftsAsync(familyMemberId);

    var session = await LogWorkoutASessionAsync(familyMemberId, Day1, squatWeightKg: 30, squatFinalRep: 3);

    var squatOutcome = session.LiftOutcomes.Single(o => o.Lift == "Squat");
    squatOutcome.Successful.ShouldBeFalse();
    squatOutcome.Deloaded.ShouldBeFalse();
    squatOutcome.NewWeightKg.ShouldBe(30);

    var workWeight = await _client.GetAndDeserializeAsync<WorkWeightRecord>(GetWorkWeightRequest.BuildRoute(familyMemberId, "Squat"));
    workWeight.WeightKg.ShouldBe(30);
    workWeight.ConsecutiveFailures.ShouldBe(1);
  }

  [Fact]
  public async Task SecondConsecutiveFailureDeloadsTheWorkWeight()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Quinn", "3333");
    await RampWorkoutALiftsAsync(familyMemberId);

    await LogWorkoutASessionAsync(familyMemberId, Day1, squatWeightKg: 30, squatFinalRep: 3); // 1st failure
    var second = await LogWorkoutASessionAsync(familyMemberId, Day2, squatWeightKg: 30, squatFinalRep: 3); // 2nd consecutive failure

    var squatOutcome = second.LiftOutcomes.Single(o => o.Lift == "Squat");
    squatOutcome.Deloaded.ShouldBeTrue();
    squatOutcome.NewWeightKg.ShouldBe(27); // floor(30 * 0.9)

    var workWeight = await _client.GetAndDeserializeAsync<WorkWeightRecord>(GetWorkWeightRequest.BuildRoute(familyMemberId, "Squat"));
    workWeight.WeightKg.ShouldBe(27);
    workWeight.ConsecutiveFailures.ShouldBe(0);
  }

  [Fact]
  public async Task ASuccessfulSessionBetweenTwoFailuresPreventsADeload()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Riko", "4444");
    await RampWorkoutALiftsAsync(familyMemberId);

    await LogWorkoutASessionAsync(familyMemberId, Day1, squatWeightKg: 30, squatFinalRep: 3); // 1st failure, streak = 1
    await LogWorkoutASessionAsync(familyMemberId, Day2, squatWeightKg: 30, squatFinalRep: 5); // success, streak resets, weight -> 35
    var third = await LogWorkoutASessionAsync(familyMemberId, Day3, squatWeightKg: 35, squatFinalRep: 3); // only the "first" failure again

    var squatOutcome = third.LiftOutcomes.Single(o => o.Lift == "Squat");
    squatOutcome.Deloaded.ShouldBeFalse();
    squatOutcome.NewWeightKg.ShouldBe(35); // unchanged, not deloaded

    var workWeight = await _client.GetAndDeserializeAsync<WorkWeightRecord>(GetWorkWeightRequest.BuildRoute(familyMemberId, "Squat"));
    workWeight.ConsecutiveFailures.ShouldBe(1);
  }

  [Fact]
  public async Task DeloadNeverDropsBelowTheLiftsOpeningWeight()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Sable", "5555");
    await RampAsync(familyMemberId, "Squat", finalSetNumber: 1); // opens exactly at its 20kg floor
    await RampAsync(familyMemberId, "Press");
    await RampAsync(familyMemberId, "Deadlift");

    await LogWorkoutASessionAsync(familyMemberId, Day1, squatWeightKg: 20, squatFinalRep: 3); // 1st failure
    var second = await LogWorkoutASessionAsync(familyMemberId, Day2, squatWeightKg: 20, squatFinalRep: 3); // 2nd consecutive failure

    var squatOutcome = second.LiftOutcomes.Single(o => o.Lift == "Squat");
    squatOutcome.Deloaded.ShouldBeTrue();
    squatOutcome.NewWeightKg.ShouldBe(20); // floored at the opening weight, not floor(20 * 0.9) = 18

    var workWeight = await _client.GetAndDeserializeAsync<WorkWeightRecord>(GetWorkWeightRequest.BuildRoute(familyMemberId, "Squat"));
    workWeight.WeightKg.ShouldBe(20);
  }
}
