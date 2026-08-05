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
    public Dictionary<string, int> ConsecutiveFailures { get; init; } = new();
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

        // Calculate new weights with progression
        var newWeights = new Dictionary<string, decimal>(programmeSession.LiftProgression);
        foreach (var (liftName, currentWeight) in programmeSession.LiftProgression)
        {
            int failures = request.ConsecutiveFailures.TryGetValue(liftName, out var f) ? f : 0;
            newWeights[liftName] = StartingStrengthProgressionService.NextWeight(liftName, currentWeight, failures);
        }

        // Determine next workout type
        var nextType = programmeSession.WorkoutType == WorkoutType.A ? WorkoutType.B : WorkoutType.A;

        var nextSession = new ProgrammeSession
        {
            UserProgrammeId = programme.Id,
            WorkoutType = nextType,
            ScheduledDate = DateTimeOffset.UtcNow.AddDays(2),
            LiftProgression = newWeights
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
    }
}
