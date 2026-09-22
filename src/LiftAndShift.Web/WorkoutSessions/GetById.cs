using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.UseCases.WorkoutSessions;
using LiftAndShift.UseCases.WorkoutSessions.Get;
using LiftAndShift.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.WorkoutSessions;

public class GetById(IMediator mediator)
  : Endpoint<GetWorkoutSessionByIdRequest,
             Results<Ok<WorkoutSessionRecord>,
                     NotFound,
                     ProblemHttpResult>,
             GetWorkoutSessionByIdMapper>
{
  public override void Configure()
  {
    Get(GetWorkoutSessionByIdRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Get a family member's logged Workout Session by ID";
      s.Description = "Retrieves a specific Workout Session, including every logged set.";
      s.ExampleRequest = new GetWorkoutSessionByIdRequest { FamilyMemberId = 1, WorkoutSessionId = 1 };

      s.Responses[200] = "Workout Session found and returned successfully";
      s.Responses[404] = "Workout Session not found for this family member";
    });

    Tags("WorkoutSessions");

    Description(builder => builder
      .Accepts<GetWorkoutSessionByIdRequest>()
      .Produces<WorkoutSessionRecord>(200, "application/json")
      .ProducesProblem(404));
  }

  public override async Task<Results<Ok<WorkoutSessionRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetWorkoutSessionByIdRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(
      new GetWorkoutSessionQuery(FamilyMemberId.From(request.FamilyMemberId), WorkoutSessionId.From(request.WorkoutSessionId)),
      ct);

    return result.ToGetByIdResult(Map.FromEntity);
  }
}

public class GetWorkoutSessionByIdRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/WorkoutSessions/{WorkoutSessionId:int}";
  public static string BuildRoute(int familyMemberId, int workoutSessionId) => Route.BuildRoute(familyMemberId, workoutSessionId);

  public int FamilyMemberId { get; set; }
  public int WorkoutSessionId { get; set; }
}

public class GetWorkoutSessionByIdValidator : Validator<GetWorkoutSessionByIdRequest>
{
  public GetWorkoutSessionByIdValidator()
  {
    RuleFor(x => x.FamilyMemberId).GreaterThan(0);
    RuleFor(x => x.WorkoutSessionId).GreaterThan(0);
  }
}

public sealed class GetWorkoutSessionByIdMapper
  : Mapper<GetWorkoutSessionByIdRequest, WorkoutSessionRecord, WorkoutSessionDto>
{
  public override WorkoutSessionRecord FromEntity(WorkoutSessionDto e) => WorkoutSessionRecord.FromDto(e);
}
