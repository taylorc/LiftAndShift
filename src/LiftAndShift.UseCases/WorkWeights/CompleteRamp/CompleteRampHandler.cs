using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.FamilyMemberAggregate.Specifications;
using LiftAndShift.Core.WorkWeightAggregate;
using LiftAndShift.Core.WorkWeightAggregate.Specifications;

namespace LiftAndShift.UseCases.WorkWeights.CompleteRamp;

public class CompleteRampHandler(IRepository<WorkWeight> _repository, IReadRepository<FamilyMember> _familyMemberRepository)
  : ICommandHandler<CompleteRampCommand, Result<WorkWeightDto>>
{
  public async ValueTask<Result<WorkWeightDto>> Handle(CompleteRampCommand command, CancellationToken ct)
  {
    var spec = new WorkWeightByFamilyMemberIdAndLiftSpec(command.FamilyMemberId, command.Lift);
    var existing = await _repository.FirstOrDefaultAsync(spec, ct);
    if (existing != null)
    {
      return AlreadyRampedResult(command.Lift);
    }

    var familyMember = await _familyMemberRepository.FirstOrDefaultAsync(new FamilyMemberByIdSpec(command.FamilyMemberId), ct);
    if (familyMember == null) return Result.NotFound();

    var workWeight = WorkWeight.CompleteRamp(command.FamilyMemberId, command.Lift, command.FinalSetNumber);

    try
    {
      await _repository.AddAsync(workWeight, ct);
    }
    catch (Exception)
    {
      // A concurrent request may have Ramped this lift between our check above and this insert;
      // the unique (FamilyMemberId, Lift) index rejects the second insert. Treat that as the
      // same "already Ramped" failure rather than surfacing an unhandled 500. Any other cause
      // of failure is not our concern here, so it's rethrown.
      var conflicting = await _repository.FirstOrDefaultAsync(spec, ct);
      if (conflicting != null)
      {
        return AlreadyRampedResult(command.Lift);
      }

      throw;
    }

    return new WorkWeightDto(workWeight.Id, workWeight.FamilyMemberId, workWeight.Lift, workWeight.WeightKg, workWeight.ConsecutiveFailures);
  }

  private static Result<WorkWeightDto> AlreadyRampedResult(Core.Lifts.Lift lift) =>
    Result.Invalid(new ValidationError
    {
      Identifier = nameof(CompleteRampCommand.Lift),
      ErrorMessage = $"{lift.Name} has already been Ramped for this family member."
    });
}
