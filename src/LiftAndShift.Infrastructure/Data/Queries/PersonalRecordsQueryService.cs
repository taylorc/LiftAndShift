using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.UseCases.PersonalRecords;
using LiftAndShift.UseCases.PersonalRecords.Get;

namespace LiftAndShift.Infrastructure.Data.Queries;

/// <summary>
/// Computes Personal Records in the database rather than materializing every WorkoutSession/LoggedSet
/// a family member has ever logged: one GroupBy per (session, lift) filters to successful lift
/// performances server-side (reusing <see cref="WorkoutSession.RequiredRepsPerSet"/>, the same
/// threshold <see cref="WorkoutSession.WasSuccessful"/> uses), and only that already-small,
/// already-filtered set is brought back to pick the heaviest per lift.
/// </summary>
public class PersonalRecordsQueryService(AppDbContext db) : IPersonalRecordsQueryService
{
  public async Task<IReadOnlyList<PersonalRecordDto>> GetAsync(FamilyMemberId familyMemberId, CancellationToken cancellationToken)
  {
    var successfulLiftPerformances = await db.WorkoutSessions
      .Where(session => session.FamilyMemberId == familyMemberId)
      .SelectMany(session => session.LoggedSets, (session, set) => new { session.Id, session.PerformedOn, set.Lift, set.WeightKg, set.RepsAchieved })
      .GroupBy(row => new { row.Id, row.Lift })
      .Where(g => g.Min(row => row.RepsAchieved) >= WorkoutSession.RequiredRepsPerSet)
      .Select(g => new PersonalRecordDto(
        g.Key.Lift,
        g.Max(row => row.WeightKg),
        g.Max(row => row.PerformedOn),
        g.Key.Id))
      .ToListAsync(cancellationToken);

    return successfulLiftPerformances
      .GroupBy(record => record.Lift)
      .Select(group => group
        .OrderByDescending(record => record.WeightKg.Value)
        .ThenBy(record => record.AchievedOn)
        .First())
      .ToList();
  }
}
