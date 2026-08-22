using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;

namespace LiftAndShift.Application.Exercises.Commands.DeleteExercise;

[Authorize]
public record DeleteExerciseCommand(int Id) : IRequest;

public class DeleteExerciseCommandHandler : IRequestHandler<DeleteExerciseCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public DeleteExerciseCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(DeleteExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = await _context.Exercises
            .FirstOrDefaultAsync(e => e.Id == request.Id && e.IsCustom && e.CreatedByUserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, exercise);

        exercise.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
