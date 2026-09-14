using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.ProgrammeAggregate;

namespace LiftAndShift.UseCases.Programmes;
public record ProgrammeDto(ProgrammeId Id, FamilyMemberId FamilyMemberId, TrainingPhase TrainingPhase);
