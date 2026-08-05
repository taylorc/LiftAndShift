namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class HistoryStepDefinitions(LogWorkoutPage logWorkoutPage, HistoryPage historyPage, WorkoutDetailPage workoutDetailPage)
{
    [BeforeFeature("History")]
    public static async Task BeforeHistoryFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new LogWorkoutPage(page));
        container.RegisterInstanceAs(new HistoryPage(page));
        container.RegisterInstanceAs(new WorkoutDetailPage(page));
    }

    [AfterFeature("History")]
    public static async Task AfterHistoryFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user has logged a draft workout for {string}")]
    public async Task GivenAnAuthenticatedUserHasLoggedADraftWorkoutFor(string exerciseName)
    {
        await logWorkoutPage.GotoAsync();
        await logWorkoutPage.SelectExerciseByName(exerciseName);
        await logWorkoutPage.ClickAddExercise();
        await logWorkoutPage.SetFirstSetWeight("60");
        await logWorkoutPage.ClickSaveAsDraft();
        await logWorkoutPage.WaitForWorkoutDetailUrlAndGetId();
    }

    [When("they visit the history page")]
    public Task WhenTheyVisitTheHistoryPage() => historyPage.GotoAsync();

    [Then("the {string} session is visible with status {string}")]
    public async Task ThenTheSessionIsVisibleWithStatus(string exerciseName, string status)
    {
        await historyPage.AssertSessionVisible(exerciseName);
        await historyPage.AssertStatusTag(exerciseName, status);
    }

    [When("they view the {string} session")]
    public Task WhenTheyViewTheSession(string exerciseName) => historyPage.ClickViewFor(exerciseName);

    [Then("they are taken to its workout detail page")]
    public Task ThenTheyAreTakenToItsWorkoutDetailPage() => workoutDetailPage.AssertOnWorkoutDetailPage();
}
