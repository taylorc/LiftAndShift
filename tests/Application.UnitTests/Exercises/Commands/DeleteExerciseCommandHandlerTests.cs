
using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Exercises.Commands.CreateExercise;
using LiftAndShift.Application.Exercises.Commands.DeleteExercise;
using LiftAndShift.Application.Programmes.Commands.AdoptProgramme;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LiftAndShift.Application.UnitTests.Exercises.Commands;

public class DeleteExerciseCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private CreateExerciseCommandHandler _createExerciseCommandHandler = null!;
    private DeleteExerciseCommandHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new DeleteExerciseCommandHandler(_context, _currentUser.Object);
        _createExerciseCommandHandler = new CreateExerciseCommandHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ShouldDeleteExerciseForCurrentUser()
    {
        var command = new CreateExerciseCommand
        {
            Name = "Push-ups",
            Description = "Standard push-up exercise",
            MuscleGroup = MuscleGroup.Chest,
            EquipmentType = EquipmentType.Bodyweight,
            MovementPattern = MovementPattern.Push
        };

        var id = await _createExerciseCommandHandler.Handle(command, CancellationToken.None);

        var deleteCommand = new DeleteExerciseCommand(id);

        await _handler.Handle(deleteCommand, CancellationToken.None);

        var exercise = await _context.Exercises.FindAsync(id);
        exercise.ShouldNotBeNull();
        exercise.IsActive.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldThrowNotFoundExceptionWhenExerciseNotFound()
    {
        var deleteCommand = new DeleteExerciseCommand(18);

        await Should.ThrowAsync<NotFoundException>(() => _handler.Handle(deleteCommand, CancellationToken.None).AsTask());

    }
}   
         

