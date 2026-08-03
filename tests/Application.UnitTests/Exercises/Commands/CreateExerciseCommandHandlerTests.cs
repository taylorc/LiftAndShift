
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Exercises.Commands.CreateExercise;
using LiftAndShift.Application.Programmes.Commands.AdoptProgramme;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Exercises.Commands;

public class CreateExerciseCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private CreateExerciseCommandHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new CreateExerciseCommandHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ShouldCreateExerciseForCurrentUser()
    {
        var command = new CreateExerciseCommand
        {
            Name = "Push-ups",
            Description = "Standard push-up exercise",
            MuscleGroup = MuscleGroup.Chest,
            EquipmentType = EquipmentType.Bodyweight,
            MovementPattern = MovementPattern.Push
        };

        var id = await _handler.Handle(command, CancellationToken.None);

        var exercise = await _context.Exercises.FindAsync(id);
        exercise.ShouldNotBeNull();
        exercise.CreatedByUserId.ShouldBe(UserId);
        exercise.Name.ShouldBe("Push-ups");
        exercise.Description.ShouldBe("Standard push-up exercise");
        exercise.EquipmentType.ShouldBe(EquipmentType.Bodyweight);
        exercise.MuscleGroup.ShouldBe(MuscleGroup.Chest);
        exercise.IsActive.ShouldBeTrue();
        exercise.MovementPattern.ShouldBe(MovementPattern.Push);
        exercise.IsCustom.ShouldBeTrue();
    }

    
}

