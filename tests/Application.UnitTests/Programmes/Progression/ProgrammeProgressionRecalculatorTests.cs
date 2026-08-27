using LiftAndShift.Application.Programmes.Progression;
using LiftAndShift.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Progression;

[TestFixture]
public class ProgrammeProgressionRecalculatorTests
{
    private static LoggedLift Lift(string name, int? completedReps, SetType setType = SetType.WorkingSet) =>
        new(name, new[] { new LoggedSet(setType, completedReps, Reps: 5) });

    [Test]
    public void NextSessionState_IncrementsWeightAndResetsFailures_WhenWorkingSetsMeetTarget()
    {
        var next = ProgrammeProgressionRecalculator.NextSessionState(
            currentWeights: new Dictionary<string, decimal> { ["Squat"] = 60m },
            currentFailures: new Dictionary<string, int> { ["Squat"] = 2 },
            loggedLifts: new[] { Lift("Squat", completedReps: 5) });

        // Squat is a heavy lift: +5kg
        next.Weights["Squat"].ShouldBe(65m);
        next.Failures["Squat"].ShouldBe(0);
    }

    [Test]
    public void NextSessionState_HoldsWeightAndIncrementsFailures_OnFirstFailure()
    {
        var next = ProgrammeProgressionRecalculator.NextSessionState(
            currentWeights: new Dictionary<string, decimal> { ["Squat"] = 60m },
            currentFailures: new Dictionary<string, int>(),
            loggedLifts: new[] { Lift("Squat", completedReps: 3) });

        next.Weights["Squat"].ShouldBe(60m);
        next.Failures["Squat"].ShouldBe(1);
    }

    [Test]
    public void NextSessionState_DeloadsAndResetsFailures_OnThirdConsecutiveFailure()
    {
        var next = ProgrammeProgressionRecalculator.NextSessionState(
            currentWeights: new Dictionary<string, decimal> { ["Squat"] = 60m },
            currentFailures: new Dictionary<string, int> { ["Squat"] = 2 },
            loggedLifts: new[] { Lift("Squat", completedReps: 3) });

        // 60 * 0.9 = 54, rounded to nearest 1.25 = 53.75, streak resets
        next.Weights["Squat"].ShouldBe(53.75m);
        next.Failures["Squat"].ShouldBe(0);
    }

    [Test]
    public void NextSessionState_CarriesLiftForwardUnchanged_WhenNotLoggedThisSession()
    {
        var next = ProgrammeProgressionRecalculator.NextSessionState(
            currentWeights: new Dictionary<string, decimal> { ["Squat"] = 60m, ["Bench Press"] = 40m },
            currentFailures: new Dictionary<string, int> { ["Bench Press"] = 1 },
            loggedLifts: new[] { Lift("Squat", completedReps: 5) });

        next.Weights["Bench Press"].ShouldBe(40m);
        next.Failures["Bench Press"].ShouldBe(1);
    }

    [Test]
    public void NextSessionState_CarriesLiftForwardUnchanged_WhenNoEvaluableWorkingSets()
    {
        var next = ProgrammeProgressionRecalculator.NextSessionState(
            currentWeights: new Dictionary<string, decimal> { ["Squat"] = 60m },
            currentFailures: new Dictionary<string, int> { ["Squat"] = 1 },
            loggedLifts: new[] { Lift("Squat", completedReps: null) });

        next.Weights["Squat"].ShouldBe(60m);
        next.Failures["Squat"].ShouldBe(1);
    }

    [Test]
    public void NextSessionState_IgnoresWarmupSets_WhenDeterminingOutcome()
    {
        var next = ProgrammeProgressionRecalculator.NextSessionState(
            currentWeights: new Dictionary<string, decimal> { ["Squat"] = 60m },
            currentFailures: new Dictionary<string, int>(),
            loggedLifts: new[]
            {
                new LoggedLift("Squat", new[]
                {
                    new LoggedSet(SetType.Warmup, CompletedReps: 2, Reps: 5),
                    new LoggedSet(SetType.WorkingSet, CompletedReps: 5, Reps: 5),
                })
            });

        next.Weights["Squat"].ShouldBe(65m);
    }

    [Test]
    public void NextSessionState_LeavesUnknownLiftOut_WhenNotInCurrentWeights()
    {
        var next = ProgrammeProgressionRecalculator.NextSessionState(
            currentWeights: new Dictionary<string, decimal> { ["Squat"] = 60m },
            currentFailures: new Dictionary<string, int>(),
            loggedLifts: new[] { Lift("Power Clean", completedReps: 5) });

        next.Weights.ShouldNotContainKey("Power Clean");
    }
}
