using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.Lifts;

namespace LiftAndShift.UseCases.WorkWeights.Get;

public record GetWorkWeightQuery(FamilyMemberId FamilyMemberId, Lift Lift) : IQuery<Result<WorkWeightDto>>;
