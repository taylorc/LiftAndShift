using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.FamilyMembers.Delete;
using LiftAndShift.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.FamilyMembers;

public class Delete
  : Endpoint<DeleteFamilyMemberRequest,
             Results<NoContent,
                     NotFound,
                     ProblemHttpResult>>
{
  private readonly IMediator _mediator;
  public Delete(IMediator mediator) => _mediator = mediator;

  public override void Configure()
  {
    Delete(DeleteFamilyMemberRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Delete a family member";
      s.Description = "Deletes an existing family member by ID. This action cannot be undone.";
      s.ExampleRequest = new DeleteFamilyMemberRequest { FamilyMemberId = 1 };

      s.Responses[204] = "Family member deleted successfully";
      s.Responses[404] = "Family member not found";
      s.Responses[400] = "Invalid request or deletion failed";
    });

    Tags("FamilyMembers");

    Description(builder => builder
      .Accepts<DeleteFamilyMemberRequest>()
      .Produces(204)
      .ProducesProblem(404)
      .ProducesProblem(400));
  }

  public override async Task<Results<NoContent, NotFound, ProblemHttpResult>>
    ExecuteAsync(DeleteFamilyMemberRequest req, CancellationToken ct)
  {
    var cmd = new DeleteFamilyMemberCommand(FamilyMemberId.From(req.FamilyMemberId));
    var result = await _mediator.Send(cmd, ct);

    return result.ToDeleteResult();
  }
}
