using LiftAndShift.Core.FamilyMemberAggregate;

namespace LiftAndShift.UseCases.FamilyMembers.Delete;

public class DeleteFamilyMemberHandler(IRepository<FamilyMember> _repository)
  : ICommandHandler<DeleteFamilyMemberCommand, Result>
{
  public async ValueTask<Result> Handle(DeleteFamilyMemberCommand request, CancellationToken cancellationToken)
  {
    var aggregateToDelete = await _repository.GetByIdAsync(request.FamilyMemberId, cancellationToken);
    if (aggregateToDelete == null) return Result.NotFound();

    await _repository.DeleteAsync(aggregateToDelete, cancellationToken);

    return Result.Success();
  }
}
