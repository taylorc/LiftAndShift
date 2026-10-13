using System.Net.Http.Json;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.Workouts;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.Core.WorkWeightAggregate;
using LiftAndShift.Infrastructure.Data;
using LiftAndShift.Web.FamilyMembers;
using LiftAndShift.Web.PersonalRecords;
using LiftAndShift.Web.WorkoutSessions;
using LiftAndShift.Web.WorkWeights;

namespace LiftAndShift.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class PersonalRecordApiEndpointsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();
  private readonly CustomWebApplicationFactory<Program> _factory = factory;
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

  // Press and Deadlift always succeed here so only Squat's outcome varies between calls; their current
  // Work Weight is fetched fresh each time since a prior successful call already moved it.
  private async Task<WorkoutSessionRecord> LogWorkoutASessionAsync(int familyMemberId, DateOnly performedOn, int squatFinalRep)
  {
    var squatWeightKg = await GetCurrentWorkWeightAsync(familyMemberId, "Squat");
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

  private Task<List<PersonalRecordRecord>> GetPersonalRecordsAsync(int familyMemberId) =>
    _client.GetAndDeserializeAsync<List<PersonalRecordRecord>>(GetPersonalRecordsRequest.BuildRoute(familyMemberId));

  [Fact]
  public async Task ASuccessfulSessionCreatesAPersonalRecord()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Vik", "1111");
    await RampWorkoutALiftsAsync(familyMemberId); // Squat opens at 30kg

    var session = await LogWorkoutASessionAsync(familyMemberId, Day1, squatFinalRep: 5);

    var records = await GetPersonalRecordsAsync(familyMemberId);
    var squatRecord = records.Single(r => r.Lift == "Squat");
    squatRecord.WeightKg.ShouldBe(30);
    squatRecord.AchievedOn.ShouldBe(Day1);
    squatRecord.WorkoutSessionId.ShouldBe(session.Id);
  }

  [Fact]
  public async Task AHeavierSuccessfulSessionUpdatesTheRecord()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Wren", "2222");
    await RampWorkoutALiftsAsync(familyMemberId); // Squat opens at 30kg

    // A session's record weight is what was actually lifted *at* that session (the Work Weight
    // going in), not the Work Weight the session's own progression produces afterward.
    await LogWorkoutASessionAsync(familyMemberId, Day1, squatFinalRep: 5); // logged at 30kg, succeeds -> Squat becomes 35kg
    await LogWorkoutASessionAsync(familyMemberId, Day2, squatFinalRep: 5); // logged at 35kg, succeeds -> Squat becomes 40kg

    var records = await GetPersonalRecordsAsync(familyMemberId);
    records.Single(r => r.Lift == "Squat").WeightKg.ShouldBe(35); // the heaviest weight an actual session was logged at
  }

  [Fact]
  public async Task ALaterDeloadDoesNotLowerTheRecord()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Xena", "3333");
    await RampWorkoutALiftsAsync(familyMemberId); // Squat opens at 30kg

    await LogWorkoutASessionAsync(familyMemberId, Day1, squatFinalRep: 5); // logged at 30kg, succeeds -> Squat becomes 35kg; PR = 30kg
    await LogWorkoutASessionAsync(familyMemberId, Day2, squatFinalRep: 3); // logged at 35kg, 1st failure, weight unchanged
    await LogWorkoutASessionAsync(familyMemberId, Day3, squatFinalRep: 3); // logged at 35kg, 2nd consecutive failure -> deloads to 31kg

    var currentWorkWeight = await GetCurrentWorkWeightAsync(familyMemberId, "Squat");
    currentWorkWeight.ShouldBe(31); // confirms the deload actually happened

    var records = await GetPersonalRecordsAsync(familyMemberId);
    records.Single(r => r.Lift == "Squat").WeightKg.ShouldBe(30); // the record set on Day1 still stands, unaffected by the deload
  }

  [Fact]
  public async Task AHeavierFailedSessionDoesNotBecomeTheRecord()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Abe", "6666");
    await RampWorkoutALiftsAsync(familyMemberId); // Squat opens at 30kg

    await LogWorkoutASessionAsync(familyMemberId, Day1, squatFinalRep: 5); // logged at 30kg, succeeds -> Squat becomes 35kg
    await LogWorkoutASessionAsync(familyMemberId, Day2, squatFinalRep: 3); // logged at 35kg, fails

    var squatRecord = (await GetPersonalRecordsAsync(familyMemberId)).Single(r => r.Lift == "Squat");
    squatRecord.WeightKg.ShouldBe(30); // the heavier 35kg session missed a set, so it isn't a record
    squatRecord.AchievedOn.ShouldBe(Day1);
  }

  [Fact]
  public async Task ATieAtTheRecordWeightKeepsTheEarliestDate()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Bea", "7777");

    // Two successful sessions at the same Squat weight can't be produced through the API (a success
    // always progresses Work Weight, and a session must be logged at the current one), so they're
    // seeded directly. The later date is inserted first so insertion order can't decide the tie.
    await SeedSuccessfulWorkoutASessionAsync(familyMemberId, Day3, squatWeightKg: 50);
    await SeedSuccessfulWorkoutASessionAsync(familyMemberId, Day1, squatWeightKg: 50);

    var squatRecord = (await GetPersonalRecordsAsync(familyMemberId)).Single(r => r.Lift == "Squat");
    squatRecord.WeightKg.ShouldBe(50);
    squatRecord.AchievedOn.ShouldBe(Day1);
  }

  private async Task SeedSuccessfulWorkoutASessionAsync(int familyMemberId, DateOnly performedOn, decimal squatWeightKg)
  {
    using var scope = _factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    db.WorkoutSessions.Add(WorkoutSession.Log(
      FamilyMemberId.From(familyMemberId), Workout.A, TrainingPhase.From(1), performedOn,
      [
        new LoggedLiftInput(Lift.Squat, WeightKg.From(squatWeightKg), [5, 5, 5]),
        new LoggedLiftInput(Lift.Press, WeightKg.From(30), [5, 5, 5]),
        new LoggedLiftInput(Lift.Deadlift, WeightKg.From(90), [5])
      ]));
    await db.SaveChangesAsync(TestContext.Current.CancellationToken);
  }

  [Fact]
  public async Task ALiftWithOnlyFailedSessionsIsOmitted()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Yara", "4444");
    await RampWorkoutALiftsAsync(familyMemberId);

    await LogWorkoutASessionAsync(familyMemberId, Day1, squatFinalRep: 3); // Squat fails; Press/Deadlift succeed

    var records = await GetPersonalRecordsAsync(familyMemberId);
    records.ShouldNotContain(r => r.Lift == "Squat");
    records.ShouldContain(r => r.Lift == "Press");
    records.ShouldContain(r => r.Lift == "Deadlift");
  }

  [Fact]
  public async Task ReturnsEmptyListForAFamilyMemberWithNoSessions()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Zane", "5555");

    var records = await GetPersonalRecordsAsync(familyMemberId);

    records.ShouldBeEmpty();
  }

  [Fact]
  public async Task ReturnsNotFoundGivenNonexistentFamilyMember()
  {
    _ = await _client.GetAndEnsureNotFoundAsync(GetPersonalRecordsRequest.BuildRoute(1_000_000));
  }
}
