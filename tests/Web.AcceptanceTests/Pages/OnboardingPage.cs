using System.Text.RegularExpressions;

namespace LiftAndShift.Web.AcceptanceTests.Pages;

public class OnboardingPage(IPage page) : BasePage(page)
{
    public override string PagePath => $"{BaseUrl}/onboarding";

    public Task AssertHeading(string text) => Assertions.Expect(Page.Locator("h2")).ToHaveTextAsync(text);

    public Task SetBodyWeight(string value) => Page.FillAsync("#bodyWeight", value);

    public Task SetSquat(string value) => Page.FillAsync("#squat", value);

    public Task SetBench(string value) => Page.FillAsync("#bench", value);

    public Task SetOverheadPress(string value) => Page.FillAsync("#ohp", value);

    public Task SetDeadlift(string value) => Page.FillAsync("#deadlift", value);

    public Task SetAlternatingLiftWeight(string value) => Page.FillAsync("#altLiftWeight", value);

    public Task ClickStartTraining() => Page.Locator("button[type='submit']").ClickAsync();

    public Task AssertStillOnOnboardingPage() => Assertions.Expect(Page).ToHaveURLAsync(new Regex(".*/onboarding$"));

    public Task AssertRedirectedToHome() => Assertions.Expect(Page.Locator("h1")).ToHaveTextAsync("Welcome");

    public async Task FillRequiredWeights(string bodyWeight, string squat, string bench, string overheadPress, string deadlift, string alternatingLiftWeight)
    {
        await SetBodyWeight(bodyWeight);
        await SetSquat(squat);
        await SetBench(bench);
        await SetOverheadPress(overheadPress);
        await SetDeadlift(deadlift);
        await SetAlternatingLiftWeight(alternatingLiftWeight);
    }
}
