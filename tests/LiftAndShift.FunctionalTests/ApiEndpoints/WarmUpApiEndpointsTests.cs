using System.Net.Http.Json;
using LiftAndShift.Infrastructure.Data;
using LiftAndShift.Web.FamilyMembers;
using LiftAndShift.Web.WorkoutSessions;
using LiftAndShift.Web.WorkWeights;

namespace LiftAndShift.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class WarmUpApiEndpointsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();
  private static readonly DateOnly TestPerformedOn = new(2026, 9, 21);

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

  private async Task<decimal> GetCurrentWorkWeightAsync(int familyMemberId, string lift)
  {
    var workWeight = await _client.GetAndDeserializeAsync<WorkWeightRecord>(GetWorkWeightRequest.BuildRoute(familyMemberId, lift));
    return workWeight.WeightKg;
  }

  private async Task LogSuccessfulWorkoutASessionAsync(int familyMemberId)
  {
    var squatWeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Squat");
    var pressWeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Press");
    var deadliftWeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Deadlift");

    var response = await _client.PostAsJsonAsync(LogWorkoutSessionRequest.BuildRoute(familyMemberId), new
    {
      Workout = "A",
      PerformedOn = TestPerformedOn,
      LoggedLifts = new object[]
      {
        new { Lift = "Squat", WeightKg = squatWeightKg, RepsPerSet = new[] { 5, 5, 5 } },
        new { Lift = "Press", WeightKg = pressWeightKg, RepsPerSet = new[] { 5, 5, 5 } },
        new { Lift = "Deadlift", WeightKg = deadliftWeightKg, RepsPerSet = new[] { 5 } }
      }
    });
    response.EnsureSuccessStatusCode();
  }

  [Fact]
  public async Task ReturnsTheCorrectLadderForARampedLift()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Sana", "1111");
    await RampAsync(familyMemberId, "Squat"); // 30kg

    var warmUpSets = await _client.GetAndDeserializeAsync<List<WarmUpSetRecord>>(GetWarmUpSetsRequest.BuildRoute(familyMemberId, "Squat"));

    warmUpSets.Count.ShouldBe(4);
    warmUpSets[0].ShouldBe(new WarmUpSetRecord(20, 5)); // opening weight
    warmUpSets[1].ShouldBe(new WarmUpSetRecord(20, 5)); // floor(30 * 0.4) = 12, floored up to 20
    warmUpSets[2].ShouldBe(new WarmUpSetRecord(20, 3)); // floor(30 * 0.6) = 18, floored up to 20
    warmUpSets[3].ShouldBe(new WarmUpSetRecord(24, 2)); // floor(30 * 0.8) = 24
  }

  [Fact]
  public async Task ReturnsNotFoundGivenLiftNotYetRamped()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Tomas", "2222");

    _ = await _client.GetAndEnsureNotFoundAsync(GetWarmUpSetsRequest.BuildRoute(familyMemberId, "Squat"));
  }

  [Fact]
  public async Task ReturnsNotFoundGivenNonexistentFamilyMember()
  {
    _ = await _client.GetAndEnsureNotFoundAsync(GetWarmUpSetsRequest.BuildRoute(1_000_000, "Squat"));
  }

  [Fact]
  public async Task ReflectsWorkWeightAfterProgression()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Uma", "3333");
    await RampAsync(familyMemberId, "Squat"); // 30kg
    await RampAsync(familyMemberId, "Press");
    await RampAsync(familyMemberId, "Deadlift");

    var beforeWarmUpSets = await _client.GetAndDeserializeAsync<List<WarmUpSetRecord>>(GetWarmUpSetsRequest.BuildRoute(familyMemberId, "Squat"));
    beforeWarmUpSets[3].WeightKg.ShouldBe(24); // floor(30 * 0.8)

    await LogSuccessfulWorkoutASessionAsync(familyMemberId); // Squat progresses from 30kg to 35kg

    var afterWarmUpSets = await _client.GetAndDeserializeAsync<List<WarmUpSetRecord>>(GetWarmUpSetsRequest.BuildRoute(familyMemberId, "Squat"));
    afterWarmUpSets[3].WeightKg.ShouldBe(28); // floor(35 * 0.8)
  }
}
