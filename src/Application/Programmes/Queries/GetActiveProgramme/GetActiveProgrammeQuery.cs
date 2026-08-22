using LiftAndShift.Application.Calculators;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Programmes.Queries.GetActiveProgramme;

public record WarmupSetDto
{
    public int SetNumber { get; init; }
    public int Reps { get; init; }
    public decimal WeightKg { get; init; }
}

public record PrescribedLiftDto
{
    public string LiftName { get; init; } = string.Empty;
    public decimal WeightKg { get; init; }
    public int Sets { get; init; }
    public int Reps { get; init; }
    public List<WarmupSetDto> WarmupSets { get; init; } = new();
}

public record NextProgrammeSessionDto
{
    public int SessionId { get; init; }
    public string WorkoutType { get; init; } = string.Empty;
    public DateTimeOffset ScheduledDate { get; init; }
    public List<PrescribedLiftDto> PrescribedLifts { get; init; } = new();
}

public record ActiveProgrammeDto
{
    public int Id { get; init; }
    public string ProgrammeTemplateId { get; init; } = string.Empty;
    public string ProgrammeName { get; init; } = string.Empty;
    public DateTimeOffset StartedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public int SessionCount { get; init; }
    public NextProgrammeSessionDto? NextSession { get; init; }
}

[Authorize]
public record GetActiveProgrammeQuery : IRequest<ActiveProgrammeDto?>;

public class GetActiveProgrammeQueryHandler : IRequestHandler<GetActiveProgrammeQuery, ActiveProgrammeDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly WarmupCalculatorService _warmupCalculator;

    // Starting Strength workout definitions
    private static readonly Dictionary<string, (int sets, int reps)> WorkoutALifts = new()
    {
        ["Squat"] = (3, 5),
        ["Bench Press"] = (3, 5),
        ["Deadlift"] = (1, 5)
    };

    private static readonly Dictionary<string, (int sets, int reps)> WorkoutBLifts = new()
    {
        ["Squat"] = (3, 5),
        ["Overhead Press"] = (3, 5),
        ["Deadlift"] = (1, 5)
    };

    public GetActiveProgrammeQueryHandler(IApplicationDbContext context, IUser currentUser, WarmupCalculatorService warmupCalculator)
    {
        _context = context;
        _currentUser = currentUser;
        _warmupCalculator = warmupCalculator;
    }

    public async ValueTask<ActiveProgrammeDto?> Handle(GetActiveProgrammeQuery request, CancellationToken cancellationToken)
    {
        var programme = await _context.UserProgrammes
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.Id && p.Status == ProgrammeStatus.Active, cancellationToken);

        if (programme == null) return null;

        var nextSession = programme.Sessions
            .Where(s => s.CompletedDate == null)
            .OrderBy(s => s.ScheduledDate)
            .FirstOrDefault();

        NextProgrammeSessionDto? nextSessionDto = null;

        if (nextSession != null)
        {
            var liftDefs = nextSession.WorkoutType == WorkoutType.A ? WorkoutALifts : WorkoutBLifts;
            var prescribedLifts = new List<PrescribedLiftDto>();

            foreach (var (liftName, (sets, reps)) in liftDefs)
            {
                decimal weight = nextSession.LiftProgression.TryGetValue(liftName, out var w) ? w : 20m;
                var warmupSets = _warmupCalculator.Calculate(weight);
                prescribedLifts.Add(new PrescribedLiftDto
                {
                    LiftName = liftName,
                    WeightKg = weight,
                    Sets = sets,
                    Reps = reps,
                    WarmupSets = warmupSets.Select(ws => new WarmupSetDto
                    {
                        SetNumber = ws.SetNumber,
                        Reps = ws.Reps,
                        WeightKg = ws.WeightKg
                    }).ToList()
                });
            }

            nextSessionDto = new NextProgrammeSessionDto
            {
                SessionId = nextSession.Id,
                WorkoutType = nextSession.WorkoutType.ToString(),
                ScheduledDate = nextSession.ScheduledDate,
                PrescribedLifts = prescribedLifts
            };
        }

        return new ActiveProgrammeDto
        {
            Id = programme.Id,
            ProgrammeTemplateId = programme.ProgrammeTemplateId,
            ProgrammeName = "Starting Strength",
            StartedAt = programme.StartedAt,
            Status = programme.Status.ToString(),
            SessionCount = programme.SessionCount,
            NextSession = nextSessionDto
        };
    }
}
