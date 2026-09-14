using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.FamilyMembers;
using LiftAndShift.UseCases.FamilyMembers.Update;
using LiftAndShift.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.FamilyMembers;

public class Update(IMediator mediator)
  : Endpoint<
        UpdateFamilyMemberRequest,
        Results<Ok<UpdateFamilyMemberResponse>, NotFound, ProblemHttpResult>,
        UpdateFamilyMemberMapper>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Put(UpdateFamilyMemberRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Update a family member";
      s.Description = "Updates an existing family member's name and PIN.";
      s.ExampleRequest = new UpdateFamilyMemberRequest { Id = 1, Name = "Updated Name", Pin = "4321" };
      s.ResponseExamples[200] = new UpdateFamilyMemberResponse(new FamilyMemberRecord(1, "Updated Name", "4321"));

      s.Responses[200] = "Family member updated successfully";
      s.Responses[404] = "Family member with specified ID not found";
      s.Responses[400] = "Invalid input data or business rule violation";
    });

    Tags("FamilyMembers");

    Description(builder => builder
      .Accepts<UpdateFamilyMemberRequest>("application/json")
      .Produces<UpdateFamilyMemberResponse>(200, "application/json")
      .ProducesProblem(404)
      .ProducesProblem(400));
  }

  public override async Task<Results<Ok<UpdateFamilyMemberResponse>, NotFound, ProblemHttpResult>>
    ExecuteAsync(UpdateFamilyMemberRequest request, CancellationToken ct)
  {
    var cmd = new UpdateFamilyMemberCommand(
      FamilyMemberId.From(request.Id),
      FamilyMemberName.From(request.Name!),
      Pin.From(request.Pin!));

    var result = await _mediator.Send(cmd, ct);

    return result.ToUpdateResult(Map.FromEntity);
  }
}

public sealed class UpdateFamilyMemberMapper
  : Mapper<UpdateFamilyMemberRequest, UpdateFamilyMemberResponse, FamilyMemberDto>
{
  public override UpdateFamilyMemberResponse FromEntity(FamilyMemberDto e)
    => new(new FamilyMemberRecord(e.Id.Value, e.Name.Value, e.Pin.Value));
}
