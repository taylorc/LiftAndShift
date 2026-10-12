using LiftAndShift.Core.BodyMetricAggregate;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.BodyMetrics;
using LiftAndShift.UseCases.BodyMetrics.Get;
using LiftAndShift.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.BodyMetrics;

public class GetById(IMediator mediator)
  : Endpoint<GetBodyMetricByIdRequest,
             Results<Ok<BodyMetricRecord>,
                     NotFound,
                     ProblemHttpResult>,
             GetBodyMetricByIdMapper>
{
  public override void Configure()
  {
    Get(GetBodyMetricByIdRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Get a family member's bodyweight entry by ID";
      s.Description = "Retrieves a specific logged bodyweight entry.";
      s.ExampleRequest = new GetBodyMetricByIdRequest { FamilyMemberId = 1, BodyMetricId = 1 };

      s.Responses[200] = "Bodyweight entry found and returned successfully";
      s.Responses[404] = "Bodyweight entry not found for this family member";
    });

    Tags("BodyMetrics");

    Description(builder => builder
      .Accepts<GetBodyMetricByIdRequest>()
      .Produces<BodyMetricRecord>(200, "application/json")
      .ProducesProblem(404));
  }

  public override async Task<Results<Ok<BodyMetricRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetBodyMetricByIdRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(
      new GetBodyMetricQuery(FamilyMemberId.From(request.FamilyMemberId), BodyMetricId.From(request.BodyMetricId)),
      ct);

    return result.ToGetByIdResult(Map.FromEntity);
  }
}

public class GetBodyMetricByIdRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/BodyMetrics/{BodyMetricId:int}";
  public static string BuildRoute(int familyMemberId, int bodyMetricId) => Route.BuildRoute(familyMemberId, bodyMetricId);

  public int FamilyMemberId { get; set; }
  public int BodyMetricId { get; set; }
}

public class GetBodyMetricByIdValidator : Validator<GetBodyMetricByIdRequest>
{
  public GetBodyMetricByIdValidator()
  {
    RuleFor(x => x.FamilyMemberId).GreaterThan(0);
    RuleFor(x => x.BodyMetricId).GreaterThan(0);
  }
}

public sealed class GetBodyMetricByIdMapper
  : Mapper<GetBodyMetricByIdRequest, BodyMetricRecord, BodyMetricDto>
{
  public override BodyMetricRecord FromEntity(BodyMetricDto e) => BodyMetricRecord.FromDto(e);
}
