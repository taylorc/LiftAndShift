using FluentValidation.Results;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using ValidationException = LiftAndShift.Application.Common.Exceptions.ValidationException;

namespace LiftAndShift.Application.Programmes.Commands.DeleteProgrammeSession;

[Authorize]
public record DeleteProgrammeSessionCommand : IRequest
{
    public int UserProgrammeId { get; init; }
    public int ProgrammeSessionId { get; init; }
}

public class DeleteProgrammeSessionCommandHandler : IRequestHandler<DeleteProgrammeSessionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public DeleteProgrammeSessionCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(DeleteProgrammeSessionCommand request, CancellationToken cancellationToken)
    {
        var programme = await _context.UserProgrammes
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == request.UserProgrammeId && p.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.UserProgrammeId, programme);

        var loggedSessions = programme.Sessions
            .Where(s => s.CompletedDate != null && s.WorkoutSessionId != null)
            .OrderBy(s => s.Id)
            .ToList();

        var latest = loggedSessions.LastOrDefault();
        Guard.Against.NotFound(request.ProgrammeSessionId, latest);

        if (latest.Id != request.ProgrammeSessionId)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.ProgrammeSessionId),
                    "Only the most recently logged session can be deleted."),
            });
        }

        // Remove the pending session that logging this one generated, if it's still untouched.
        var generatedPending = programme.Sessions
            .Where(s => s.Id > latest.Id && s.CompletedDate == null)
            .OrderBy(s => s.Id)
            .FirstOrDefault();
        if (generatedPending != null)
        {
            _context.ProgrammeSessions.Remove(generatedPending);
        }

        var workout = await _context.WorkoutSessions
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .FirstAsync(w => w.Id == latest.WorkoutSessionId!.Value, cancellationToken);

        _context.WorkoutSets.RemoveRange(workout.Exercises.SelectMany(e => e.Sets));
        _context.WorkoutExercises.RemoveRange(workout.Exercises);
        _context.WorkoutSessions.Remove(workout);

        latest.CompletedDate = null;
        latest.WorkoutSessionId = null;

        programme.SessionCount = Math.Max(0, programme.SessionCount - 1);
        programme.CurrentWorkoutType = latest.WorkoutType;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
