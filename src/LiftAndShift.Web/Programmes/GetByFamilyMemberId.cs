using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.Programmes;
using LiftAndShift.UseCases.Programmes.Get;
using LiftAndShift.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.Programmes;

public class GetByFamilyMemberId(IMediator mediator)
  : Endpoint<GetProgrammeByFamilyMemberIdRequest,
             Results<Ok<ProgrammeRecord>,
                     NotFound,
                     ProblemHttpResult>,
             GetProgrammeByFamilyMemberIdMapper>
{
  public override void Configure()
  {
    Get(GetProgrammeByFamilyMemberIdRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Get a family member's Programme";
      s.Description = "Retrieves the Programme (current Training Phase) for the specified family member.";
      s.ExampleRequest = new GetProgrammeByFamilyMemberIdRequest { FamilyMemberId = 1 };
      s.ResponseExamples[200] = new ProgrammeRecord(1, 1, 1);

      s.Responses[200] = "Programme found and returned successfully";
      s.Responses[404] = "Family member (or their Programme) not found";
    });

    Tags("Programmes");

    Description(builder => builder
      .Accepts<GetProgrammeByFamilyMemberIdRequest>()
      .Produces<ProgrammeRecord>(200, "application/json")
      .ProducesProblem(404));
  }

  public override async Task<Results<Ok<ProgrammeRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetProgrammeByFamilyMemberIdRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(new GetProgrammeQuery(FamilyMemberId.From(request.FamilyMemberId)), ct);

    return result.ToGetByIdResult(Map.FromEntity);
  }
}

public class GetProgrammeByFamilyMemberIdRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/Programme";
  public static string BuildRoute(int familyMemberId) => Route.Replace("{FamilyMemberId:int}", familyMemberId.ToString());

  public int FamilyMemberId { get; set; }
}

public sealed class GetProgrammeByFamilyMemberIdMapper
  : Mapper<GetProgrammeByFamilyMemberIdRequest, ProgrammeRecord, ProgrammeDto>
{
  public override ProgrammeRecord FromEntity(ProgrammeDto e)
    => new(e.Id.Value, e.FamilyMemberId.Value, e.TrainingPhase.Value);
}
