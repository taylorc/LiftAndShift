using LiftAndShift.Application.Common.Models;
using LiftAndShift.Application.Onboarding.Commands.SaveUserOnboarding;
using LiftAndShift.Application.Onboarding.Queries.GetUserOnboarding;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.Endpoints;

public class Onboarding : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetOnboarding);
        groupBuilder.MapPost(SaveOnboarding);
    }

    [EndpointSummary("Get onboarding data")]
    [EndpointDescription("Returns the current user's onboarding profile.")]
    public static async Task<Ok<UserOnboardingDto>> GetOnboarding(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetUserOnboardingQuery()));

    [EndpointSummary("Save onboarding data")]
    [EndpointDescription("Saves the onboarding profile for the current user and marks onboarding complete.")]
    public static async Task<Results<NoContent, BadRequest<string[]>>> SaveOnboarding(
        SaveUserOnboardingCommand command, ISender sender)
    {
        var result = await sender.Send(command);

        return result.Succeeded
            ? TypedResults.NoContent()
            : TypedResults.BadRequest(result.Errors);
    }
}
