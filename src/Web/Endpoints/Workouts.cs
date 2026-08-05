using LiftAndShift.Application.Workouts.Commands.CompleteWorkout;
using LiftAndShift.Application.Workouts.Commands.DuplicateWorkout;
using LiftAndShift.Application.Workouts.Commands.LogWorkout;
using LiftAndShift.Application.Workouts.Queries.GetExerciseProgress;
using LiftAndShift.Application.Workouts.Queries.GetWorkout;
using LiftAndShift.Application.Workouts.Queries.GetWorkoutHistory;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.Endpoints;

public class Workouts : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetWorkoutHistory);
        groupBuilder.MapGet(GetWorkout, "{id}");
        groupBuilder.MapPost(LogWorkout);
        groupBuilder.MapPost(CompleteWorkout, "{id}/complete");
        groupBuilder.MapPost(DuplicateWorkout, "{id}/duplicate");
        groupBuilder.MapGet(GetExerciseProgress, "progress/{exerciseId}");
    }

    [EndpointSummary("Get workout history")]
    public static async Task<Ok<List<WorkoutHistoryItemDto>>> GetWorkoutHistory(ISender sender)
        => TypedResults.Ok(await sender.Send(new GetWorkoutHistoryQuery()));

    [EndpointSummary("Get workout detail")]
    public static async Task<Ok<WorkoutDetailDto>> GetWorkout(ISender sender, int id)
        => TypedResults.Ok(await sender.Send(new GetWorkoutQuery(id)));

    [EndpointSummary("Log a workout")]
    public static async Task<Created<int>> LogWorkout(ISender sender, LogWorkoutCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Workouts/{id}", id);
    }

    [EndpointSummary("Complete a workout")]
    public static async Task<NoContent> CompleteWorkout(ISender sender, int id)
    {
        await sender.Send(new CompleteWorkoutCommand(id));
        return TypedResults.NoContent();
    }

    [EndpointSummary("Duplicate a workout")]
    public static async Task<Created<int>> DuplicateWorkout(ISender sender, int id)
    {
        var newId = await sender.Send(new DuplicateWorkoutCommand(id));
        return TypedResults.Created($"/api/Workouts/{newId}", newId);
    }

    [EndpointSummary("Get exercise progress over time")]
    public static async Task<Ok<List<ExerciseProgressPointDto>>> GetExerciseProgress(ISender sender, int exerciseId)
        => TypedResults.Ok(await sender.Send(new GetExerciseProgressQuery(exerciseId)));
}
