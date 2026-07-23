namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class ExerciseLibraryStepDefinitions(ExerciseLibraryPage exerciseLibraryPage)
{
    private string _customExerciseName = string.Empty;

    [BeforeFeature("ExerciseLibrary")]
    public static async Task BeforeExerciseLibraryFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new ExerciseLibraryPage(page));
    }

    [AfterFeature("ExerciseLibrary")]
    public static async Task AfterExerciseLibraryFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user visits the exercise library page")]
    public Task GivenAnAuthenticatedUserVisitsTheExerciseLibraryPage() => exerciseLibraryPage.GotoAsync();

    [When("they search for {string}")]
    public Task TheySearchFor(string term) => exerciseLibraryPage.Search(term);

    [Then("the {string} exercise is visible in the list")]
    public Task TheExerciseIsVisibleInTheList(string name) => exerciseLibraryPage.AssertExerciseRowVisible(name);

    [When("they add a custom exercise")]
    public async Task TheyAddACustomExercise()
    {
        _customExerciseName = $"Custom Exercise {Guid.NewGuid():N}";
        await exerciseLibraryPage.OpenAddCustomExercise();
        await exerciseLibraryPage.SetCustomExerciseName(_customExerciseName);
        await exerciseLibraryPage.ClickAddExercise();
    }

    [Then("the custom exercise is visible in the list")]
    public Task TheCustomExerciseIsVisibleInTheList() => exerciseLibraryPage.AssertExerciseRowVisible(_customExerciseName);

    [When("they delete the custom exercise")]
    public Task TheyDeleteTheCustomExercise() => exerciseLibraryPage.ClickDeleteFor(_customExerciseName);

    [Then("the custom exercise is no longer visible in the list")]
    public Task TheCustomExerciseIsNoLongerVisibleInTheList() => exerciseLibraryPage.AssertExerciseRowHidden(_customExerciseName);
}
