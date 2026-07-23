using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;

namespace LiftAndShift.Application.Workouts.Queries.GetWorkout;

public record WorkoutSetDetailDto
{
    public int Id { get; init; }
    public int SetNumber { get; init; }
    public string SetType { get; init; } = string.Empty;
    public decimal WeightKg { get; init; }
    public int Reps { get; init; }
    public int? CompletedReps { get; init; }
    public string? Notes { get; init; }
    public bool IsCompleted { get; init; }
}

public record WorkoutExerciseDetailDto
{
    public int Id { get; init; }
    public int ExerciseId { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public string? Notes { get; init; }
    public List<WorkoutSetDetailDto> Sets { get; init; } = new();
}

public record WorkoutDetailDto
{
    public int Id { get; init; }
    public DateTimeOffset Date { get; init; }
    public string? Notes { get; init; }
    public string Status { get; init; } = string.Empty;
    public bool IsProgrammeSession { get; init; }
    public List<WorkoutExerciseDetailDto> Exercises { get; init; } = new();
}

[Authorize]
public record GetWorkoutQuery(int Id) : IRequest<WorkoutDetailDto>;

public class GetWorkoutQueryHandler : IRequestHandler<GetWorkoutQuery, WorkoutDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetWorkoutQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<WorkoutDetailDto> Handle(GetWorkoutQuery request, CancellationToken cancellationToken)
    {
        var session = await _context.WorkoutSessions
            .Include(s => s.Exercises.OrderBy(e => e.OrderIndex))
                .ThenInclude(e => e.Exercise)
            .Include(s => s.Exercises)
                .ThenInclude(e => e.Sets.OrderBy(set => set.SetNumber))
            .FirstOrDefaultAsync(s => s.Id == request.Id && s.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, session);

        return new WorkoutDetailDto
        {
            Id = session.Id,
            Date = session.Date,
            Notes = session.Notes,
            Status = session.Status.ToString(),
            IsProgrammeSession = session.IsProgrammeSession,
            Exercises = session.Exercises.Select(e => new WorkoutExerciseDetailDto
            {
                Id = e.Id,
                ExerciseId = e.ExerciseId,
                ExerciseName = e.Exercise.Name,
                OrderIndex = e.OrderIndex,
                Notes = e.Notes,
                Sets = e.Sets.Select(s => new WorkoutSetDetailDto
                {
                    Id = s.Id,
                    SetNumber = s.SetNumber,
                    SetType = s.SetType.ToString(),
                    WeightKg = s.WeightKg,
                    Reps = s.Reps,
                    CompletedReps = s.CompletedReps,
                    Notes = s.Notes,
                    IsCompleted = s.IsCompleted
                }).ToList()
            }).ToList()
        };
    }
}
