using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;
using LiftAndShift.Core.WorkWeightAggregate;

namespace LiftAndShift.UseCases.WorkWeights.WarmUp;

public record GetWarmUpSetsQuery(FamilyMemberId FamilyMemberId, Lift Lift) : IQuery<Result<IReadOnlyList<WarmUpSet>>>;
