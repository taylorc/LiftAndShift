using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Exercises.Commands.CreateExercise;

[Authorize]
public record CreateExerciseCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public MuscleGroup MuscleGroup { get; init; }
    public EquipmentType EquipmentType { get; init; }
    public MovementPattern MovementPattern { get; init; }
}

public class CreateExerciseCommandHandler : IRequestHandler<CreateExerciseCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public CreateExerciseCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateExerciseCommand request, CancellationToken cancellationToken)
    {
        var exercise = new Exercise
        {
            Name = request.Name,
            Description = request.Description,
            MuscleGroup = request.MuscleGroup,
            EquipmentType = request.EquipmentType,
            MovementPattern = request.MovementPattern,
            IsCustom = true,
            CreatedByUserId = _currentUser.Id,
            IsActive = true
        };

        _context.Exercises.Add(exercise);
        await _context.SaveChangesAsync(cancellationToken);

        return exercise.Id;
    }
}

public class CreateExerciseCommandValidator : AbstractValidator<CreateExerciseCommand>
{
    public CreateExerciseCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
