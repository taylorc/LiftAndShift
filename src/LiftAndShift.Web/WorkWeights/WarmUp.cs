using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkWeightAggregate;
using LiftAndShift.UseCases.WorkWeights.WarmUp;
using LiftAndShift.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.WorkWeights;

public class WarmUp(IMediator mediator)
  : Endpoint<GetWarmUpSetsRequest,
             Results<Ok<List<WarmUpSetRecord>>,
                     NotFound,
                     ProblemHttpResult>,
             GetWarmUpSetsMapper>
{
  public override void Configure()
  {
    Get(GetWarmUpSetsRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Get the warm-up sets for a family member's lift";
      s.Description = "Computes the fixed warm-up ladder leading up to (but not including) the family member's current Work Weight for the specified lift. Nothing is persisted - this is recomputed from the current Work Weight on every call.";
      s.ExampleRequest = new GetWarmUpSetsRequest { FamilyMemberId = 1, Lift = "Squat" };
      s.ResponseExamples[200] = new List<WarmUpSetRecord>
      {
        new(20, 5),
        new(12, 5),
        new(18, 3),
        new(24, 2)
      };

      s.Responses[200] = "Warm-up sets computed successfully";
      s.Responses[404] = "Family member, lift, or Work Weight not found (lift not yet Ramped)";
    });

    Tags("WorkWeights");

    Description(builder => builder
      .Accepts<GetWarmUpSetsRequest>()
      .Produces<List<WarmUpSetRecord>>(200, "application/json")
      .ProducesProblem(404));
  }

  public override async Task<Results<Ok<List<WarmUpSetRecord>>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetWarmUpSetsRequest request, CancellationToken ct)
  {
    var lift = Lift.FromName(request.Lift!, ignoreCase: true);
    var result = await mediator.Send(new GetWarmUpSetsQuery(FamilyMemberId.From(request.FamilyMemberId), lift), ct);

    return result.ToGetByIdResult(Map.FromEntity);
  }
}

public class GetWarmUpSetsRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/WorkWeights/{Lift}/WarmUp";
  public static string BuildRoute(int familyMemberId, string lift) => Route.BuildRoute(familyMemberId, lift);

  public int FamilyMemberId { get; set; }
  public string? Lift { get; set; }
}

public class GetWarmUpSetsValidator : Validator<GetWarmUpSetsRequest>
{
  public GetWarmUpSetsValidator()
  {
    RuleFor(x => x.FamilyMemberId)
      .GreaterThan(0);

    RuleFor(x => x.Lift)
      .NotEmpty()
      .MustBeAKnownLift();
  }
}

public sealed class GetWarmUpSetsMapper
  : Mapper<GetWarmUpSetsRequest, List<WarmUpSetRecord>, IReadOnlyList<WarmUpSet>>
{
  public override List<WarmUpSetRecord> FromEntity(IReadOnlyList<WarmUpSet> e)
    => e.Select(s => new WarmUpSetRecord(s.WeightKg.Value, s.Reps)).ToList();
}
