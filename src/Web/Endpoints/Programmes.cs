using LiftAndShift.Application.Programmes.Commands.AdoptProgramme;
using LiftAndShift.Application.Programmes.Commands.LogProgrammeSession;
using LiftAndShift.Application.Programmes.Queries.GetActiveProgramme;
using LiftAndShift.Application.Programmes.Queries.GetProgrammeTemplates;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.Endpoints;

public class Programmes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetProgrammeTemplates, "templates");
        groupBuilder.MapGet(GetActiveProgramme, "active");
        groupBuilder.MapPost(AdoptProgramme, "adopt");
        groupBuilder.MapPost(LogProgrammeSession, "{id}/log-session");
    }

    [EndpointSummary("Get available programme templates")]
    public static async Task<Ok<List<ProgrammeTemplateDto>>> GetProgrammeTemplates(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetProgrammeTemplatesQuery()));

    [EndpointSummary("Get active programme")]
    public static async Task<Ok<ActiveProgrammeDto>> GetActiveProgramme(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetActiveProgrammeQuery()));

    [EndpointSummary("Adopt a programme")]
    public static async Task<Created<int>> AdoptProgramme(ISender sender, AdoptProgrammeCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Programmes/active", id);
    }

    [EndpointSummary("Log a programme session")]
    public static async Task<Created<int>> LogProgrammeSession(ISender sender, int id, LogProgrammeSessionCommand command)
    {
        var workoutId = await sender.Send(command with { UserProgrammeId = id });
        return TypedResults.Created($"/api/Workouts/{workoutId}", workoutId);
    }
}
