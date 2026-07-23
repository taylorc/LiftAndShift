namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

[Binding]
public sealed class LogWorkoutStepDefinitions(LogWorkoutPage logWorkoutPage, WorkoutDetailPage workoutDetailPage)
{
    [BeforeFeature("LogWorkout")]
    public static async Task BeforeLogWorkoutFeature(IObjectContainer container)
    {
        var context = await PlaywrightSetup.Browser.NewContextAsync();
        var page = await AuthSteps.LoginAsAdministrator(context);
        container.RegisterInstanceAs(context);
        container.RegisterInstanceAs(new LogWorkoutPage(page));
        container.RegisterInstanceAs(new WorkoutDetailPage(page));
    }

    [AfterFeature("LogWorkout")]
    public static async Task AfterLogWorkoutFeature(IObjectContainer container)
    {
        var context = container.Resolve<IBrowserContext>();
        await context.DisposeAsync();
    }

    [Given("an authenticated user visits the log workout page")]
    public Task GivenAnAuthenticatedUserVisitsTheLogWorkoutPage() => logWorkoutPage.GotoAsync();

    [When("they add the {string} exercise with a working weight")]
    public async Task TheyAddTheExerciseWithAWorkingWeight(string exerciseName)
    {
        await logWorkoutPage.SelectExerciseByName(exerciseName);
        await logWorkoutPage.ClickAddExercise();
        await logWorkoutPage.SetFirstSetWeight("60");
    }

    [When("they save the workout as a draft")]
    public Task TheySaveTheWorkoutAsADraft() => logWorkoutPage.ClickSaveAsDraft();

    [Then("they are taken to the workout detail page showing {string} as a draft")]
    public async Task TheyAreTakenToTheWorkoutDetailPageShowingAsADraft(string exerciseName)
    {
        await logWorkoutPage.WaitForWorkoutDetailUrlAndGetId();
        await workoutDetailPage.AssertExerciseVisible(exerciseName);
        await workoutDetailPage.AssertStatus("Draft");
    }
}
