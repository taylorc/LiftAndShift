namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class ExerciseLibraryPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/exercise-library";

    public Task AssertHeading() => Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Exercise Library");

    public Task Search(string term) => Page.FillAsync("input[type='search']", term);

    private ILocator ExerciseNameCell(string name) =>
        Page.Locator("tbody tr td:first-child strong", new() { HasText = name });

    private ILocator ExerciseRow(string name) =>
        Page.Locator("tbody tr").Filter(new() { Has = Page.Locator("td:first-child strong", new() { HasText = name }) });

    public Task AssertExerciseRowVisible(string name) =>
        Assertions.Expect(ExerciseNameCell(name)).ToBeVisibleAsync();

    public Task AssertExerciseRowHidden(string name) =>
        Assertions.Expect(ExerciseNameCell(name)).ToHaveCountAsync(0);

    public Task OpenAddCustomExercise() => Page.Locator("summary:has-text('Add Custom Exercise')").ClickAsync();

    public Task SetCustomExerciseName(string name) => Page.GetByLabel("Name *").FillAsync(name);

    public Task ClickAddExercise() => Page.Locator("button:has-text('Add Exercise')").ClickAsync();

    public Task ClickDeleteFor(string name) =>
        ExerciseRow(name).Locator("button:has-text('Delete')").ClickAsync();
}
