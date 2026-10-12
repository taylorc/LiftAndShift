using System.Net;
using System.Net.Http.Json;
using LiftAndShift.Web.BodyMetrics;
using LiftAndShift.Web.FamilyMembers;

namespace LiftAndShift.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class BodyMetricApiEndpointsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();
  private static readonly DateOnly TestRecordedOn = new(2026, 10, 12);

  private async Task<int> CreateFamilyMemberAsync(string name, string pin)
  {
    var response = await _client.PostAsJsonAsync(CreateFamilyMemberRequest.Route, new { Name = name, Pin = pin });
    response.EnsureSuccessStatusCode();
    var created = await response.Content.ReadFromJsonAsync<CreateFamilyMemberResponse>();
    return created!.Id;
  }

  private async Task<HttpResponseMessage> LogBodyMetricAsync(int familyMemberId, DateOnly recordedOn, decimal bodyWeightKg) =>
    await _client.PostAsJsonAsync(LogBodyMetricRequest.BuildRoute(familyMemberId), new { RecordedOn = recordedOn, BodyWeightKg = bodyWeightKg });

  [Fact]
  public async Task LoggingABodyweightEntrySucceedsAndRoundTripsThroughGet()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Olive", "7111");

    var logResponse = await LogBodyMetricAsync(familyMemberId, TestRecordedOn, 82.5m);
    logResponse.StatusCode.ShouldBe(HttpStatusCode.Created);
    var logged = await logResponse.Content.ReadFromJsonAsync<BodyMetricRecord>(TestContext.Current.CancellationToken);

    logged!.FamilyMemberId.ShouldBe(familyMemberId);
    logged.RecordedOn.ShouldBe(TestRecordedOn);
    logged.BodyWeightKg.ShouldBe(82.5m);

    var fetched = await _client.GetAndDeserializeAsync<BodyMetricRecord>(
      GetBodyMetricByIdRequest.BuildRoute(familyMemberId, logged.Id));

    fetched.ShouldBe(logged);
  }

  [Fact]
  public async Task LoggingAnEntryForANonexistentFamilyMemberReturnsNotFound()
  {
    var response = await LogBodyMetricAsync(1_000_000, TestRecordedOn, 82.5m);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public async Task LoggingANonPositiveWeightReturnsBadRequest(decimal bodyWeightKg)
  {
    int familyMemberId = await CreateFamilyMemberAsync($"Pip{bodyWeightKg}", "7222");

    var response = await LogBodyMetricAsync(familyMemberId, TestRecordedOn, bodyWeightKg);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }

  [Fact]
  public async Task GetBodyMetricReturnsNotFoundGivenNonexistentEntry()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Quinn", "7333");

    _ = await _client.GetAndEnsureNotFoundAsync(GetBodyMetricByIdRequest.BuildRoute(familyMemberId, 1_000_000));
  }

  [Fact]
  public async Task GetBodyMetricReturnsNotFoundUnderADifferentFamilyMember()
  {
    int ownerId = await CreateFamilyMemberAsync("Rosa", "7444");
    int otherId = await CreateFamilyMemberAsync("Sami", "7555");

    var logResponse = await LogBodyMetricAsync(ownerId, TestRecordedOn, 70m);
    var logged = await logResponse.Content.ReadFromJsonAsync<BodyMetricRecord>(TestContext.Current.CancellationToken);

    _ = await _client.GetAndEnsureNotFoundAsync(GetBodyMetricByIdRequest.BuildRoute(otherId, logged!.Id));
  }

  [Fact]
  public async Task ListingEntriesReturnsThemMostRecentFirst()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Theo", "7666");

    var earliest = new DateOnly(2026, 9, 1);
    var middle = new DateOnly(2026, 9, 10);
    var latest = new DateOnly(2026, 9, 20);

    await LogBodyMetricAsync(familyMemberId, earliest, 80m);
    await LogBodyMetricAsync(familyMemberId, latest, 82m);
    await LogBodyMetricAsync(familyMemberId, middle, 81m);

    var list = await _client.GetAndDeserializeAsync<BodyMetricListResponse>(ListBodyMetricsRequest.BuildRoute(familyMemberId));

    list.Items.Select(m => m.RecordedOn).ShouldBe([latest, middle, earliest]);
  }
}
