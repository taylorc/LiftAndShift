using System.Net;
using System.Net.Http.Json;
using LiftAndShift.Infrastructure.Data;
using LiftAndShift.Web.FamilyMembers;
using LiftAndShift.Web.Programmes;
using LiftAndShift.Web.WorkoutSessions;
using LiftAndShift.Web.WorkWeights;

namespace LiftAndShift.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class WorkoutSessionApiEndpointsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
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

  private async Task AdvanceTrainingPhaseAsync(int familyMemberId)
  {
    var response = await _client.PostAsync(AdvanceTrainingPhaseRequest.BuildRoute(familyMemberId), content: null);
    response.EnsureSuccessStatusCode();
  }

  private async Task<HttpResponseMessage> LogWorkoutSessionAsync(int familyMemberId, string workout, DateOnly performedOn, object loggedLifts) =>
    await _client.PostAsJsonAsync(LogWorkoutSessionRequest.BuildRoute(familyMemberId), new { Workout = workout, PerformedOn = performedOn, LoggedLifts = loggedLifts });

  // A session can only be logged at a lift's *current* Work Weight (see WorkWeightProgressionApiEndpointsTests),
  // so this fetches it fresh rather than hardcoding a value that drifts as soon as a session progresses it.
  private async Task<decimal> GetCurrentWorkWeightAsync(int familyMemberId, string lift)
  {
    var workWeight = await _client.GetAndDeserializeAsync<WorkWeightRecord>(GetWorkWeightRequest.BuildRoute(familyMemberId, lift));
    return workWeight.WeightKg;
  }

  private async Task<object[]> WorkoutALiftsAsync(int familyMemberId) =>
  [
    new { Lift = "Squat", WeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Squat"), RepsPerSet = new[] { 5, 5, 5 } },
    new { Lift = "Press", WeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Press"), RepsPerSet = new[] { 5, 5, 5 } },
    new { Lift = "Deadlift", WeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Deadlift"), RepsPerSet = new[] { 5 } }
  ];

  // Fixed weights for tests where the request is rejected before the weight-match check ever runs
  // (an un-Ramped lift or a nonexistent family member) - the actual values here don't matter.
  private static object[] ArbitraryWorkoutALifts() =>
  [
    new { Lift = "Squat", WeightKg = 30m, RepsPerSet = new[] { 5, 5, 5 } },
    new { Lift = "Press", WeightKg = 30m, RepsPerSet = new[] { 5, 5, 5 } },
    new { Lift = "Deadlift", WeightKg = 90m, RepsPerSet = new[] { 5 } }
  ];

  private async Task RampWorkoutALiftsAsync(int familyMemberId)
  {
    await RampAsync(familyMemberId, "Squat");
    await RampAsync(familyMemberId, "Press");
    await RampAsync(familyMemberId, "Deadlift");
  }

  [Fact]
  public async Task LoggingAValidWorkoutASessionSucceedsAndRoundTripsThroughGet()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Ivy", "1111");
    await RampWorkoutALiftsAsync(familyMemberId);

    var logResponse = await LogWorkoutSessionAsync(familyMemberId, "A", TestPerformedOn, await WorkoutALiftsAsync(familyMemberId));
    logResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
    var logged = await logResponse.Content.ReadFromJsonAsync<WorkoutSessionRecord>(TestContext.Current.CancellationToken);

    logged!.FamilyMemberId.ShouldBe(familyMemberId);
    logged.Workout.ShouldBe("A");
    logged.TrainingPhase.ShouldBe(1);
    logged.PerformedOn.ShouldBe(TestPerformedOn);
    logged.LoggedSets.Count.ShouldBe(7);

    var fetched = await _client.GetAndDeserializeAsync<WorkoutSessionRecord>(
      GetWorkoutSessionByIdRequest.BuildRoute(familyMemberId, logged.Id));

    fetched.LoggedSets.Count.ShouldBe(7);
  }

  [Fact]
  public async Task LoggingWorkoutBUsesTheThirdLiftForTheCurrentTrainingPhase()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Jay", "2222");
    await RampAsync(familyMemberId, "Squat");
    await RampAsync(familyMemberId, "BenchPress");
    await RampAsync(familyMemberId, "Deadlift");

    object[] phase1Lifts =
    [
      new { Lift = "Squat", WeightKg = 30m, RepsPerSet = new[] { 5, 5, 5 } },
      new { Lift = "BenchPress", WeightKg = 30m, RepsPerSet = new[] { 5, 5, 5 } },
      new { Lift = "Deadlift", WeightKg = 90m, RepsPerSet = new[] { 5 } }
    ];

    var phase1Response = await LogWorkoutSessionAsync(familyMemberId, "B", TestPerformedOn, phase1Lifts);
    phase1Response.StatusCode.ShouldBe(HttpStatusCode.Created);
    var phase1Session = await phase1Response.Content.ReadFromJsonAsync<WorkoutSessionRecord>(TestContext.Current.CancellationToken);
    phase1Session!.LoggedSets.ShouldContain(s => s.Lift == "Deadlift");

    await AdvanceTrainingPhaseAsync(familyMemberId);
    await RampAsync(familyMemberId, "Row");

    // Squat and BenchPress both progressed after the successful phase1 session, so their Work Weight
    // is no longer the 30kg they Ramped to - fetch current values rather than assuming they're unchanged.
    object[] phase2Lifts =
    [
      new { Lift = "Squat", WeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Squat"), RepsPerSet = new[] { 5, 5, 5 } },
      new { Lift = "BenchPress", WeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "BenchPress"), RepsPerSet = new[] { 5, 5, 5 } },
      new { Lift = "Row", WeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Row"), RepsPerSet = new[] { 5, 5, 5 } }
    ];

    var phase2Response = await LogWorkoutSessionAsync(familyMemberId, "B", TestPerformedOn, phase2Lifts);
    phase2Response.StatusCode.ShouldBe(HttpStatusCode.Created);
    var phase2Session = await phase2Response.Content.ReadFromJsonAsync<WorkoutSessionRecord>(TestContext.Current.CancellationToken);
    phase2Session!.TrainingPhase.ShouldBe(2);
    phase2Session.LoggedSets.ShouldContain(s => s.Lift == "Row");
    phase2Session.LoggedSets.ShouldNotContain(s => s.Lift == "Deadlift");
  }

  [Fact]
  public async Task LoggingASessionWithAnUnRampedLiftReturnsBadRequest()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Kai", "3333");
    await RampAsync(familyMemberId, "Squat");
    await RampAsync(familyMemberId, "Press");
    // Deadlift intentionally not Ramped.

    var response = await LogWorkoutSessionAsync(familyMemberId, "A", TestPerformedOn, ArbitraryWorkoutALifts());

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task LoggingASessionWithTheWrongNumberOfSetsReturnsBadRequest()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Lena", "4444");
    await RampWorkoutALiftsAsync(familyMemberId);

    object[] wrongSetCountLifts =
    [
      new { Lift = "Squat", WeightKg = 30m, RepsPerSet = new[] { 5, 5 } }, // only 2 sets; Squat needs 3
      new { Lift = "Press", WeightKg = 30m, RepsPerSet = new[] { 5, 5, 5 } },
      new { Lift = "Deadlift", WeightKg = 90m, RepsPerSet = new[] { 5 } }
    ];

    var response = await LogWorkoutSessionAsync(familyMemberId, "A", TestPerformedOn, wrongSetCountLifts);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task LoggingASessionForANonexistentFamilyMemberReturnsNotFound()
  {
    var response = await LogWorkoutSessionAsync(1_000_000, "A", TestPerformedOn, ArbitraryWorkoutALifts());

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task GetWorkoutSessionReturnsNotFoundGivenNonexistentSession()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Milo", "5555");

    _ = await _client.GetAndEnsureNotFoundAsync(GetWorkoutSessionByIdRequest.BuildRoute(familyMemberId, 1_000_000));
  }

  [Fact]
  public async Task ListingSessionsReturnsThemMostRecentFirst()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Nora", "6666");
    await RampWorkoutALiftsAsync(familyMemberId);

    var earliest = new DateOnly(2026, 9, 1);
    var middle = new DateOnly(2026, 9, 10);
    var latest = new DateOnly(2026, 9, 20);

    // Each session progresses Work Weight, so the lifts (and their weights) must be re-fetched before
    // every log call rather than reused - logging at a stale weight is now rejected (see WorkWeightProgressionApiEndpointsTests).
    await LogWorkoutSessionAsync(familyMemberId, "A", earliest, await WorkoutALiftsAsync(familyMemberId));
    await LogWorkoutSessionAsync(familyMemberId, "A", latest, await WorkoutALiftsAsync(familyMemberId));
    await LogWorkoutSessionAsync(familyMemberId, "A", middle, await WorkoutALiftsAsync(familyMemberId));

    var list = await _client.GetAndDeserializeAsync<WorkoutSessionListResponse>(ListWorkoutSessionsRequest.BuildRoute(familyMemberId));

    list.Items.Select(s => s.PerformedOn).ShouldBe([latest, middle, earliest]);
  }
}
