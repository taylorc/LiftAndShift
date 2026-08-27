using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Application.Programmes.Progression;
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

    public async ValueTask<int> Handle(LogProgrammeSessionCommand request, CancellationToken cancellationToken)
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
        var loggedLifts = request.Exercises
            .Where(ex => liftNamesByExerciseId.ContainsKey(ex.ExerciseId))
            .Select(ex => new LoggedLift(
                liftNamesByExerciseId[ex.ExerciseId],
                ex.Sets.Select(s => new LoggedSet(s.SetType, s.CompletedReps, s.Reps)).ToList()))
            .ToList();

        var (newWeights, newFailures) = ProgrammeProgressionRecalculator.NextSessionState(
            programmeSession.LiftProgression,
            programmeSession.ConsecutiveFailures,
            loggedLifts);

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
}

public class LogProgrammeSessionCommandValidator : AbstractValidator<LogProgrammeSessionCommand>
{
    public LogProgrammeSessionCommandValidator()
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
                    .WithMessage("Every working set must have its completed reps filled in before the session can be completed.");

                set.RuleFor(s => s.IsCompleted)
                    .Equal(true)
                    .When(s => s.CompletedReps.HasValue)
                    .WithMessage("A set with completed reps entered must be marked done.");
            });
        });
    }
}
