using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Workouts.Queries.GetExerciseProgress;

public record ExerciseProgressPointDto
{
    public DateTimeOffset Date { get; init; }
    public decimal MaxWeightKg { get; init; }
    public decimal Estimated1Rm { get; init; }
    public decimal TotalVolumeKg { get; init; }
}

[Authorize]
public record GetExerciseProgressQuery(int ExerciseId) : IRequest<List<ExerciseProgressPointDto>>;

public class GetExerciseProgressQueryHandler : IRequestHandler<GetExerciseProgressQuery, List<ExerciseProgressPointDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetExerciseProgressQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ExerciseProgressPointDto>> Handle(GetExerciseProgressQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _context.WorkoutSessions
            .Include(s => s.Exercises)
                .ThenInclude(e => e.Sets)
            .Where(s => s.UserId == _currentUser.Id
                && s.Status == WorkoutStatus.Completed
                && s.Exercises.Any(e => e.ExerciseId == request.ExerciseId))
            .OrderBy(s => s.Date)
            .ToListAsync(cancellationToken);

        return sessions.Select(s =>
        {
            var relevantSets = s.Exercises
                .Where(e => e.ExerciseId == request.ExerciseId)
                .SelectMany(e => e.Sets)
                .Where(set => set.IsCompleted && set.SetType == SetType.WorkingSet && set.WeightKg > 0)
                .ToList();

            decimal maxWeight = relevantSets.Any() ? relevantSets.Max(set => set.WeightKg) : 0;
            int maxReps = relevantSets.Any() ? relevantSets.Where(set => set.WeightKg == maxWeight).Max(set => set.CompletedReps ?? set.Reps) : 0;
            decimal est1Rm = maxReps == 0 ? 0
                : maxReps == 1 ? maxWeight
                : Math.Round(maxWeight * (1 + maxReps / 30m), 2);
            decimal totalVolume = relevantSets.Sum(set => set.WeightKg * (set.CompletedReps ?? set.Reps));

            return new ExerciseProgressPointDto
            {
                Date = s.Date,
                MaxWeightKg = maxWeight,
                Estimated1Rm = est1Rm,
                TotalVolumeKg = totalVolume
            };
        }).ToList();
    }
}
