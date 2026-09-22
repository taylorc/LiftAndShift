using System.ComponentModel.DataAnnotations;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;
using LiftAndShift.UseCases.WorkWeights;
using LiftAndShift.UseCases.WorkWeights.CompleteRamp;
using LiftAndShift.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.WorkWeights;

public class CompleteRamp(IMediator mediator)
  : Endpoint<CompleteRampRequest,
             Results<Ok<WorkWeightRecord>,
                     NotFound,
                     ProblemHttpResult>,
             CompleteRampMapper>
{
  public override void Configure()
  {
    Post(CompleteRampRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Complete a family member's Ramp for a lift";
      s.Description = "Records the Work Weight discovered by the Ramp procedure for the specified family member and lift, based on the set at which the bar slowed. Fails if this lift has already been Ramped for this family member.";
      s.ExampleRequest = new CompleteRampRequest { FamilyMemberId = 1, Lift = "Squat", FinalSetNumber = 3 };
      s.ResponseExamples[200] = new WorkWeightRecord(1, 1, "Squat", 30, 0);

      s.Responses[200] = "Ramp completed and Work Weight recorded successfully";
      s.Responses[404] = "Family member not found";
      s.Responses[400] = "This lift has already been Ramped for this family member";
    });

    Tags("WorkWeights");

    Description(builder => builder
      .Accepts<CompleteRampRequest>()
      .Produces<WorkWeightRecord>(200, "application/json")
      .ProducesProblem(404)
      .ProducesProblem(400));
  }

  public override async Task<Results<Ok<WorkWeightRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(CompleteRampRequest request, CancellationToken ct)
  {
    var lift = Lift.FromName(request.Lift!, ignoreCase: true);
    var result = await mediator.Send(
      new CompleteRampCommand(FamilyMemberId.From(request.FamilyMemberId), lift, request.FinalSetNumber),
      ct);

    return result.ToUpdateResult(Map.FromEntity);
  }
}

public class CompleteRampRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/WorkWeights/{Lift}/CompleteRamp";
  public static string BuildRoute(int familyMemberId, string lift) => Route.BuildRoute(familyMemberId, lift);

  public int FamilyMemberId { get; set; }
  public string? Lift { get; set; }

  [Required]
  public int FinalSetNumber { get; set; }
}

public class CompleteRampValidator : Validator<CompleteRampRequest>
{
  public CompleteRampValidator()
  {
    RuleFor(x => x.FamilyMemberId)
      .GreaterThan(0);

    RuleFor(x => x.Lift)
      .NotEmpty()
      .MustBeAKnownLift();

    RuleFor(x => x.FinalSetNumber)
      .GreaterThan(0);
  }
}

public sealed class CompleteRampMapper
  : Mapper<CompleteRampRequest, WorkWeightRecord, WorkWeightDto>
{
  public override WorkWeightRecord FromEntity(WorkWeightDto e)
    => new(e.Id.Value, e.FamilyMemberId.Value, e.Lift.Name, e.WeightKg.Value, e.ConsecutiveFailures);
}
