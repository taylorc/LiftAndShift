using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Programmes.Commands.AdoptProgramme;

[Authorize]
public record AdoptProgrammeCommand : IRequest<int>
{
    public string ProgrammeTemplateId { get; init; } = "starting-strength";
    public Dictionary<string, decimal> StartingWeights { get; init; } = new();
}

public class AdoptProgrammeCommandHandler : IRequestHandler<AdoptProgrammeCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public AdoptProgrammeCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(AdoptProgrammeCommand request, CancellationToken cancellationToken)
    {
        // Abandon any existing active programme
        var existingActive = await _context.UserProgrammes
            .Where(p => p.UserId == _currentUser.Id && p.Status == ProgrammeStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var prog in existingActive)
        {
            prog.Status = ProgrammeStatus.Abandoned;
        }

        var programme = new UserProgramme
        {
            UserId = _currentUser.Id!,
            ProgrammeTemplateId = request.ProgrammeTemplateId,
            StartedAt = DateTimeOffset.UtcNow,
            Status = ProgrammeStatus.Active,
            SessionCount = 0,
            CurrentWorkoutType = WorkoutType.A
        };

        // Create the first session
        var firstSession = new ProgrammeSession
        {
            WorkoutType = WorkoutType.A,
            ScheduledDate = DateTimeOffset.UtcNow,
            LiftProgression = new Dictionary<string, decimal>(request.StartingWeights),
            ConsecutiveFailures = new Dictionary<string, int>()
        };

        programme.Sessions.Add(firstSession);
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(cancellationToken);

        return programme.Id;
    }
}

public class AdoptProgrammeCommandValidator : AbstractValidator<AdoptProgrammeCommand>
{
    public AdoptProgrammeCommandValidator()
    {
        RuleFor(x => x.ProgrammeTemplateId)
            .NotEmpty().WithMessage("ProgrammeTemplateId is required.");
    }
}
