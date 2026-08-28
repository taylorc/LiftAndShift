using Ardalis.GuardClauses;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Programmes.Commands.UpdateProgramme;
using LiftAndShift.Application.UnitTests.Common.TestHelpers;
using LiftAndShift.Domain.Entities;
using LiftAndShift.Domain.Enums;
using LiftAndShift.Infrastructure.Data;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Programmes.Commands;

public class UpdateProgrammeCommandHandlerTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IUser> _currentUser = null!;
    private UpdateProgrammeCommandHandler _handler = null!;
    private const string UserId = "user-1";

    [SetUp]
    public void SetUp()
    {
        _context = ApplicationDbContextFactory.Create();
        _currentUser = new Mock<IUser>();
        _currentUser.Setup(u => u.Id).Returns(UserId);
        _handler = new UpdateProgrammeCommandHandler(_context, _currentUser.Object);
    }

    [TearDown]
    public void TearDown() => _context.Dispose();

    private async Task<UserProgramme> SeedAsync(string userId = UserId)
    {
        var programme = new UserProgramme
        {
            UserId = userId,
            ProgrammeTemplateId = "starting-strength",
            StartedAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
            Status = ProgrammeStatus.Active,
        };
        _context.UserProgrammes.Add(programme);
        await _context.SaveChangesAsync(CancellationToken.None);
        return programme;
    }

    [Test]
    public async Task ShouldUpdateStartedAt_WhenProvided()
    {
        var programme = await SeedAsync();
        var newStart = new DateTimeOffset(2025, 12, 1, 0, 0, 0, TimeSpan.Zero);

        await _handler.Handle(new UpdateProgrammeCommand { UserProgrammeId = programme.Id, StartedAt = newStart }, CancellationToken.None);

        (await _context.UserProgrammes.FindAsync(programme.Id))!.StartedAt.ShouldBe(newStart);
    }

    [Test]
    public async Task ShouldUpdateStatus_WhenProvided()
    {
        var programme = await SeedAsync();

        await _handler.Handle(new UpdateProgrammeCommand { UserProgrammeId = programme.Id, Status = ProgrammeStatus.Paused }, CancellationToken.None);

        (await _context.UserProgrammes.FindAsync(programme.Id))!.Status.ShouldBe(ProgrammeStatus.Paused);
    }

    [Test]
    public async Task ShouldLeaveFieldsUnchanged_WhenNotProvided()
    {
        var programme = await SeedAsync();
        var originalStart = programme.StartedAt;

        await _handler.Handle(new UpdateProgrammeCommand { UserProgrammeId = programme.Id }, CancellationToken.None);

        var reloaded = await _context.UserProgrammes.FindAsync(programme.Id);
        reloaded!.StartedAt.ShouldBe(originalStart);
        reloaded.Status.ShouldBe(ProgrammeStatus.Active);
    }

    [Test]
    public async Task ShouldThrowNotFound_WhenProgrammeBelongsToAnotherUser()
    {
        var programme = await SeedAsync(userId: "someone-else");

        await Should.ThrowAsync<NotFoundException>(
            () => _handler.Handle(new UpdateProgrammeCommand { UserProgrammeId = programme.Id, Status = ProgrammeStatus.Paused }, CancellationToken.None).AsTask());
    }
}
