using System.Text.RegularExpressions;

namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class LogWorkoutPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/log-workout";

    public Task AssertHeading() => Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Log Workout");

    public async Task SelectExerciseByName(string name)
    {
        var select = Page.Locator("select").First;
        var value = await select.Locator("option", new() { HasText = name }).First.GetAttributeAsync("value");
        await select.SelectOptionAsync(new SelectOptionValue { Value = value });
    }

    public Task ClickAddExercise() => Page.Locator("button:has-text('Add Exercise')").ClickAsync();

    public Task SetFirstSetWeight(string value) =>
        Page.Locator("article table input[type='number']").First.FillAsync(value);

    public Task MarkFirstSetCompleted() =>
        Page.Locator("article table input[type='checkbox']").First.CheckAsync();

    public Task ClickSaveAsDraft() => Page.Locator("button.outline:has-text('Save as Draft')").ClickAsync();

    public Task ClickCompleteWorkout() => Page.Locator("button:has-text('Complete Workout')").ClickAsync();

    public async Task<int> WaitForWorkoutDetailUrlAndGetId()
    {
        await Page.WaitForURLAsync(new Regex(@"/workout/\d+$"));
        var match = Regex.Match(Page.Url, @"/workout/(\d+)$");
        return int.Parse(match.Groups[1].Value);
    }
}
