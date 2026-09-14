using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.FamilyMembers;
using LiftAndShift.UseCases.FamilyMembers.Get;
using LiftAndShift.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.FamilyMembers;

public class GetById(IMediator mediator)
  : Endpoint<GetFamilyMemberByIdRequest,
             Results<Ok<FamilyMemberRecord>,
                     NotFound,
                     ProblemHttpResult>,
             GetFamilyMemberByIdMapper>
{
  public override void Configure()
  {
    Get(GetFamilyMemberByIdRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Get a family member by ID";
      s.Description = "Retrieves a specific family member by their unique identifier.";
      s.ExampleRequest = new GetFamilyMemberByIdRequest { FamilyMemberId = 1 };
      s.ResponseExamples[200] = new FamilyMemberRecord(1, "Ada", "1234");

      s.Responses[200] = "Family member found and returned successfully";
      s.Responses[404] = "Family member with specified ID not found";
    });

    Tags("FamilyMembers");

    Description(builder => builder
      .Accepts<GetFamilyMemberByIdRequest>()
      .Produces<FamilyMemberRecord>(200, "application/json")
      .ProducesProblem(404));
  }

  public override async Task<Results<Ok<FamilyMemberRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetFamilyMemberByIdRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(new GetFamilyMemberQuery(FamilyMemberId.From(request.FamilyMemberId)), ct);

    return result.ToGetByIdResult(Map.FromEntity);
  }
}
public sealed class GetFamilyMemberByIdMapper
  : Mapper<GetFamilyMemberByIdRequest, FamilyMemberRecord, FamilyMemberDto>
{
  public override FamilyMemberRecord FromEntity(FamilyMemberDto e)
    => new(e.Id.Value, e.Name.Value, e.Pin.Value);
}
