using LiftAndShift.Application.Dashboard.Queries.GetDashboard;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.Endpoints;

public class Dashboard : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetDashboard);
    }

    [EndpointSummary("Get dashboard summary")]
    public static async Task<Ok<DashboardDto>> GetDashboard(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetDashboardQuery()));
}
