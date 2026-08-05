using System.Linq;
using LiftAndShift.Application.Programmes.Queries.GetProgrammeTemplates;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Queries;

public class GetProgrammeTemplatesQueryHandlerTests
{
    private GetProgrammeTemplatesQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _handler = new GetProgrammeTemplatesQueryHandler();
    }

    [Test]
    public async Task ShouldReturnStartingStrengthTemplate()
    {
        var result = await _handler.Handle(new GetProgrammeTemplatesQuery(), CancellationToken.None);

        var template = result.Single();
        template.Id.ShouldBe("starting-strength");
        template.Name.ShouldBe("Starting Strength");
        template.WorkoutAExercises.ShouldBe(new[] { "Squat", "Bench Press", "Deadlift" });
        template.WorkoutBExercises.ShouldBe(new[] { "Squat", "Overhead Press", "Deadlift" });
    }
}
