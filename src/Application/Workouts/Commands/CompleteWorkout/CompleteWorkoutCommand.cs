using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Workouts.Commands.CompleteWorkout;

[Authorize]
public record CompleteWorkoutCommand(int Id) : IRequest;

public class CompleteWorkoutCommandHandler : IRequestHandler<CompleteWorkoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public CompleteWorkoutCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(CompleteWorkoutCommand request, CancellationToken cancellationToken)
    {
        var session = await _context.WorkoutSessions
            .Include(s => s.Exercises)
                .ThenInclude(e => e.Sets)
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, session);

        session.Status = WorkoutStatus.Completed;

        // Update personal records for completed working sets
        foreach (var workoutExercise in session.Exercises)
        {
            var completedSets = workoutExercise.Sets
                .Where(s => s.IsCompleted && s.SetType == SetType.WorkingSet && s.WeightKg > 0)
                .ToList();

            foreach (var set in completedSets)
            {
                int repsCompleted = set.CompletedReps ?? set.Reps;
                // Epley formula for estimated 1RM
                decimal estimated1Rm = repsCompleted == 1
                    ? set.WeightKg
                    : set.WeightKg * (1 + repsCompleted / 30m);

                var existingPr = await _context.PersonalRecords
                    .FirstOrDefaultAsync(pr => pr.UserId == _currentUser.Id && pr.ExerciseId == workoutExercise.ExerciseId, cancellationToken);

                if (existingPr == null || estimated1Rm > existingPr.Estimated1RmKg)
                {
                    if (existingPr == null)
                    {
                        existingPr = new PersonalRecord
                        {
                            UserId = _currentUser.Id!,
                            ExerciseId = workoutExercise.ExerciseId
                        };
                        _context.PersonalRecords.Add(existingPr);
                    }

                    existingPr.WeightKg = set.WeightKg;
                    existingPr.Reps = repsCompleted;
                    existingPr.AchievedAt = session.Date;
                    existingPr.Estimated1RmKg = Math.Round(estimated1Rm, 2);
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
