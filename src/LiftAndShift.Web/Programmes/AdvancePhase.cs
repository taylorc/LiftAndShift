using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.Programmes;
using LiftAndShift.UseCases.Programmes.AdvancePhase;
using LiftAndShift.Web.Extensions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.Programmes;

public class AdvancePhase(IMediator mediator)
  : Endpoint<AdvanceTrainingPhaseRequest,
             Results<Ok<ProgrammeRecord>,
                     NotFound,
                     ProblemHttpResult>,
             AdvanceTrainingPhaseMapper>
{
  public override void Configure()
  {
    Post(AdvanceTrainingPhaseRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Advance a family member's Training Phase";
      s.Description = "Manually advances the family member's Programme to the next Training Phase (1 to 2, or 2 to 3). Fails if already at the final Training Phase.";
      s.ExampleRequest = new AdvanceTrainingPhaseRequest { FamilyMemberId = 1 };
      s.ResponseExamples[200] = new ProgrammeRecord(1, 1, 2);

      s.Responses[200] = "Training Phase advanced successfully";
      s.Responses[404] = "Family member (or their Programme) not found";
      s.Responses[400] = "Already at the final Training Phase";
    });

    Tags("Programmes");

    Description(builder => builder
      .Accepts<AdvanceTrainingPhaseRequest>()
      .Produces<ProgrammeRecord>(200, "application/json")
      .ProducesProblem(404)
      .ProducesProblem(400));
  }

  public override async Task<Results<Ok<ProgrammeRecord>, NotFound, ProblemHttpResult>>
    ExecuteAsync(AdvanceTrainingPhaseRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(new AdvanceTrainingPhaseCommand(FamilyMemberId.From(request.FamilyMemberId)), ct);

    return result.ToUpdateResult(Map.FromEntity);
  }
}

public class AdvanceTrainingPhaseRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/Programme/AdvancePhase";
  public static string BuildRoute(int familyMemberId) => Route.Replace("{FamilyMemberId:int}", familyMemberId.ToString());

  public int FamilyMemberId { get; set; }
}

public sealed class AdvanceTrainingPhaseMapper
  : Mapper<AdvanceTrainingPhaseRequest, ProgrammeRecord, ProgrammeDto>
{
  public override ProgrammeRecord FromEntity(ProgrammeDto e)
    => new(e.Id.Value, e.FamilyMemberId.Value, e.TrainingPhase.Value);
}
