namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class WorkoutDetailStepDefinitions(LogWorkoutPage logWorkoutPage, WorkoutDetailPage workoutDetailPage)
{
    [BeforeFeature("WorkoutDetail")]
    public static async Task BeforeWorkoutDetailFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new LogWorkoutPage(page));
        container.RegisterInstanceAs(new WorkoutDetailPage(page));
    }

    [AfterFeature("WorkoutDetail")]
    public static async Task AfterWorkoutDetailFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user has a draft workout for {string} open on its detail page")]
    public async Task GivenAnAuthenticatedUserHasADraftWorkoutForOpenOnItsDetailPage(string exerciseName)
    {
        await logWorkoutPage.GotoAsync();
        await logWorkoutPage.SelectExerciseByName(exerciseName);
        await logWorkoutPage.ClickAddExercise();
        await logWorkoutPage.SetFirstSetWeight("60");
        await logWorkoutPage.ClickSaveAsDraft();
        await logWorkoutPage.WaitForWorkoutDetailUrlAndGetId();
    }

    [When("they mark the workout as complete")]
    public Task WhenTheyMarkTheWorkoutAsComplete() => workoutDetailPage.ClickMarkComplete();

    [Then("the workout status shows {string}")]
    public Task ThenTheWorkoutStatusShows(string status) => workoutDetailPage.AssertStatus(status);

    [When("they duplicate the workout")]
    public Task WhenTheyDuplicateTheWorkout() => workoutDetailPage.ClickDuplicate();

    [Then("they are taken to a new draft workout detail page")]
    public async Task ThenTheyAreTakenToANewDraftWorkoutDetailPage()
    {
        await workoutDetailPage.AssertOnWorkoutDetailPage();
        await workoutDetailPage.AssertStatus("Draft");
    }
}
