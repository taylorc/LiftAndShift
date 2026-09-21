using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;
using LiftAndShift.UseCases.WorkWeights;
using LiftAndShift.UseCases.WorkWeights.Get;
using LiftAndShift.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.WorkWeights;

public class GetByFamilyMemberIdAndLift(IMediator mediator)
  : Endpoint<GetWorkWeightRequest,
             Results<Ok<WorkWeightRecord>,
                     NotFound,
                     ProblemHttpResult>,
             GetWorkWeightMapper>
{
  public override void Configure()
  {
    Get(GetWorkWeightRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Get a family member's Work Weight for a lift";
      s.Description = "Retrieves the Work Weight established by the Ramp for the specified family member and lift.";
      s.ExampleRequest = new GetWorkWeightRequest { FamilyMemberId = 1, Lift = "Squat" };
      s.ResponseExamples[200] = new WorkWeightRecord(1, 1, "Squat", 30);

      s.Responses[200] = "Work Weight found and returned successfully";
      s.Responses[404] = "Family member, lift, or Work Weight not found (lift not yet Ramped)";
    });

    Tags("WorkWeights");

    Description(builder => builder
      .Accepts<GetWorkWeightRequest>()
      .Produces<WorkWeightRecord>(200, "application/json")
      .ProducesProblem(404));
  }

  public override async Task<Results<Ok<WorkWeightRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetWorkWeightRequest request, CancellationToken ct)
  {
    var lift = Lift.FromName(request.Lift!, ignoreCase: true);
    var result = await mediator.Send(new GetWorkWeightQuery(FamilyMemberId.From(request.FamilyMemberId), lift), ct);

    return result.ToGetByIdResult(Map.FromEntity);
  }
}

public class GetWorkWeightRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/WorkWeights/{Lift}";
  public static string BuildRoute(int familyMemberId, string lift) =>
    Route.Replace("{FamilyMemberId:int}", familyMemberId.ToString()).Replace("{Lift}", lift);

  public int FamilyMemberId { get; set; }
  public string? Lift { get; set; }
}

public class GetWorkWeightValidator : Validator<GetWorkWeightRequest>
{
  public GetWorkWeightValidator()
  {
    RuleFor(x => x.FamilyMemberId)
      .GreaterThan(0);

    RuleFor(x => x.Lift)
      .NotEmpty()
      .Must(name => Lift.TryFromName(name!, ignoreCase: true, out _))
      .WithMessage("Lift must be one of the known lifts.");
  }
}

public sealed class GetWorkWeightMapper
  : Mapper<GetWorkWeightRequest, WorkWeightRecord, WorkWeightDto>
{
  public override WorkWeightRecord FromEntity(WorkWeightDto e)
    => new(e.Id.Value, e.FamilyMemberId.Value, e.Lift.Name, e.WeightKg.Value);
}
