using LiftAndShift.UseCases.Lifts.List;

namespace LiftAndShift.Web.Lifts;

public class List(IMediator mediator) : EndpointWithoutRequest<IEnumerable<LiftRecord>>
{
  public override void Configure()
  {
    Get("/Lifts");
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "List the fixed Lift catalogue";
      s.Description = "Retrieves every Lift in the Programme along with its fixed Ramp opening weight and increment.";
      s.ResponseExamples[200] = new List<LiftRecord> { new(1, "Squat", 20, 5), new(4, "Deadlift", 70, 10) };

      s.Responses[200] = "Lift catalogue returned successfully";
    });

    Tags("Lifts");

    Description(builder => builder
      .Produces<IEnumerable<LiftRecord>>(200, "application/json"));
  }

  public override async Task HandleAsync(CancellationToken cancellationToken)
  {
    var lifts = await mediator.Send(new ListLiftsQuery(), cancellationToken);
    var response = lifts.Select(lift => new LiftRecord(lift.Id, lift.Name, lift.OpeningWeightKg, lift.IncrementKg));

    await Send.OkAsync(response, cancellationToken);
  }
}
