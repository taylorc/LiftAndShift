using LiftAndShift.Application.Calculators;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Programmes.Progression;

public enum LiftOutcome { Success, Failure, Skip }

public sealed record LoggedSet(SetType SetType, int? CompletedReps, int Reps);

public sealed record LoggedLift(string LiftName, IReadOnlyList<LoggedSet> Sets);

public sealed record ProgressionState(Dictionary<string, decimal> Weights, Dictionary<string, int> Failures);

/// <summary>
/// The Starting Strength carry-forward fold: a session's prescribed weights and consecutive-failure
/// counts are derived from the previous session's values plus that session's logged outcome. This is
/// the single implementation used both when appending a freshly logged session and when replaying the
/// chain after an earlier session is edited.
/// </summary>
public static class ProgrammeProgressionRecalculator
{
    /// <summary>
    /// Given a session's prescribed state (working weight and consecutive-failure count per lift) and
    /// the sets logged against that session, returns the next session's prescribed state. Lifts absent
    /// from <paramref name="currentWeights"/>, and lifts with no evaluable working sets logged, carry
    /// forward unchanged.
    /// </summary>
    public static ProgressionState NextSessionState(
        IReadOnlyDictionary<string, decimal> currentWeights,
        IReadOnlyDictionary<string, int> currentFailures,
        IEnumerable<LoggedLift> loggedLifts)
    {
        var weights = new Dictionary<string, decimal>(currentWeights);
        var failures = new Dictionary<string, int>(currentFailures);

        foreach (var lift in loggedLifts)
        {
            if (!weights.TryGetValue(lift.LiftName, out var currentWeight))
            {
                continue;
            }

            var outcome = DetermineOutcome(lift.Sets);
            if (outcome == LiftOutcome.Skip)
            {
                continue;
            }

            var previousFailures = failures.TryGetValue(lift.LiftName, out var pf) ? pf : 0;
            var failuresForThisSession = outcome == LiftOutcome.Success ? 0 : previousFailures + 1;

            weights[lift.LiftName] = StartingStrengthProgressionService.NextWeight(lift.LiftName, currentWeight, failuresForThisSession);
            failures[lift.LiftName] = StartingStrengthProgressionService.ShouldDeload(failuresForThisSession) ? 0 : failuresForThisSession;
        }

        return new ProgressionState(weights, failures);
    }

    /// <summary>
    /// A lift succeeds when every evaluable working set (non-null <see cref="LoggedSet.CompletedReps"/>)
    /// met its programmed <see cref="LoggedSet.Reps"/> target, fails when any evaluable working set fell
    /// short, and is skipped (untrained) when there are no evaluable working sets at all. Warm-up sets
    /// and sets with null CompletedReps are excluded from evaluation.
    /// </summary>
    public static LiftOutcome DetermineOutcome(IEnumerable<LoggedSet> sets)
    {
        var evaluableSets = sets
            .Where(s => s.SetType == SetType.WorkingSet && s.CompletedReps.HasValue)
            .ToList();

        if (evaluableSets.Count == 0)
        {
            return LiftOutcome.Skip;
        }

        return evaluableSets.All(s => s.CompletedReps!.Value >= s.Reps)
            ? LiftOutcome.Success
            : LiftOutcome.Failure;
    }
}
