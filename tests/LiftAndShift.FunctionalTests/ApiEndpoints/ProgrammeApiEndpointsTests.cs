using System.Net;
using System.Net.Http.Json;
using LiftAndShift.Infrastructure.Data;
using LiftAndShift.Web.FamilyMembers;
using LiftAndShift.Web.Programmes;

namespace LiftAndShift.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class ProgrammeApiEndpointsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  private async Task<int> CreateFamilyMemberAsync(string name = "Ada", string pin = "1234")
  {
    var response = await _client.PostAsJsonAsync(CreateFamilyMemberRequest.Route, new { Name = name, Pin = pin });
    response.EnsureSuccessStatusCode();
    var created = await response.Content.ReadFromJsonAsync<CreateFamilyMemberResponse>();
    return created!.Id;
  }

  [Fact]
  public async Task CreatingFamilyMemberCreatesProgrammeAtTrainingPhase1()
  {
    int familyMemberId = await CreateFamilyMemberAsync();

    var programme = await _client.GetAndDeserializeAsync<ProgrammeRecord>(
      GetProgrammeByFamilyMemberIdRequest.BuildRoute(familyMemberId));

    programme.FamilyMemberId.ShouldBe(familyMemberId);
    programme.TrainingPhase.ShouldBe(1);
  }

  [Fact]
  public async Task GetProgrammeReturnsNotFoundGivenNonexistentFamilyMember()
  {
    _ = await _client.GetAndEnsureNotFoundAsync(GetProgrammeByFamilyMemberIdRequest.BuildRoute(1_000_000));
  }

  private async Task<ProgrammeRecord> AdvancePhaseAsync(int familyMemberId)
  {
    var response = await _client.PostAsync(AdvanceTrainingPhaseRequest.BuildRoute(familyMemberId), content: null);
    response.EnsureSuccessStatusCode();
    return (await response.Content.ReadFromJsonAsync<ProgrammeRecord>())!;
  }

  [Fact]
  public async Task AdvancingPhaseMovesFromPhase1To2To3()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Bob", "2345");

    var afterFirstAdvance = await AdvancePhaseAsync(familyMemberId);
    afterFirstAdvance.TrainingPhase.ShouldBe(2);

    var afterSecondAdvance = await AdvancePhaseAsync(familyMemberId);
    afterSecondAdvance.TrainingPhase.ShouldBe(3);
  }

  [Fact]
  public async Task AdvancingPhasePastTrainingPhase3ReturnsBadRequest()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Cara", "3456");

    await AdvancePhaseAsync(familyMemberId);
    await AdvancePhaseAsync(familyMemberId);

    var response = await _client.PostAsync(AdvanceTrainingPhaseRequest.BuildRoute(familyMemberId), content: null);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
  }
}
