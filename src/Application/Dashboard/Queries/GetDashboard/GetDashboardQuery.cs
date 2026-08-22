using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Dashboard.Queries.GetDashboard;

public record PersonalRecordSummaryDto
{
    public int ExerciseId { get; init; }
    public string ExerciseName { get; init; } = string.Empty;
    public decimal WeightKg { get; init; }
    public int Reps { get; init; }
    public decimal Estimated1RmKg { get; init; }
    public DateTimeOffset AchievedAt { get; init; }
}

public record DashboardDto
{
    public int SessionsThisWeek { get; init; }
    public int SessionsThisMonth { get; init; }
    public int CurrentStreak { get; init; }
    public List<PersonalRecordSummaryDto> PersonalRecords { get; init; } = new();
    public bool HasActiveProgramme { get; init; }
    public int? NextProgrammeSessionId { get; init; }
    public string? NextWorkoutType { get; init; }
    public DateTimeOffset? NextSessionDate { get; init; }
}

[Authorize]
public record GetDashboardQuery : IRequest<DashboardDto>;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetDashboardQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var weekStart = now.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);

        var sessions = await _context.WorkoutSessions
            .Where(s => s.UserId == _currentUser.Id && s.Status == WorkoutStatus.Completed)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);

        int sessionsThisWeek = sessions.Count(s => s.Date >= weekStart);
        int sessionsThisMonth = sessions.Count(s => s.Date >= monthStart);

        // Calculate streak (consecutive days with at least 1 session)
        int streak = 0;
        var today = now.Date;
        var sessionDates = sessions.Select(s => s.Date.Date).Distinct().OrderByDescending(d => d).ToList();

        for (int i = 0; i < sessionDates.Count; i++)
        {
            var expected = today.AddDays(-i);
            if (sessionDates.Count > i && sessionDates[i] == expected)
                streak++;
            else
                break;
        }

        var prs = await _context.PersonalRecords
            .Include(pr => pr.Exercise)
            .Where(pr => pr.UserId == _currentUser.Id)
            .OrderBy(pr => pr.Exercise.Name)
            .ToListAsync(cancellationToken);

        var activeProgramme = await _context.UserProgrammes
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.Id && p.Status == ProgrammeStatus.Active, cancellationToken);

        var nextSession = activeProgramme?.Sessions
            .Where(s => s.CompletedDate == null)
            .OrderBy(s => s.ScheduledDate)
            .FirstOrDefault();

        return new DashboardDto
        {
            SessionsThisWeek = sessionsThisWeek,
            SessionsThisMonth = sessionsThisMonth,
            CurrentStreak = streak,
            PersonalRecords = prs.Select(pr => new PersonalRecordSummaryDto
            {
                ExerciseId = pr.ExerciseId,
                ExerciseName = pr.Exercise.Name,
                WeightKg = pr.WeightKg,
                Reps = pr.Reps,
                Estimated1RmKg = pr.Estimated1RmKg,
                AchievedAt = pr.AchievedAt
            }).ToList(),
            HasActiveProgramme = activeProgramme != null,
            NextProgrammeSessionId = nextSession?.Id,
            NextWorkoutType = nextSession?.WorkoutType.ToString(),
            NextSessionDate = nextSession?.ScheduledDate
        };
    }
}
