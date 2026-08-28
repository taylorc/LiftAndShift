using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Application.Programmes.Progression;
using LiftAndShift.Application.Workouts.Commands.LogWorkout;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Programmes.Commands.EditProgrammeSession;

[Authorize]
public record EditProgrammeSessionCommand : IRequest
{
    public int UserProgrammeId { get; init; }
    public int ProgrammeSessionId { get; init; }
    public List<LogWorkoutExerciseDto> Exercises { get; init; } = new();
}

public class EditProgrammeSessionCommandHandler : IRequestHandler<EditProgrammeSessionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public EditProgrammeSessionCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(EditProgrammeSessionCommand request, CancellationToken cancellationToken)
    {
        var programme = await _context.UserProgrammes
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == request.UserProgrammeId && p.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.UserProgrammeId, programme);

        // Only a logged session (completed, with a linked workout) can be edited.
        var logged = programme.Sessions
            .FirstOrDefault(s => s.Id == request.ProgrammeSessionId && s.CompletedDate != null && s.WorkoutSessionId != null);

        Guard.Against.NotFound(request.ProgrammeSessionId, logged);

        var workout = await _context.WorkoutSessions
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .FirstAsync(w => w.Id == logged.WorkoutSessionId!.Value, cancellationToken);

        ReplaceSetData(workout, request.Exercises);

        await ReplayFrom(programme, logged, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private void ReplaceSetData(WorkoutSession workout, IReadOnlyCollection<LogWorkoutExerciseDto> exercises)
    {
        _context.WorkoutSets.RemoveRange(workout.Exercises.SelectMany(e => e.Sets));
        _context.WorkoutExercises.RemoveRange(workout.Exercises);
        workout.Exercises.Clear();

        foreach (var ex in exercises)
        {
            var workoutExercise = new WorkoutExercise
            {
                ExerciseId = ex.ExerciseId,
                OrderIndex = ex.OrderIndex,
                Notes = ex.Notes,
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
                    IsCompleted = set.IsCompleted,
                });
            }

            workout.Exercises.Add(workoutExercise);
        }
    }

    /// <summary>
    /// Re-derives <see cref="ProgrammeSession.LiftProgression"/> and
    /// <see cref="ProgrammeSession.ConsecutiveFailures"/> for every session after
    /// <paramref name="edited"/>, folding forward from the edited session's own prescription
    /// through each later session's logged outcome. The edited session's own prescription is
    /// left untouched.
    /// </summary>
    private async Task ReplayFrom(UserProgramme programme, ProgrammeSession edited, CancellationToken cancellationToken)
    {
        var ordered = programme.Sessions.OrderBy(s => s.Id).ToList();
        var editIndex = ordered.FindIndex(s => s.Id == edited.Id);
        if (editIndex == ordered.Count - 1)
        {
            return; // nothing downstream to replay
        }

        // The edited session's workout is already updated in memory; load the rest.
        var downstreamWorkoutIds = ordered.Skip(editIndex + 1)
            .Where(s => s.WorkoutSessionId != null)
            .Select(s => s.WorkoutSessionId!.Value)
            .ToList();

        var editedWorkout = await _context.WorkoutSessions
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .FirstAsync(w => w.Id == edited.WorkoutSessionId!.Value, cancellationToken);

        var workoutsBySessionId = new Dictionary<int, WorkoutSession> { [edited.Id] = editedWorkout };
        if (downstreamWorkoutIds.Count > 0)
        {
            var loadedWorkouts = await _context.WorkoutSessions
                .Include(w => w.Exercises).ThenInclude(e => e.Sets)
                .Where(w => downstreamWorkoutIds.Contains(w.Id))
                .ToListAsync(cancellationToken);

            foreach (var w in loadedWorkouts)
            {
                workoutsBySessionId[w.ProgrammeSessionId!.Value] = w;
            }
        }

        var exerciseIds = workoutsBySessionId.Values
            .SelectMany(w => w.Exercises.Select(e => e.ExerciseId))
            .Distinct()
            .ToList();
        var liftNames = await _context.Exercises
            .Where(e => exerciseIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.Name, cancellationToken);

        var weights = new Dictionary<string, decimal>(ordered[editIndex].LiftProgression);
        var failures = new Dictionary<string, int>(ordered[editIndex].ConsecutiveFailures);

        for (var i = editIndex; i < ordered.Count - 1; i++)
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

public class EditProgrammeSessionCommandValidator : AbstractValidator<EditProgrammeSessionCommand>
{
    public EditProgrammeSessionCommandValidator()
    {
        RuleFor(x => x.UserProgrammeId).GreaterThan(0);
        RuleFor(x => x.ProgrammeSessionId).GreaterThan(0);

        RuleForEach(x => x.Exercises).ChildRules(exercise =>
        {
            exercise.RuleForEach(e => e.Sets).ChildRules(set =>
            {
                set.RuleFor(s => s.CompletedReps)
                    .NotNull()
                    .When(s => s.SetType == SetType.WorkingSet)
                    .WithMessage("Every working set must have its completed reps filled in.");

                set.RuleFor(s => s.IsCompleted)
                    .Equal(true)
                    .When(s => s.CompletedReps.HasValue)
                    .WithMessage("A set with completed reps entered must be marked done.");
            });
        });
    }
}
