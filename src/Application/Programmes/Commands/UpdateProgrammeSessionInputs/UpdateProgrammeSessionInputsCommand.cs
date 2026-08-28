using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Application.Programmes.Progression;

namespace LiftAndShift.Application.Programmes.Commands.UpdateProgrammeSessionInputs;

/// <summary>
/// Manually overrides a programme session's prescribed inputs (per-lift working weight and/or
/// consecutive-failure count), then replays every later session's prescription from that new
/// baseline. Use when a lifter wants to override what the progression algorithm produced.
/// </summary>
[Authorize]
public record UpdateProgrammeSessionInputsCommand : IRequest
{
    public int UserProgrammeId { get; init; }
    public int ProgrammeSessionId { get; init; }
    public Dictionary<string, decimal>? LiftProgression { get; init; }
    public Dictionary<string, int>? ConsecutiveFailures { get; init; }
}

public class UpdateProgrammeSessionInputsCommandHandler : IRequestHandler<UpdateProgrammeSessionInputsCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public UpdateProgrammeSessionInputsCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(UpdateProgrammeSessionInputsCommand request, CancellationToken cancellationToken)
    {
        var programme = await _context.UserProgrammes
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == request.UserProgrammeId && p.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.UserProgrammeId, programme);

        var session = programme.Sessions.FirstOrDefault(s => s.Id == request.ProgrammeSessionId);
        Guard.Against.NotFound(request.ProgrammeSessionId, session);

        if (request.LiftProgression != null)
        {
            session.LiftProgression = new Dictionary<string, decimal>(request.LiftProgression);
        }

        if (request.ConsecutiveFailures != null)
        {
            session.ConsecutiveFailures = new Dictionary<string, int>(request.ConsecutiveFailures);
        }

        await ProgrammeSessionChainReplayer.ReplayForwardAsync(_context, programme, session.Id, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

public class UpdateProgrammeSessionInputsCommandValidator : AbstractValidator<UpdateProgrammeSessionInputsCommand>
{
    public UpdateProgrammeSessionInputsCommandValidator()
    {
        RuleFor(x => x.UserProgrammeId).GreaterThan(0);
        RuleFor(x => x.ProgrammeSessionId).GreaterThan(0);
        RuleFor(x => x)
            .Must(x => x.LiftProgression != null || x.ConsecutiveFailures != null)
            .WithMessage("Provide LiftProgression, ConsecutiveFailures, or both.");
    }
}
