using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.UseCases.BodyMetrics.Log;

public record LogBodyMetricCommand(
  FamilyMemberId FamilyMemberId,
  DateOnly RecordedOn,
  WeightKg BodyWeightKg) : ICommand<Result<BodyMetricDto>>;
