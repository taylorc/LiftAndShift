using LiftAndShift.Core.Lifts;

namespace LiftAndShift.UseCases.Lifts.List;

public class ListLiftsHandler : IQueryHandler<ListLiftsQuery, IReadOnlyList<LiftDto>>
{
  public ValueTask<IReadOnlyList<LiftDto>> Handle(ListLiftsQuery request, CancellationToken cancellationToken)
  {
    IReadOnlyList<LiftDto> lifts = Lift.List
      .Select(lift => new LiftDto(lift.Value, lift.Name, lift.OpeningWeightKg, lift.IncrementKg, lift.WorkSetCount))
      .ToList();

    return ValueTask.FromResult(lifts);
  }
}
