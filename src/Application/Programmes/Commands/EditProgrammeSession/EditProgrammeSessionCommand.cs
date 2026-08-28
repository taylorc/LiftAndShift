using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Application.Programmes.Progression;
using LiftAndShift.Application.Workouts.Commands.LogWorkout;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;

namespace LiftAndShift.Application.Programmes.Commands.EditProgrammeSession;

[Authorize]
public record EditProgrammeSessionCommand : IRequest
{
    public int UserProgrammeId { get; init; }
    public int ProgrammeSessionId { get; init; }
    public List<LogWorkoutExerciseDto> Exercises { get; init; } = new();
}

public class EditProgrammeSessionCommandHandler : IRequestHandler<EditProgrammeSessionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public EditProgrammeSessionCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<Unit> Handle(EditProgrammeSessionCommand request, CancellationToken cancellationToken)
    {
        var programme = await _context.UserProgrammes
            .Include(p => p.Sessions)
            .FirstOrDefaultAsync(p => p.Id == request.UserProgrammeId && p.UserId == _currentUser.Id, cancellationToken);

        Guard.Against.NotFound(request.UserProgrammeId, programme);

        // Only a logged session (completed, with a linked workout) can be edited.
        var logged = programme.Sessions
            .FirstOrDefault(s => s.Id == request.ProgrammeSessionId && s.CompletedDate != null && s.WorkoutSessionId != null);

        Guard.Against.NotFound(request.ProgrammeSessionId, logged);

        var workout = await _context.WorkoutSessions
            .Include(w => w.Exercises).ThenInclude(e => e.Sets)
            .FirstAsync(w => w.Id == logged.WorkoutSessionId!.Value, cancellationToken);

        ReplaceSetData(workout, request.Exercises);

        await ProgrammeSessionChainReplayer.ReplayForwardAsync(_context, programme, logged.Id, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }

    private void ReplaceSetData(WorkoutSession workout, IReadOnlyCollection<LogWorkoutExerciseDto> exercises)
    {
        _context.WorkoutSets.RemoveRange(workout.Exercises.SelectMany(e => e.Sets));
        _context.WorkoutExercises.RemoveRange(workout.Exercises);
        workout.Exercises.Clear();

        foreach (var ex in exercises)
        {
            var workoutExercise = new WorkoutExercise
            {
                ExerciseId = ex.ExerciseId,
                OrderIndex = ex.OrderIndex,
                Notes = ex.Notes,
            };

            foreach (var set in ex.Sets)
            {
                workoutExercise.Sets.Add(new WorkoutSet
                {
                    SetNumber = set.SetNumber,
                    SetType = set.SetType,
                    WeightKg = set.WeightKg,
                    Reps = set.Reps,
                    CompletedReps = set.CompletedReps,
                    Notes = set.Notes,
                    IsCompleted = set.IsCompleted,
                });
            }

            workout.Exercises.Add(workoutExercise);
        }
    }
}

public class EditProgrammeSessionCommandValidator : AbstractValidator<EditProgrammeSessionCommand>
{
    public EditProgrammeSessionCommandValidator()
    {
        RuleFor(x => x.UserProgrammeId).GreaterThan(0);
        RuleFor(x => x.ProgrammeSessionId).GreaterThan(0);

        RuleForEach(x => x.Exercises).ChildRules(exercise =>
        {
            exercise.RuleForEach(e => e.Sets).ChildRules(set =>
            {
                set.RuleFor(s => s.CompletedReps)
                    .NotNull()
                    .When(s => s.SetType == SetType.WorkingSet)
                    .WithMessage("Every working set must have its completed reps filled in.");

                set.RuleFor(s => s.IsCompleted)
                    .Equal(true)
                    .When(s => s.CompletedReps.HasValue)
                    .WithMessage("A set with completed reps entered must be marked done.");
            });
        });
    }
}
