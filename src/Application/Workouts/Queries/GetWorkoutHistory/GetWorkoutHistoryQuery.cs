using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;

namespace LiftAndShift.Application.Workouts.Queries.GetWorkoutHistory;

public record WorkoutHistoryItemDto
{
    public int Id { get; init; }
    public DateTimeOffset Date { get; init; }
    public string Status { get; init; } = string.Empty;
    public List<string> ExerciseNames { get; init; } = new();
    public decimal TotalVolumeKg { get; init; }
    public bool IsProgrammeSession { get; init; }
}

[Authorize]
public record GetWorkoutHistoryQuery : IRequest<List<WorkoutHistoryItemDto>>;

public class GetWorkoutHistoryQueryHandler : IRequestHandler<GetWorkoutHistoryQuery, List<WorkoutHistoryItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetWorkoutHistoryQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<WorkoutHistoryItemDto>> Handle(GetWorkoutHistoryQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _context.WorkoutSessions
            .Include(s => s.Exercises)
                .ThenInclude(e => e.Exercise)
            .Include(s => s.Exercises)
                .ThenInclude(e => e.Sets)
            .Where(s => s.UserId == _currentUser.Id)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);

        return sessions.Select(s => new WorkoutHistoryItemDto
        {
            Id = s.Id,
            Date = s.Date,
            Status = s.Status.ToString(),
            ExerciseNames = s.Exercises.Select(e => e.Exercise.Name).ToList(),
            TotalVolumeKg = s.Exercises.SelectMany(e => e.Sets)
                .Where(set => set.IsCompleted)
                .Sum(set => set.WeightKg * (set.CompletedReps ?? set.Reps)),
            IsProgrammeSession = s.IsProgrammeSession
        }).ToList();
    }
}
