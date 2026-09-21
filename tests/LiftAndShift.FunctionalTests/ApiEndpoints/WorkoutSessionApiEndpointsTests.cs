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

  private static object[] WorkoutALifts() =>
  [
    new { Lift = "Squat", WeightKg = 30m, RepsPerSet = new[] { 5, 5, 5 } },
    new { Lift = "Press", WeightKg = 25m, RepsPerSet = new[] { 5, 5, 5 } },
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

    var logResponse = await LogWorkoutSessionAsync(familyMemberId, "A", TestPerformedOn, WorkoutALifts());
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
      new { Lift = "BenchPress", WeightKg = 20m, RepsPerSet = new[] { 5, 5, 5 } },
      new { Lift = "Deadlift", WeightKg = 90m, RepsPerSet = new[] { 5 } }
    ];

    var phase1Response = await LogWorkoutSessionAsync(familyMemberId, "B", TestPerformedOn, phase1Lifts);
    phase1Response.StatusCode.ShouldBe(HttpStatusCode.Created);
    var phase1Session = await phase1Response.Content.ReadFromJsonAsync<WorkoutSessionRecord>(TestContext.Current.CancellationToken);
    phase1Session!.LoggedSets.ShouldContain(s => s.Lift == "Deadlift");

    await AdvanceTrainingPhaseAsync(familyMemberId);
    await RampAsync(familyMemberId, "Row");

    object[] phase2Lifts =
    [
      new { Lift = "Squat", WeightKg = 30m, RepsPerSet = new[] { 5, 5, 5 } },
      new { Lift = "BenchPress", WeightKg = 20m, RepsPerSet = new[] { 5, 5, 5 } },
      new { Lift = "Row", WeightKg = 30m, RepsPerSet = new[] { 5, 5, 5 } }
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

    var response = await LogWorkoutSessionAsync(familyMemberId, "A", TestPerformedOn, WorkoutALifts());

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
      new { Lift = "Press", WeightKg = 25m, RepsPerSet = new[] { 5, 5, 5 } },
      new { Lift = "Deadlift", WeightKg = 90m, RepsPerSet = new[] { 5 } }
    ];

    var response = await LogWorkoutSessionAsync(familyMemberId, "A", TestPerformedOn, wrongSetCountLifts);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task LoggingASessionForANonexistentFamilyMemberReturnsNotFound()
  {
    var response = await LogWorkoutSessionAsync(1_000_000, "A", TestPerformedOn, WorkoutALifts());

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

    await LogWorkoutSessionAsync(familyMemberId, "A", earliest, WorkoutALifts());
    await LogWorkoutSessionAsync(familyMemberId, "A", latest, WorkoutALifts());
    await LogWorkoutSessionAsync(familyMemberId, "A", middle, WorkoutALifts());

    var list = await _client.GetAndDeserializeAsync<WorkoutSessionListResponse>(ListWorkoutSessionsRequest.BuildRoute(familyMemberId));

    list.Items.Select(s => s.PerformedOn).ShouldBe([latest, middle, earliest]);
  }
}
