using LiftAndShift.Domain.Entities;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Domain.UnitTests.Entities;

public class ExerciseCategoryTests
{
    [Test]
    public void NameShouldDefaultToEmptyString()
    {
        var category = new ExerciseCategory();

        category.Name.ShouldBe(string.Empty);
    }

    [Test]
    public void ShouldRetainAssignedName()
    {
        var category = new ExerciseCategory { Name = "Push" };

        category.Name.ShouldBe("Push");
    }
}
