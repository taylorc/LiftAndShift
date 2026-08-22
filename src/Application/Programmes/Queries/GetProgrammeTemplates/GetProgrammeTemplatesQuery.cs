using LiftAndShift.Application.Common.Security;

namespace LiftAndShift.Application.Programmes.Queries.GetProgrammeTemplates;

public record ProgrammeTemplateDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public List<string> WorkoutAExercises { get; init; } = new();
    public List<string> WorkoutBExercises { get; init; } = new();
}

[Authorize]
public record GetProgrammeTemplatesQuery : IRequest<List<ProgrammeTemplateDto>>;

public class GetProgrammeTemplatesQueryHandler : IRequestHandler<GetProgrammeTemplatesQuery, List<ProgrammeTemplateDto>>
{
    public ValueTask<List<ProgrammeTemplateDto>> Handle(GetProgrammeTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = new List<ProgrammeTemplateDto>
        {
            new()
            {
                Id = "starting-strength",
                Name = "Starting Strength",
                Description = "Mark Rippetoe's beginner barbell programme. Three full-body sessions per week alternating A/B workouts with linear progression.",
                WorkoutAExercises = new() { "Squat", "Bench Press", "Deadlift" },
                WorkoutBExercises = new() { "Squat", "Overhead Press", "Deadlift" }
            }
        };

        return ValueTask.FromResult(templates);
    }
}
