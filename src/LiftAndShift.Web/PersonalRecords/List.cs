using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.PersonalRecords;
using LiftAndShift.UseCases.PersonalRecords.Get;
using LiftAndShift.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.PersonalRecords;

public class List(IMediator mediator)
  : Endpoint<GetPersonalRecordsRequest,
             Results<Ok<List<PersonalRecordRecord>>,
                     NotFound,
                     ProblemHttpResult>,
             GetPersonalRecordsMapper>
{
  public override void Configure()
  {
    Get(GetPersonalRecordsRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "Get a family member's Personal Records";
      s.Description = "For each lift, the heaviest weight the family member has ever completed a fully successful session at. A lift with no successful session is omitted entirely, rather than shown as zero.";
      s.ExampleRequest = new GetPersonalRecordsRequest { FamilyMemberId = 1 };
      s.ResponseExamples[200] = new List<PersonalRecordRecord>
      {
        new("Squat", 100, new DateOnly(2026, 6, 1), 42)
      };

      s.Responses[200] = "Personal Records returned successfully (an empty list if none exist yet)";
      s.Responses[404] = "Family member not found";
    });

    Tags("PersonalRecords");

    Description(builder => builder
      .Accepts<GetPersonalRecordsRequest>()
      .Produces<List<PersonalRecordRecord>>(200, "application/json")
      .ProducesProblem(404));
  }

  public override async Task<Results<Ok<List<PersonalRecordRecord>>, NotFound, ProblemHttpResult>>
    ExecuteAsync(GetPersonalRecordsRequest request, CancellationToken ct)
  {
    var result = await mediator.Send(new GetPersonalRecordsQuery(FamilyMemberId.From(request.FamilyMemberId)), ct);

    return result.ToGetByIdResult(Map.FromEntity);
  }
}

public class GetPersonalRecordsRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/PersonalRecords";
  public static string BuildRoute(int familyMemberId) => Route.Replace("{FamilyMemberId:int}", familyMemberId.ToString());

  public int FamilyMemberId { get; set; }
}

public class GetPersonalRecordsValidator : Validator<GetPersonalRecordsRequest>
{
  public GetPersonalRecordsValidator()
  {
    RuleFor(x => x.FamilyMemberId).GreaterThan(0);
  }
}

public sealed class GetPersonalRecordsMapper
  : Mapper<GetPersonalRecordsRequest, List<PersonalRecordRecord>, IReadOnlyList<PersonalRecordDto>>
{
  public override List<PersonalRecordRecord> FromEntity(IReadOnlyList<PersonalRecordDto> e)
    => e.Select(pr => new PersonalRecordRecord(pr.Lift.Name, pr.WeightKg.Value, pr.AchievedOn, pr.WorkoutSessionId.Value)).ToList();
}
