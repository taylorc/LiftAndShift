using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.ProgrammeAggregate;

namespace LiftAndShift.UseCases.FamilyMembers.Create;

public class CreateFamilyMemberHandler(IRepository<FamilyMember> _familyMemberRepository, IRepository<Programme> _programmeRepository)
  : ICommandHandler<CreateFamilyMemberCommand, Result<FamilyMemberId>>
{
  public async ValueTask<Result<FamilyMemberId>> Handle(CreateFamilyMemberCommand command,
    CancellationToken cancellationToken)
  {
    var newFamilyMember = new FamilyMember(command.Name, command.Pin);
    var createdItem = await _familyMemberRepository.AddAsync(newFamilyMember, cancellationToken);

    var newProgramme = new Programme(createdItem.Id, TrainingPhase.From(TrainingPhase.Min));
    await _programmeRepository.AddAsync(newProgramme, cancellationToken);

    return createdItem.Id;
  }
}
