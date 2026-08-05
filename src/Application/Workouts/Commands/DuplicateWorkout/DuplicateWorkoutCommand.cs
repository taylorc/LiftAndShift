using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Workouts.Commands.DuplicateWorkout;

[Authorize]
public record DuplicateWorkoutCommand(int Id) : IRequest<int>;

public class DuplicateWorkoutCommandHandler : IRequestHandler<DuplicateWorkoutCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public DuplicateWorkoutCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(DuplicateWorkoutCommand request, CancellationToken cancellationToken)
    {
        var source = await _context.WorkoutSessions
            .Include(s => s.Exercises)
                .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, source);

        var newSession = new WorkoutSession
        {
            UserId = _currentUser.Id!,
            Date = DateTimeOffset.UtcNow,
            Notes = source.Notes,
            Status = WorkoutStatus.Draft
        };

        foreach (var ex in source.Exercises)
        {
            var newExercise = new WorkoutExercise
            {
                ExerciseId = ex.ExerciseId,
                OrderIndex = ex.OrderIndex,
                Notes = ex.Notes
            };

            foreach (var set in ex.Sets)
            {
                newExercise.Sets.Add(new WorkoutSet
                {
                    SetNumber = set.SetNumber,
                    SetType = set.SetType,
                    WeightKg = set.WeightKg,
                    Reps = set.Reps,
                    IsCompleted = false
                });
            }

            newSession.Exercises.Add(newExercise);
        }

        _context.WorkoutSessions.Add(newSession);
        await _context.SaveChangesAsync(cancellationToken);

        return newSession.Id;
    }
}
