namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class ProgressStepDefinitions(LogWorkoutPage logWorkoutPage, ProgressPage progressPage)
{
    [BeforeFeature("Progress")]
    public static async Task BeforeProgressFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new LogWorkoutPage(page));
        container.RegisterInstanceAs(new ProgressPage(page));
    }

    [AfterFeature("Progress")]
    public static async Task AfterProgressFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user has completed a workout for {string} with a working set")]
    public async Task GivenAnAuthenticatedUserHasCompletedAWorkoutForWithAWorkingSet(string exerciseName)
    {
        await logWorkoutPage.GotoAsync();
        await logWorkoutPage.SelectExerciseByName(exerciseName);
        await logWorkoutPage.ClickAddExercise();
        await logWorkoutPage.SetFirstSetWeight("60");
        await logWorkoutPage.MarkFirstSetCompleted();
        await logWorkoutPage.ClickCompleteWorkout();
        await logWorkoutPage.WaitForWorkoutDetailUrlAndGetId();
    }

    [When("they view their progress for {string}")]
    public async Task WhenTheyViewTheirProgressFor(string exerciseName)
    {
        await progressPage.GotoAsync();
        await progressPage.SelectExerciseByName(exerciseName);
    }

    [Then("a progress data point is visible")]
    public Task ThenAProgressDataPointIsVisible() => progressPage.AssertTableRowVisible();
}
