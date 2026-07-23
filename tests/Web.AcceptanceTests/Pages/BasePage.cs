namespace LiftAndShift.Web.AcceptanceTests.Pages;

public abstract class BasePage(IPage page)
{
    protected static string BaseUrl => AspireSetup.App.GetEndpoint(Shared.Services.WebFrontend).ToString().TrimEnd('/');

    public abstract string PagePath { get; }

    protected IPage Page { get; } = page;

    public async Task GotoAsync()
    {
        await Page.GotoAsync(PagePath);

        // Nuxt server-renders the initial HTML, but client-side hydration finishes slightly
        // later. Filling a form before hydration completes can have Vue overwrite the typed
        // value once it reconciles the input against its (still empty) reactive state.
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
}
