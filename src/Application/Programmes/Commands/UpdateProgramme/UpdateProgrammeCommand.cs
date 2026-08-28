using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Programmes.Commands.UpdateProgramme;

[Authorize]
public record UpdateProgrammeCommand : IRequest
{
    public int UserProgrammeId { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public ProgrammeStatus? Status { get; init; }
}

public class UpdateProgrammeCommandHandler : IRequestHandler<UpdateProgrammeCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public UpdateProgrammeCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(UpdateProgrammeCommand request, CancellationToken cancellationToken)
    {
        var programme = await _context.UserProgrammes
            .FirstOrDefaultAsync(p => p.Id == request.UserProgrammeId && p.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.UserProgrammeId, programme);

        if (request.StartedAt.HasValue)
        {
            programme.StartedAt = request.StartedAt.Value;
        }

        if (request.Status.HasValue)
        {
            programme.Status = request.Status.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}

public class UpdateProgrammeCommandValidator : AbstractValidator<UpdateProgrammeCommand>
{
    public UpdateProgrammeCommandValidator()
    {
        RuleFor(x => x.UserProgrammeId).GreaterThan(0);
        RuleFor(x => x.Status!.Value).IsInEnum().When(x => x.Status.HasValue);
        RuleFor(x => x)
            .Must(x => x.StartedAt.HasValue || x.Status.HasValue)
            .WithMessage("Provide StartedAt, Status, or both.");
    }
}
