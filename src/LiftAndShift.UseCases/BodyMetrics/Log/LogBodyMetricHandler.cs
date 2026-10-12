using LiftAndShift.Core.BodyMetricAggregate;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.FamilyMemberAggregate.Specifications;

namespace LiftAndShift.UseCases.BodyMetrics.Log;

public class LogBodyMetricHandler(IRepository<BodyMetric> _repository, IReadRepository<FamilyMember> _familyMemberRepository)
  : ICommandHandler<LogBodyMetricCommand, Result<BodyMetricDto>>
{
  public async ValueTask<Result<BodyMetricDto>> Handle(LogBodyMetricCommand command, CancellationToken ct)
  {
    var familyMember = await _familyMemberRepository.FirstOrDefaultAsync(new FamilyMemberByIdSpec(command.FamilyMemberId), ct);
    if (familyMember == null) return Result.NotFound();

    var bodyMetric = new BodyMetric(command.FamilyMemberId, command.RecordedOn, command.BodyWeightKg);
    await _repository.AddAsync(bodyMetric, ct);

    return BodyMetricDto.FromEntity(bodyMetric);
  }
}
