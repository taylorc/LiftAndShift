using LiftAndShift.Web.Lifts;

namespace LiftAndShift.FunctionalTests.ApiEndpoints;

[Collection("Sequential")]
public class LiftsApiEndpointsTests(CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task ListsAllSixLiftsWithTheirOpeningWeightAndIncrement()
  {
    var lifts = await _client.GetAndDeserializeAsync<List<LiftRecord>>("/Lifts");

    lifts.Count.ShouldBe(6);

    var deadlift = lifts.Single(l => l.Name == "Deadlift");
    deadlift.OpeningWeightKg.ShouldBe(70);
    deadlift.IncrementKg.ShouldBe(10);

    var squat = lifts.Single(l => l.Name == "Squat");
    squat.OpeningWeightKg.ShouldBe(20);
    squat.IncrementKg.ShouldBe(5);
  }
}
