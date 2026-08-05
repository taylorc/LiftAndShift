using System.Text.RegularExpressions;

namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class WorkoutDetailPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/workout";

    public Task GotoWorkout(int id) => Page.GotoAsync($"{BaseUrl}/workout/{id}");

    public Task AssertOnWorkoutDetailPage() => Assertions.Expect(Page).ToHaveURLAsync(new Regex(@"/workout/\d+$"));

    public Task AssertExerciseVisible(string name) =>
        Assertions.Expect(Page.Locator("article header strong", new() { HasText = name })).ToBeVisibleAsync();

    public Task AssertStatus(string status) => Assertions.Expect(Page.Locator("hgroup p")).ToContainTextAsync(status);

    public Task ClickMarkComplete() => Page.Locator("button:has-text('Mark Complete')").ClickAsync();

    public Task ClickDuplicate() => Page.Locator("button:has-text('Duplicate as New Draft')").ClickAsync();

    public Task AssertMarkCompleteButtonHidden() =>
        Assertions.Expect(Page.Locator("button:has-text('Mark Complete')")).ToHaveCountAsync(0);
}
