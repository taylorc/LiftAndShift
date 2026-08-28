using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Domain.Entities;

namespace LiftAndShift.Application.Programmes.Progression;

/// <summary>
/// Re-derives <see cref="ProgrammeSession.LiftProgression"/> and
/// <see cref="ProgrammeSession.ConsecutiveFailures"/> for every session that comes after a given
/// one in a programme, folding <see cref="ProgrammeProgressionRecalculator"/> forward from that
/// session's own (possibly just-edited) prescription through each later session's logged outcome.
/// The from-session's own prescription is left untouched. Callers must have loaded
/// <see cref="UserProgramme.Sessions"/> and are responsible for calling SaveChanges.
/// </summary>
public static class ProgrammeSessionChainReplayer
{
    public static async Task ReplayForwardAsync(
        IApplicationDbContext context,
        UserProgramme programme,
        int fromSessionId,
        CancellationToken cancellationToken)
    {
        var ordered = programme.Sessions.OrderBy(s => s.Id).ToList();
        var fromIndex = ordered.FindIndex(s => s.Id == fromSessionId);
        if (fromIndex < 0 || fromIndex == ordered.Count - 1)
        {
            return; // unknown session, or nothing downstream to replay
        }

        var workoutIds = ordered.Skip(fromIndex)
            .Where(s => s.WorkoutSessionId != null)
            .Select(s => s.WorkoutSessionId!.Value)
            .ToList();

        var workoutsBySessionId = new Dictionary<int, WorkoutSession>();
        if (workoutIds.Count > 0)
        {
            var workouts = await context.WorkoutSessions
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Where(w => workoutIds.Contains(w.Id))
                .ToListAsync(cancellationToken);

            foreach (var w in workouts)
            {
                workoutsBySessionId[w.ProgrammeSessionId!.Value] = w;
            }
        }

        var exerciseIds = workoutsBySessionId.Values
            .SelectMany(w => w.Exercises.Select(e => e.ExerciseId))
            .Distinct()
            .ToList();
        var liftNames = await context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);

        var weights = new Dictionary<string, decimal>(ordered[fromIndex].LiftProgression);
        var failures = new Dictionary<string, int>(ordered[fromIndex].ConsecutiveFailures);

        for (var i = fromIndex; i < ordered.Count - 1; i++)
        {
            var loggedLifts = workoutsBySessionId.TryGetValue(ordered[i].Id, out var w)
                ? ToLoggedLifts(w, liftNames)
                : new List<LoggedLift>();

            var nextState = ProgrammeProgressionRecalculator.NextSessionState(weights, failures, loggedLifts);

            ordered[i + 1].LiftProgression = nextState.Weights;
            ordered[i + 1].ConsecutiveFailures = nextState.Failures;

            weights = nextState.Weights;
            failures = nextState.Failures;
        }
    }

    private static List<LoggedLift> ToLoggedLifts(WorkoutSession workout, IReadOnlyDictionary<int, string> liftNames) =>
        workout.Exercises
            .Where(e => liftNames.ContainsKey(e.ExerciseId))
            .Select(e => new LoggedLift(
                liftNames[e.ExerciseId],
                e.Sets.Select(s => new LoggedSet(s.SetType, s.CompletedReps, s.Reps)).ToList()))
            .ToList();
}
