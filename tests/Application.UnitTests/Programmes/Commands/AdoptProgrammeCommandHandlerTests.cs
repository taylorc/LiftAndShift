using System.Linq;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Programmes.Commands.AdoptProgramme;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Commands;

public class AdoptProgrammeCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private AdoptProgrammeCommandHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new AdoptProgrammeCommandHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ShouldCreateActiveProgrammeForCurrentUser()
    {
        var command = new AdoptProgrammeCommand
        {
            ProgrammeTemplateId = "starting-strength",
            StartingWeights = new() { ["Squat"] = 60m, ["Bench Press"] = 40m }
        };

        var id = await _handler.Handle(command, CancellationToken.None);

        var programme = await _context.UserProgrammes.FindAsync(id);
        programme.ShouldNotBeNull();
        programme.UserId.ShouldBe(UserId);
        programme.ProgrammeTemplateId.ShouldBe("starting-strength");
        programme.Status.ShouldBe(ProgrammeStatus.Active);
        programme.SessionCount.ShouldBe(0);
        programme.CurrentWorkoutType.ShouldBe(WorkoutType.A);
    }

    [Test]
    public async Task ShouldCreateFirstSessionWithStartingWeights()
    {
        var command = new AdoptProgrammeCommand
        {
            StartingWeights = new() { ["Squat"] = 60m, ["Deadlift"] = 70m }
        };

        var id = await _handler.Handle(command, CancellationToken.None);

        var programme = await _context.UserProgrammes
            .Include(p => p.Sessions)
            .FirstAsync(p => p.Id == id);

        var firstSession = programme.Sessions.Single();
        firstSession.WorkoutType.ShouldBe(WorkoutType.A);
        firstSession.LiftProgression["Squat"].ShouldBe(60m);
        firstSession.LiftProgression["Deadlift"].ShouldBe(70m);
    }

    [Test]
    public async Task ShouldAbandonExistingActiveProgramme_WhenAdoptingNewOne()
    {
        var existing = new UserProgramme
        {
            UserId = UserId,
            ProgrammeTemplateId = "starting-strength",
            Status = ProgrammeStatus.Active
        };
        _context.UserProgrammes.Add(existing);
        await _context.SaveChangesAsync(CancellationToken.None);

        await _handler.Handle(new AdoptProgrammeCommand(), CancellationToken.None);

        var updatedExisting = await _context.UserProgrammes.FindAsync(existing.Id);
        updatedExisting!.Status.ShouldBe(ProgrammeStatus.Abandoned);
    }

    [Test]
    public async Task ShouldNotAbandonActiveProgrammesBelongingToOtherUsers()
    {
        var others = new UserProgramme
        {
            UserId = "other-user",
            ProgrammeTemplateId = "starting-strength",
            Status = ProgrammeStatus.Active
        };
        _context.UserProgrammes.Add(others);
        await _context.SaveChangesAsync(CancellationToken.None);

        await _handler.Handle(new AdoptProgrammeCommand(), CancellationToken.None);

        var updatedOthers = await _context.UserProgrammes.FindAsync(others.Id);
        updatedOthers!.Status.ShouldBe(ProgrammeStatus.Active);
    }
}
