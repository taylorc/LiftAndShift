using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;

namespace LiftAndShift.Application.Programmes.Queries.GetProgrammeSessions;

public record LoggedSessionSetDto
{
    public int SetNumber { get; init; }
    public string SetType { get; init; } = string.Empty;
    public decimal WeightKg { get; init; }
    public int Reps { get; init; }
    public int? CompletedReps { get; init; }
    public string? Notes { get; init; }
    public bool IsCompleted { get; init; }
}

public record LoggedSessionExerciseDto
{
    public int ExerciseId { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public string? Notes { get; init; }
    public List<LoggedSessionSetDto> Sets { get; init; } = new();
}

public record LoggedProgrammeSessionDto
{
    public int SessionId { get; init; }
    public int WorkoutSessionId { get; init; }
    public string WorkoutType { get; init; } = string.Empty;
    public DateTimeOffset ScheduledDate { get; init; }
    public DateTimeOffset CompletedDate { get; init; }
    public List<LoggedSessionExerciseDto> Exercises { get; init; } = new();
}

[Authorize]
public record GetProgrammeSessionsQuery(int UserProgrammeId) : IRequest<List<LoggedProgrammeSessionDto>>;

public class GetProgrammeSessionsQueryHandler : IRequestHandler<GetProgrammeSessionsQuery, List<LoggedProgrammeSessionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetProgrammeSessionsQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<List<LoggedProgrammeSessionDto>> Handle(GetProgrammeSessionsQuery request, CancellationToken cancellationToken)
    {
        var programme = await _context.UserProgrammes
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == request.UserProgrammeId && p.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.UserProgrammeId, programme);

        var loggedSessions = programme.Sessions
            .Where(s => s.CompletedDate != null && s.WorkoutSessionId != null)
            .OrderBy(s => s.Id)
            .ToList();

        if (loggedSessions.Count == 0)
        {
            return new List<LoggedProgrammeSessionDto>();
        }

        var workoutSessionIds = loggedSessions.Select(s => s.WorkoutSessionId!.Value).ToList();
        var workoutsById = await _context.WorkoutSessions
            .Include(w => w.Exercises).ThenInclude(e => e.Exercise)
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .Where(w => workoutSessionIds.Contains(w.Id))
            .ToDictionaryAsync(w => w.Id, cancellationToken);

        return loggedSessions
            .Where(s => workoutsById.ContainsKey(s.WorkoutSessionId!.Value))
            .Select(s =>
            {
                var workout = workoutsById[s.WorkoutSessionId!.Value];
                return new LoggedProgrammeSessionDto
                {
                    SessionId = s.Id,
                    WorkoutSessionId = workout.Id,
                    WorkoutType = s.WorkoutType.ToString(),
                    ScheduledDate = s.ScheduledDate,
                    CompletedDate = s.CompletedDate!.Value,
                    Exercises = workout.Exercises
                        .OrderBy(e => e.OrderIndex)
                        .Select(e => new LoggedSessionExerciseDto
                        {
                            ExerciseId = e.ExerciseId,
                            ExerciseName = e.Exercise.Name,
                            OrderIndex = e.OrderIndex,
                            Notes = e.Notes,
                            Sets = e.Sets
                                .OrderBy(set => set.SetNumber)
                                .Select(set => new LoggedSessionSetDto
                                {
                                    SetNumber = set.SetNumber,
                                    SetType = set.SetType.ToString(),
                                    WeightKg = set.WeightKg,
                                    Reps = set.Reps,
                                    CompletedReps = set.CompletedReps,
                                    Notes = set.Notes,
                                    IsCompleted = set.IsCompleted,
                                })
                                .ToList(),
                        })
                        .ToList(),
                };
            })
            .ToList();
    }
}
