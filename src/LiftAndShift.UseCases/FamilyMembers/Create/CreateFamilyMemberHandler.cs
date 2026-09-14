using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.FamilyMembers.Create;

public class CreateFamilyMemberHandler(IRepository<FamilyMember> _repository)
  : ICommandHandler<CreateFamilyMemberCommand, Result<FamilyMemberId>>
{
  public async ValueTask<Result<FamilyMemberId>> Handle(CreateFamilyMemberCommand command,
    CancellationToken cancellationToken)
  {
    var newFamilyMember = new FamilyMember(command.Name, command.Pin);
    var createdItem = await _repository.AddAsync(newFamilyMember, cancellationToken);

    return createdItem.Id;
  }
}
