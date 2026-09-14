using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.Programmes.Get;

public record GetProgrammeQuery(FamilyMemberId FamilyMemberId) : IQuery<Result<ProgrammeDto>>;
