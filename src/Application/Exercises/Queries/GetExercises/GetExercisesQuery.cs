using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Exercises.Queries.GetExercises;

public record ExerciseDto
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string MuscleGroup { get; init; } = string.Empty;
    public string EquipmentType { get; init; } = string.Empty;
    public string MovementPattern { get; init; } = string.Empty;
    public bool IsCustom { get; init; }
    public bool IsActive { get; init; }
}

[Authorize]
public record GetExercisesQuery : IRequest<List<ExerciseDto>>
{
    public string? Search { get; init; }
    public MuscleGroup? MuscleGroup { get; init; }
    public EquipmentType? EquipmentType { get; init; }
}

public class GetExercisesQueryHandler : IRequestHandler<GetExercisesQuery, List<ExerciseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetExercisesQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<List<ExerciseDto>> Handle(GetExercisesQuery request, CancellationToken cancellationToken)
    {
        // Built-in exercises have no creator and are visible to everyone; custom ones
        // are only visible to the user who created them.
        var query = _context.Exercises
            .Where(e => e.IsActive && (e.CreatedByUserId == null || e.CreatedByUserId == _currentUser.Id));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(search));
        }

        if (request.MuscleGroup.HasValue)
        {
            query = query.Where(e => e.MuscleGroup == request.MuscleGroup.Value);
        }

        if (request.EquipmentType.HasValue)
        {
            query = query.Where(e => e.EquipmentType == request.EquipmentType.Value);
        }

        return await query
            .OrderBy(e => e.Name)
            .Select(e => new ExerciseDto
            {
                Id = e.Id,
                Name = e.Name,
                Description = e.Description,
                MuscleGroup = e.MuscleGroup.ToString(),
                EquipmentType = e.EquipmentType.ToString(),
                MovementPattern = e.MovementPattern.ToString(),
                IsCustom = e.IsCustom,
                IsActive = e.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
