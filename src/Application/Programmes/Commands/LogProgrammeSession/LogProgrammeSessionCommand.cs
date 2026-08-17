using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Application.Calculators;
using LiftAndShift.Application.Workouts.Commands.LogWorkout;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Programmes.Commands.LogProgrammeSession;

[Authorize]
public record LogProgrammeSessionCommand : IRequest<int>
{
    public int UserProgrammeId { get; init; }
    public int ProgrammeSessionId { get; init; }
    public List<LogWorkoutExerciseDto> Exercises { get; init; } = new();
}

public class LogProgrammeSessionCommandHandler : IRequestHandler<LogProgrammeSessionCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public LogProgrammeSessionCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(LogProgrammeSessionCommand request, CancellationToken cancellationToken)
    {
        var programme = await _context.UserProgrammes
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == request.UserProgrammeId && p.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.UserProgrammeId, programme);

        var programmeSession = programme.Sessions
            .FirstOrDefault(s => s.Id == request.ProgrammeSessionId);

        Guard.Against.NotFound(request.ProgrammeSessionId, programmeSession);

        // Create the workout session
        var workoutSession = new WorkoutSession
        {
            UserId = _currentUser.Id!,
            Date = DateTimeOffset.UtcNow,
            Status = WorkoutStatus.Completed,
            IsProgrammeSession = true,
            ProgrammeSessionId = request.ProgrammeSessionId
        };

        foreach (var ex in request.Exercises)
        {
            var workoutExercise = new WorkoutExercise
            {
                ExerciseId = ex.ExerciseId,
                OrderIndex = ex.OrderIndex,
                Notes = ex.Notes
            };

            foreach (var set in ex.Sets)
            {
                workoutExercise.Sets.Add(new WorkoutSet
                {
                    SetNumber = set.SetNumber,
                    SetType = set.SetType,
                    WeightKg = set.WeightKg,
                    Reps = set.Reps,
                    CompletedReps = set.CompletedReps,
                    Notes = set.Notes,
                    IsCompleted = set.IsCompleted
                });
            }

            workoutSession.Exercises.Add(workoutExercise);
        }

        _context.WorkoutSessions.Add(workoutSession);

        // Save the workout session first so it gets its database-generated Id
        await _context.SaveChangesAsync(cancellationToken);

        // Apply progression and create next session
        programmeSession.CompletedDate = DateTimeOffset.UtcNow;
        programmeSession.WorkoutSessionId = workoutSession.Id;

        // Resolve exercise ids to lift names so we can match logged sets against LiftProgression/ConsecutiveFailures keys
        var exerciseIds = request.Exercises.Select(e => e.ExerciseId).Distinct().ToList();
        var liftNamesByExerciseId = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);

        // Only lifts actually trained this session (with evaluable working sets) get their weight/failure-count
        // updated; everything else carries forward unchanged.
        var newWeights = new Dictionary<string, decimal>(programmeSession.LiftProgression);
        var newFailures = new Dictionary<string, int>(programmeSession.ConsecutiveFailures);

        foreach (var ex in request.Exercises)
        {
            if (!liftNamesByExerciseId.TryGetValue(ex.ExerciseId, out var liftName) ||
                !newWeights.TryGetValue(liftName, out var currentWeight))
            {
                continue;
            }

            var outcome = DetermineOutcome(ex);
            if (outcome == LiftOutcome.Skip)
            {
                continue;
            }

            var previousFailures = newFailures.TryGetValue(liftName, out var pf) ? pf : 0;
            var failuresForThisSession = outcome == LiftOutcome.Success ? 0 : previousFailures + 1;

            newWeights[liftName] = StartingStrengthProgressionService.NextWeight(liftName, currentWeight, failuresForThisSession);
            newFailures[liftName] = StartingStrengthProgressionService.ShouldDeload(failuresForThisSession) ? 0 : failuresForThisSession;
        }

        // Determine next workout type
        var nextType = programmeSession.WorkoutType == WorkoutType.A ? WorkoutType.B : WorkoutType.A;

        var nextSession = new ProgrammeSession
        {
            UserProgrammeId = programme.Id,
            WorkoutType = nextType,
            ScheduledDate = DateTimeOffset.UtcNow.AddDays(2),
            LiftProgression = newWeights,
            ConsecutiveFailures = newFailures
        };

        programme.Sessions.Add(nextSession);
        programme.SessionCount++;
        programme.CurrentWorkoutType = nextType;

        await _context.SaveChangesAsync(cancellationToken);

        return workoutSession.Id;
    }

    private enum LiftOutcome { Success, Failure, Skip }

    /// <summary>
    /// A lift succeeds when every evaluable working set (non-null CompletedReps) met its programmed Reps target,
    /// fails when any evaluable working set fell short, and is skipped (untrained) when there are no evaluable
    /// working sets at all. Warm-up sets and sets with null CompletedReps are excluded from evaluation.
    /// </summary>
    private static LiftOutcome DetermineOutcome(LogWorkoutExerciseDto exercise)
    {
        var evaluableSets = exercise.Sets
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

public class LogProgrammeSessionCommandValidator : AbstractValidator<LogProgrammeSessionCommand>
{
    public LogProgrammeSessionCommandValidator()
    {
        RuleFor(x => x.UserProgrammeId).GreaterThan(0);
        RuleFor(x => x.ProgrammeSessionId).GreaterThan(0);
    }
}
