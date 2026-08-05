//namespace LiftAndShift.Web.AcceptanceTests.StepDefinitions;

//[Binding]
//public sealed class OnboardingStepDefinitions(OnboardingPage onboardingPage)
//{
//    [BeforeFeature("Onboarding")]
//    public static async Task BeforeOnboardingFeature(IObjectContainer container)
//    {
//        var context = await PlaywrightSetup.Browser.NewContextAsync();
//        var page = await AuthSteps.LoginAsAdministrator(context);
//        container.RegisterInstanceAs(context);
//        container.RegisterInstanceAs(new OnboardingPage(page));
//    }

//    [AfterFeature("Onboarding")]
//    public static async Task AfterOnboardingFeature(IObjectContainer container)
//    {
//        var context = container.Resolve<IBrowserContext>();
//        await context.DisposeAsync();
//    }

//    [Given("an authenticated user is on the onboarding page")]
//    public Task GivenAnAuthenticatedUserIsOnTheOnboardingPage() => onboardingPage.GotoAsync();

//    [When("they submit valid starting weights")]
//    public async Task TheySubmitValidStartingWeights()
//    {
//        await onboardingPage.FillRequiredWeights("180", "120", "80", "50", "150", "60");
//        await onboardingPage.ClickStartTraining();
//    }

//    [Then("they are redirected to the home page")]
//    public Task TheyAreRedirectedToTheHomePage() => onboardingPage.AssertRedirectedToHome();

//    [When("they submit the form without a squat starting weight")]
//    public async Task TheySubmitTheFormWithoutASquatStartingWeight()
//    {
//        await onboardingPage.SetBodyWeight("180");
//        await onboardingPage.SetBench("80");
//        await onboardingPage.SetOverheadPress("50");
//        await onboardingPage.SetDeadlift("150");
//        await onboardingPage.SetAlternatingLiftWeight("60");
//        await onboardingPage.ClickStartTraining();
//    }

//    [Then("they remain on the onboarding page")]
//    public Task TheyRemainOnTheOnboardingPage() => onboardingPage.AssertStillOnOnboardingPage();
//}
