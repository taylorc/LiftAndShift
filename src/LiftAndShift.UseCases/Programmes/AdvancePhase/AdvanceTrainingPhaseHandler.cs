using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.ProgrammeAggregate.Specifications;

namespace LiftAndShift.UseCases.Programmes.AdvancePhase;

public class AdvanceTrainingPhaseHandler(IRepository<Programme> _repository)
  : ICommandHandler<AdvanceTrainingPhaseCommand, Result<ProgrammeDto>>
{
  public async ValueTask<Result<ProgrammeDto>> Handle(AdvanceTrainingPhaseCommand command, CancellationToken ct)
  {
    var spec = new ProgrammeByFamilyMemberIdSpec(command.FamilyMemberId);
    var programme = await _repository.FirstOrDefaultAsync(spec, ct);
    if (programme == null) return Result.NotFound();

    var advanceResult = programme.AdvancePhase();
    if (!advanceResult.IsSuccess)
    {
      return Result.Invalid(advanceResult.ValidationErrors.ToArray());
    }

    await _repository.UpdateAsync(programme, ct);

    return new ProgrammeDto(programme.Id, programme.FamilyMemberId, programme.TrainingPhase);
  }
}
