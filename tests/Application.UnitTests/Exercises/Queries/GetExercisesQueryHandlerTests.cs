using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Exercises.Queries.GetExercises;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Exercises.Queries;

public class GetExercisesQueryHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private GetExercisesQueryHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new GetExercisesQueryHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ShouldReturnEmptyList_WhenNoExercisesExist()
    {
        var result = await _handler.Handle(new GetExercisesQuery { Search = "banana lounge"}, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count().ShouldBe(0);
    }

    [Test]
    public async Task ShouldReturnCorrectExercise_WhenExercisesExist()
    {
        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 1,
            Name = "Squat",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Back,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 2,
            Name = "Deadlift",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId
        });

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExercisesQuery { Search = "squat" }, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result[0].Id.ShouldBe(1);
        result[0].Name.ShouldBe("Squat");
        result[0].Description.ShouldBe("A lower body exercise");
        result[0].MuscleGroup.ShouldBe(Domain.Enums.MuscleGroup.Back.ToString());
        result[0].EquipmentType.ShouldBe(Domain.Enums.EquipmentType.Barbell.ToString());
    }

    [Test]
    public async Task ShouldNotReturnExercise_WhenExercisesExistsForAnotherUser()
    {
        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 1,
            Name = "Squat",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Back,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = "user-2",
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 2,
            Name = "Deadlift",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId
        });

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExercisesQuery { Search = "squat" }, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count().ShouldBe(0);
    }

    [Test]
    public async Task ShouldReturnExercise_WhenExercisesAnExistsForAnotherUser()
    {
        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 1,
            Name = "Squat",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Back,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = "user-2",
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 2,
            Name = "Deadlift",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId
        });

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExercisesQuery { Search = "Deadlift" }, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result[0].Id.ShouldBe(2);
        result[0].Name.ShouldBe("Deadlift");
        result[0].Description.ShouldBe("A lower body exercise");
        result[0].MuscleGroup.ShouldBe(Domain.Enums.MuscleGroup.Legs.ToString());
        result[0].EquipmentType.ShouldBe(Domain.Enums.EquipmentType.Barbell.ToString());
    }

    [Test]
    public async Task ShouldIncludeMuscleGroupInQuery_WhenMuscleGroupSelected()
    {
        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 1,
            Name = "Squat",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Back,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId,
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 2,
            Name = "Deadlift",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 3,
            Name = "Bicep Curl",
            Description = "An arm exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Arms,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId
        });

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExercisesQuery { MuscleGroup = Domain.Enums.MuscleGroup.Legs }, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result[0].Id.ShouldBe(2);
        result[0].Name.ShouldBe("Deadlift");
        result[0].Description.ShouldBe("A lower body exercise");
        result[0].MuscleGroup.ShouldBe(Domain.Enums.MuscleGroup.Legs.ToString());
        result[0].EquipmentType.ShouldBe(Domain.Enums.EquipmentType.Barbell.ToString());
    }

    [Test]
    public async Task ShouldIncludeEquipmentTypeInQuery_WhenEquipmentTypeSelected()
    {
        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 1,
            Name = "Squat",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Back,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = "user-1",
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 2,
            Name = "Deadlift",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 3,
            Name = "Leg Extension",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Machine,
            CreatedByUserId = UserId
        });

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExercisesQuery { EquipmentType = Domain.Enums.EquipmentType.Barbell }, CancellationToken.None);

        result.ShouldNotBeNull();
        result.Count().ShouldBe(2);
    }

    [Test]
    public async Task ShouldIncludeAllFiltersQuery_WhenAllSelected()
    {
        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 1,
            Name = "Squat",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Back,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = "user-1",
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 2,
            Name = "Deadlift",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 3,
            Name = "Leg Extension",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Machine,
            CreatedByUserId = UserId
        });

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExercisesQuery { 
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            Search = "Deadlift"
        }, CancellationToken.None);


        result.ShouldNotBeNull();
        result.Count().ShouldBe(1);
        result[0].Id.ShouldBe(2);
        result[0].Name.ShouldBe("Deadlift");
        result[0].Description.ShouldBe("A lower body exercise");
        result[0].MuscleGroup.ShouldBe(Domain.Enums.MuscleGroup.Legs.ToString());
        result[0].EquipmentType.ShouldBe(Domain.Enums.EquipmentType.Barbell.ToString());
    }

    [Test]
    public async Task ShouldNotIncludeAllFiltersQuery_WhenAllSelected_ButNoExerciseMatches()
    {
        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 1,
            Name = "Squat",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Back,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = "user-1",
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 2,
            Name = "Deadlift",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            CreatedByUserId = UserId
        });

        _context.Exercises.Add(new Domain.Entities.Exercise
        {
            Id = 3,
            Name = "Leg Extension",
            Description = "A lower body exercise",
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            EquipmentType = Domain.Enums.EquipmentType.Machine,
            CreatedByUserId = UserId
        });

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _handler.Handle(new GetExercisesQuery
        {
            EquipmentType = Domain.Enums.EquipmentType.Barbell,
            MuscleGroup = Domain.Enums.MuscleGroup.Legs,
            Search = "Leg Curl"
        }, CancellationToken.None);


        result.ShouldNotBeNull();
        result.Count().ShouldBe(0);
    }
}
