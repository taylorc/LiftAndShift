using System.ComponentModel.DataAnnotations;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.Workouts;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.Core.WorkWeightAggregate;
using LiftAndShift.UseCases.WorkoutSessions;
using LiftAndShift.UseCases.WorkoutSessions.Log;
using LiftAndShift.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.WorkoutSessions;

public class Log(IMediator mediator)
  : Endpoint<LogWorkoutSessionRequest,
             Results<Created<WorkoutSessionRecord>,
                     NotFound,
                     ValidationProblem,
                     ProblemHttpResult>,
             LogWorkoutSessionMapper>
{
  public override void Configure()
  {
    Post(LogWorkoutSessionRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Log a completed Workout Session";
      s.Description = "Records a completed Workout A or Workout B session: the date, and for each lift in that workout (resolved from the family member's current Training Phase), the weight used and the reps achieved on each set. Every logged lift must already have a Work Weight established via Ramp.";
      s.ExampleRequest = new LogWorkoutSessionRequest
      {
        FamilyMemberId = 1,
        Workout = "A",
        PerformedOn = new DateOnly(2026, 9, 21),
        LoggedLifts =
        [
          new LoggedLiftRequestItem { Lift = "Squat", WeightKg = 30, RepsPerSet = [5, 5, 5] },
          new LoggedLiftRequestItem { Lift = "Press", WeightKg = 25, RepsPerSet = [5, 5, 5] },
          new LoggedLiftRequestItem { Lift = "Deadlift", WeightKg = 90, RepsPerSet = [5] }
        ]
      };

      s.Responses[201] = "Workout Session logged successfully";
      s.Responses[400] = "Invalid input (unknown lift/workout, wrong number of sets, or a lift not yet Ramped)";
      s.Responses[404] = "Family member not found";
    });

    Tags("WorkoutSessions");

    Description(builder => builder
      .Accepts<LogWorkoutSessionRequest>("application/json")
      .Produces<WorkoutSessionRecord>(201, "application/json")
      .ProducesProblem(400)
      .ProducesProblem(404));
  }

  public override async Task<Results<Created<WorkoutSessionRecord>, NotFound, ValidationProblem, ProblemHttpResult>>
    ExecuteAsync(LogWorkoutSessionRequest request, CancellationToken ct)
  {
    var loggedLifts = request.LoggedLifts
      .Select(item => new LoggedLiftInput(
        Lift.FromName(item.Lift!, ignoreCase: true),
        WeightKg.From(item.WeightKg),
        item.RepsPerSet))
      .ToList();

    var result = await mediator.Send(
      new LogWorkoutSessionCommand(
        FamilyMemberId.From(request.FamilyMemberId),
        Workout.FromName(request.Workout!, ignoreCase: true),
        request.PerformedOn,
        loggedLifts),
      ct);

    return result.ToCreatedOrNotFoundResult(
      dto => $"/FamilyMembers/{request.FamilyMemberId}/WorkoutSessions/{dto.Id.Value}",
      Map.FromEntity);
  }
}

public class LogWorkoutSessionRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/WorkoutSessions";
  public static string BuildRoute(int familyMemberId) => Route.Replace("{FamilyMemberId:int}", familyMemberId.ToString());

  public int FamilyMemberId { get; set; }

  [Required]
  public string? Workout { get; set; }

  [Required]
  public DateOnly PerformedOn { get; set; }

  [Required]
  public List<LoggedLiftRequestItem> LoggedLifts { get; set; } = [];
}

public class LoggedLiftRequestItem
{
  [Required]
  public string? Lift { get; set; }

  [Required]
  public decimal WeightKg { get; set; }

  [Required]
  public List<int> RepsPerSet { get; set; } = [];
}

public class LogWorkoutSessionValidator : Validator<LogWorkoutSessionRequest>
{
  public LogWorkoutSessionValidator()
  {
    RuleFor(x => x.FamilyMemberId)
      .GreaterThan(0);

    RuleFor(x => x.Workout)
      .NotEmpty()
      .Must(name => Workout.TryFromName(name!, ignoreCase: true, out _))
      .WithMessage("Workout must be 'A' or 'B'.");

    RuleFor(x => x.LoggedLifts)
      .NotEmpty()
      .WithMessage("At least one logged lift is required.");

    RuleForEach(x => x.LoggedLifts).ChildRules(loggedLift =>
    {
      loggedLift.RuleFor(l => l.Lift)
        .NotEmpty()
        .Must(name => Lift.TryFromName(name!, ignoreCase: true, out _))
        .WithMessage("Lift must be one of the known lifts.");

      loggedLift.RuleFor(l => l.WeightKg)
        .GreaterThan(0);

      loggedLift.RuleFor(l => l.RepsPerSet)
        .NotEmpty();

      loggedLift.RuleForEach(l => l.RepsPerSet)
        .GreaterThanOrEqualTo(0)
        .WithMessage("Reps achieved cannot be negative.");
    });
  }
}

public sealed class LogWorkoutSessionMapper
  : Mapper<LogWorkoutSessionRequest, WorkoutSessionRecord, WorkoutSessionDto>
{
  public override WorkoutSessionRecord FromEntity(WorkoutSessionDto e)
    => new(
      e.Id.Value,
      e.FamilyMemberId.Value,
      e.Workout.Name,
      e.TrainingPhase.Value,
      e.PerformedOn,
      e.LoggedSets.Select(s => new LoggedSetRecord(s.Id.Value, s.Lift.Name, s.WeightKg.Value, s.SetNumber, s.RepsAchieved)).ToList(),
      e.LiftOutcomes.Select(o => new LiftOutcomeRecord(o.Lift.Name, o.Successful, o.NewWeightKg.Value, o.Deloaded)).ToList());
}
