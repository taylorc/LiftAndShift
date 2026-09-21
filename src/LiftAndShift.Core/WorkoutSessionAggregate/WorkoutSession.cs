using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.Workouts;

namespace LiftAndShift.Core.WorkoutSessionAggregate;

public class WorkoutSession : EntityBase<WorkoutSession, WorkoutSessionId>, IAggregateRoot
{
  private readonly List<LoggedSet> _loggedSets = [];

  public FamilyMemberId FamilyMemberId { get; private set; }
  public Workout Workout { get; private set; } = null!;
  public TrainingPhase TrainingPhase { get; private set; }
  public DateOnly PerformedOn { get; private set; }
  public IReadOnlyCollection<LoggedSet> LoggedSets => _loggedSets.AsReadOnly();

  private WorkoutSession(FamilyMemberId familyMemberId, Workout workout, TrainingPhase trainingPhase, DateOnly performedOn)
  {
    FamilyMemberId = familyMemberId;
    Workout = workout;
    TrainingPhase = trainingPhase;
    PerformedOn = performedOn;
  }

  public static WorkoutSession Log(
    FamilyMemberId familyMemberId,
    Workout workout,
    TrainingPhase trainingPhase,
    DateOnly performedOn,
    IReadOnlyList<LoggedLiftInput> loggedLifts)
  {
    var expectedLifts = WorkoutLifts.For(workout, trainingPhase);
    var loggedLiftNames = loggedLifts.Select(l => l.Lift).ToList();

    if (loggedLiftNames.Count != expectedLifts.Count
        || loggedLiftNames.Distinct().Count() != loggedLiftNames.Count
        || !expectedLifts.All(loggedLiftNames.Contains))
    {
      throw new ArgumentException(
        $"{workout.Name} at Training Phase {trainingPhase.Value} must log exactly these lifts: {string.Join(", ", expectedLifts.Select(l => l.Name))}.",
        nameof(loggedLifts));
    }

    var session = new WorkoutSession(familyMemberId, workout, trainingPhase, performedOn);

    foreach (var input in loggedLifts)
    {
      if (input.RepsPerSet.Count != input.Lift.WorkSetCount)
      {
        throw new ArgumentException(
          $"{input.Lift.Name} requires {input.Lift.WorkSetCount} logged set(s), but {input.RepsPerSet.Count} were given.",
          nameof(loggedLifts));
      }

      for (int setNumber = 1; setNumber <= input.RepsPerSet.Count; setNumber++)
      {
        session._loggedSets.Add(new LoggedSet(input.Lift, input.WeightKg, setNumber, input.RepsPerSet[setNumber - 1]));
      }
    }

    return session;
  }
}
