using LiftAndShift.Application.Programmes.Commands.AdoptProgramme;
using LiftAndShift.Application.Programmes.Commands.DeleteProgrammeSession;
using LiftAndShift.Application.Programmes.Commands.EditProgrammeSession;
using LiftAndShift.Application.Programmes.Commands.LogProgrammeSession;
using LiftAndShift.Application.Programmes.Commands.UpdateProgramme;
using LiftAndShift.Application.Programmes.Commands.UpdateProgrammeSessionInputs;
using LiftAndShift.Application.Programmes.Queries.GetActiveProgramme;
using LiftAndShift.Application.Programmes.Queries.GetProgrammeSessions;
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
        groupBuilder.MapGet(GetProgrammeSessions, "{id}/sessions");
        groupBuilder.MapPost(AdoptProgramme, "adopt");
        groupBuilder.MapPost(LogProgrammeSession, "{id}/log-session");
        groupBuilder.MapPut(EditProgrammeSession, "{id}/sessions/{sessionId}");
        groupBuilder.MapDelete(DeleteProgrammeSession, "{id}/sessions/{sessionId}");
        groupBuilder.MapPatch(UpdateProgrammeSessionInputs, "{id}/sessions/{sessionId}");
        groupBuilder.MapPatch(UpdateProgramme, "{id}");
    }

    [EndpointSummary("Get available programme templates")]
    public static async Task<Ok<List<ProgrammeTemplateDto>>> GetProgrammeTemplates(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetProgrammeTemplatesQuery()));

    [EndpointSummary("Get active programme")]
    public static async Task<Ok<ActiveProgrammeDto>> GetActiveProgramme(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetActiveProgrammeQuery()));

    [EndpointSummary("Get the logged sessions of a programme")]
    public static async Task<Ok<List<LoggedProgrammeSessionDto>>> GetProgrammeSessions(ISender sender, int id)
        => TypedResults.Ok(await sender.Send(new GetProgrammeSessionsQuery(id)));

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

    [EndpointSummary("Edit a logged programme session and replay downstream progression")]
    public static async Task<NoContent> EditProgrammeSession(ISender sender, int id, int sessionId, EditProgrammeSessionCommand command)
    {
        await sender.Send(command with { UserProgrammeId = id, ProgrammeSessionId = sessionId });
        return TypedResults.NoContent();
    }

    [EndpointSummary("Delete the most recently logged programme session")]
    public static async Task<NoContent> DeleteProgrammeSession(ISender sender, int id, int sessionId)
    {
        await sender.Send(new DeleteProgrammeSessionCommand { UserProgrammeId = id, ProgrammeSessionId = sessionId });
        return TypedResults.NoContent();
    }

    [EndpointSummary("Update programme metadata")]
    public static async Task<NoContent> UpdateProgramme(ISender sender, int id, UpdateProgrammeCommand command)
    {
        await sender.Send(command with { UserProgrammeId = id });
        return TypedResults.NoContent();
    }

    [EndpointSummary("Override a session's prescribed weights and replay downstream progression")]
    public static async Task<NoContent> UpdateProgrammeSessionInputs(ISender sender, int id, int sessionId, UpdateProgrammeSessionInputsCommand command)
    {
        await sender.Send(command with { UserProgrammeId = id, ProgrammeSessionId = sessionId });
        return TypedResults.NoContent();
    }
}
