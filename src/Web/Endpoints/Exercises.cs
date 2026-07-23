using LiftAndShift.Application.Exercises.Commands.CreateExercise;
using LiftAndShift.Application.Exercises.Commands.DeleteExercise;
using LiftAndShift.Application.Exercises.Queries.GetExercises;
using LiftAndShift.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.Endpoints;

public class Exercises : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(GetExercises);
        groupBuilder.MapPost(CreateExercise);
        groupBuilder.MapDelete(DeleteExercise, "{id}");
    }

    [EndpointSummary("Get exercises")]
    public static async Task<Ok<List<ExerciseDto>>> GetExercises(
        ISender sender,
        string? search = null,
        MuscleGroup? muscleGroup = null,
        EquipmentType? equipmentType = null)
    {
        var result = await sender.Send(new GetExercisesQuery
        {
            Search = search,
            MuscleGroup = muscleGroup,
            EquipmentType = equipmentType
        });
        return TypedResults.Ok(result);
    }

    [EndpointSummary("Create a custom exercise")]
    public static async Task<Created<int>> CreateExercise(ISender sender, CreateExerciseCommand command)
    {
        var id = await sender.Send(command);
        return TypedResults.Created($"/api/Exercises/{id}", id);
    }

    [EndpointSummary("Delete a custom exercise")]
    public static async Task<NoContent> DeleteExercise(ISender sender, int id)
    {
        await sender.Send(new DeleteExerciseCommand(id));
        return TypedResults.NoContent();
    }
}
