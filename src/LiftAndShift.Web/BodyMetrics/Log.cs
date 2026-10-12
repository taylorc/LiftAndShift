using System.ComponentModel.DataAnnotations;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.WorkWeightAggregate;
using LiftAndShift.UseCases.BodyMetrics;
using LiftAndShift.UseCases.BodyMetrics.Log;
using LiftAndShift.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.BodyMetrics;

public class Log(IMediator mediator)
  : Endpoint<LogBodyMetricRequest,
             Results<Created<BodyMetricRecord>,
                     NotFound,
                     ValidationProblem,
                     ProblemHttpResult>,
             LogBodyMetricMapper>
{
  public override void Configure()
  {
    Post(LogBodyMetricRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Log a bodyweight entry";
      s.Description = "Records a family member's bodyweight (in kg) for a given date. Entries are append-only; several entries on the same date are all kept.";
      s.ExampleRequest = new LogBodyMetricRequest
      {
        FamilyMemberId = 1,
        RecordedOn = new DateOnly(2026, 10, 12),
        BodyWeightKg = 82.5m
      };

      s.Responses[201] = "Bodyweight entry logged successfully";
      s.Responses[400] = "Invalid input (non-positive weight)";
      s.Responses[404] = "Family member not found";
    });

    Tags("BodyMetrics");

    Description(builder => builder
      .Accepts<LogBodyMetricRequest>("application/json")
      .Produces<BodyMetricRecord>(201, "application/json")
      .ProducesProblem(400)
      .ProducesProblem(404));
  }

  public override async Task<Results<Created<BodyMetricRecord>, NotFound, ValidationProblem, ProblemHttpResult>>
    ExecuteAsync(LogBodyMetricRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(
      new LogBodyMetricCommand(
        FamilyMemberId.From(request.FamilyMemberId),
        request.RecordedOn,
        WeightKg.From(request.BodyWeightKg)),
      ct);

    return result.ToCreatedOrNotFoundResult(
      dto => GetBodyMetricByIdRequest.BuildRoute(request.FamilyMemberId, dto.Id.Value),
      Map.FromEntity);
  }
}

public class LogBodyMetricRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/BodyMetrics";
  public static string BuildRoute(int familyMemberId) => Route.BuildRoute(familyMemberId);

  public int FamilyMemberId { get; set; }

  [Required]
  public DateOnly RecordedOn { get; set; }

  [Required]
  public decimal BodyWeightKg { get; set; }
}

public class LogBodyMetricValidator : Validator<LogBodyMetricRequest>
{
  public LogBodyMetricValidator()
  {
    RuleFor(x => x.FamilyMemberId)
      .GreaterThan(0);

    RuleFor(x => x.BodyWeightKg)
      .GreaterThan(0);
  }
}

public sealed class LogBodyMetricMapper
  : Mapper<LogBodyMetricRequest, BodyMetricRecord, BodyMetricDto>
{
  public override BodyMetricRecord FromEntity(BodyMetricDto e) => BodyMetricRecord.FromDto(e);
}
