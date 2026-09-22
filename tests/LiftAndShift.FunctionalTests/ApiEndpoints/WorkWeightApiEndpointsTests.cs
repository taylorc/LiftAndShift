using System.Net;
using System.Net.Http.Json;
using LiftAndShift.Infrastructure.Data;
using LiftAndShift.Web.FamilyMembers;
using LiftAndShift.Web.WorkWeights;
using Microsoft.AspNetCore.Mvc;

namespace LiftAndShift.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class WorkWeightApiEndpointsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  private async Task<int> CreateFamilyMemberAsync(string name, string pin)
  {
    var response = await _client.PostAsJsonAsync(CreateFamilyMemberRequest.Route, new { Name = name, Pin = pin });
    response.EnsureSuccessStatusCode();
    var created = await response.Content.ReadFromJsonAsync<CreateFamilyMemberResponse>();
    return created!.Id;
  }

  private async Task<HttpResponseMessage> CompleteRampAsync(int familyMemberId, string lift, int finalSetNumber) =>
    await _client.PostAsJsonAsync(CompleteRampRequest.BuildRoute(familyMemberId, lift), new { FinalSetNumber = finalSetNumber });

  [Fact]
  public async Task CompletingRampForSquatComputesCorrectWorkWeight()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Dana", "1111");

    var response = await CompleteRampAsync(familyMemberId, "Squat", finalSetNumber: 3);
    response.EnsureSuccessStatusCode();
    var workWeight = await response.Content.ReadFromJsonAsync<WorkWeightRecord>(TestContext.Current.CancellationToken);

    workWeight!.FamilyMemberId.ShouldBe(familyMemberId);
    workWeight.Lift.ShouldBe("Squat");
    workWeight.WeightKg.ShouldBe(30);
  }

  [Fact]
  public async Task CompletingRampForDeadliftUsesItsOwnOpeningWeightAndIncrement()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Ezra", "2222");

    var response = await CompleteRampAsync(familyMemberId, "Deadlift", finalSetNumber: 3);
    response.EnsureSuccessStatusCode();
    var workWeight = await response.Content.ReadFromJsonAsync<WorkWeightRecord>(TestContext.Current.CancellationToken);

    workWeight!.WeightKg.ShouldBe(90);
  }

  [Fact]
  public async Task GetWorkWeightReturnsTheRampedValue()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Fay", "3333");
    await CompleteRampAsync(familyMemberId, "Press", finalSetNumber: 2);

    var workWeight = await _client.GetAndDeserializeAsync<WorkWeightRecord>(
      GetWorkWeightRequest.BuildRoute(familyMemberId, "Press"));

    workWeight.WeightKg.ShouldBe(25);
  }

  [Fact]
  public async Task GetWorkWeightReturnsNotFoundGivenLiftNotYetRamped()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Gus", "4444");

    _ = await _client.GetAndEnsureNotFoundAsync(GetWorkWeightRequest.BuildRoute(familyMemberId, "Squat"));
  }

  [Fact]
  public async Task GetWorkWeightReturnsNotFoundGivenNonexistentFamilyMember()
  {
    _ = await _client.GetAndEnsureNotFoundAsync(GetWorkWeightRequest.BuildRoute(1_000_000, "Squat"));
  }

  [Fact]
  public async Task CompletingRampTwiceForSameLiftReturnsBadRequestWithAnExplanation()
  {
    int familyMemberId = await CreateFamilyMemberAsync("Hana", "5555");
    await CompleteRampAsync(familyMemberId, "Squat", finalSetNumber: 3);

    var response = await CompleteRampAsync(familyMemberId, "Squat", finalSetNumber: 5);

    response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(TestContext.Current.CancellationToken);
    problem!.Detail.ShouldNotBeNullOrWhiteSpace();
    problem.Detail.ShouldContain("already been Ramped");
  }

  [Fact]
  public async Task CompletingRampReturnsNotFoundGivenNonexistentFamilyMember()
  {
    var response = await CompleteRampAsync(1_000_000, "Squat", finalSetNumber: 3);

    response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
  }
}
