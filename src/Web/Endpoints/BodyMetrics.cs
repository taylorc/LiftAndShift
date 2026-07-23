using LiftAndShift.Application.BodyMetrics.Commands.LogBodyMetric;
using LiftAndShift.Application.BodyMetrics.Queries.GetBodyMetrics;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.Endpoints;

public class BodyMetrics : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetBodyMetrics);
        groupBuilder.MapPost(LogBodyMetric);
    }

    [EndpointSummary("Get body metric history")]
    public static async Task<Ok<List<BodyMetricDto>>> GetBodyMetrics(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetBodyMetricsQuery()));

    [EndpointSummary("Log a body metric entry")]
    public static async Task<Created<int>> LogBodyMetric(ISender sender, LogBodyMetricCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/BodyMetrics/{id}", id);
    }
}
