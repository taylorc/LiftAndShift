using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.FamilyMembers.Update;

public class UpdateFamilyMemberHandler(IRepository<FamilyMember> _repository)
  : ICommandHandler<UpdateFamilyMemberCommand, Result<FamilyMemberDto>>
{
  public async ValueTask<Result<FamilyMemberDto>> Handle(UpdateFamilyMemberCommand command,
    CancellationToken ct)
  {
    var existingFamilyMember = await _repository.GetByIdAsync(command.FamilyMemberId, ct);
    if (existingFamilyMember == null)
    {
      return Result.NotFound();
    }

    existingFamilyMember.UpdateName(command.NewName);
    existingFamilyMember.UpdatePin(command.NewPin);

    await _repository.UpdateAsync(existingFamilyMember, ct);

    return new FamilyMemberDto(existingFamilyMember.Id, existingFamilyMember.Name, existingFamilyMember.Pin);
  }
}
