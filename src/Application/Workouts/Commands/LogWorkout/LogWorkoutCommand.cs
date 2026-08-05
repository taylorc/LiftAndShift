using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Workouts.Commands.LogWorkout;

public record LogWorkoutSetDto
{
    public int SetNumber { get; init; }
    public SetType SetType { get; init; } = SetType.WorkingSet;
    public decimal WeightKg { get; init; }
    public int Reps { get; init; }
    public int? CompletedReps { get; init; }
    public string? Notes { get; init; }
    public bool IsCompleted { get; init; }
}

public record LogWorkoutExerciseDto
{
    public int ExerciseId { get; init; }
    public int OrderIndex { get; init; }
    public string? Notes { get; init; }
    public List<LogWorkoutSetDto> Sets { get; init; } = new();
}

[Authorize]
public record LogWorkoutCommand : IRequest<int>
{
    public DateTimeOffset Date { get; init; } = DateTimeOffset.UtcNow;
    public string? Notes { get; init; }
    public bool Complete { get; init; }
    public List<LogWorkoutExerciseDto> Exercises { get; init; } = new();
}

public class LogWorkoutCommandHandler : IRequestHandler<LogWorkoutCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public LogWorkoutCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(LogWorkoutCommand request, CancellationToken cancellationToken)
    {
        var session = new WorkoutSession
        {
            UserId = _currentUser.Id!,
            Date = request.Date,
            Notes = request.Notes,
            Status = request.Complete ? WorkoutStatus.Completed : WorkoutStatus.Draft
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

            session.Exercises.Add(workoutExercise);
        }

        _context.WorkoutSessions.Add(session);
        await _context.SaveChangesAsync(cancellationToken);

        return session.Id;
    }
}

public class LogWorkoutCommandValidator : AbstractValidator<LogWorkoutCommand>
{
    public LogWorkoutCommandValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.Exercises)
            .NotNull().WithMessage("Exercises list cannot be null.");
    }
}
