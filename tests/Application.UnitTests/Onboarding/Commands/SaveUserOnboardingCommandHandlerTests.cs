using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Models;
using LiftAndShift.Application.Onboarding.Commands.SaveUserOnboarding;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Onboarding.Commands;

public class SaveUserOnboardingCommandHandlerTests
{
    private Mock<IIdentityService> _identityService = null!;
    private Mock<IUser> _currentUser = null!;
    private SaveUserOnboardingCommandHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _identityService = new Mock<IIdentityService>();
        _currentUser = new Mock<IUser>();
        _handler = new SaveUserOnboardingCommandHandler(_identityService.Object, _currentUser.Object);
    }

    [Test]
    public async Task ShouldCallSaveUserOnboardingAsync_WithCurrentUserIdAndMappedDto()
    {
        var userId = Guid.NewGuid().ToString();
        _currentUser.Setup(u => u.Id).Returns(userId);
        _identityService
            .Setup(s => s.SaveUserOnboardingAsync(userId, It.IsAny<UserOnboardingDto>()))
            .ReturnsAsync(Result.Success());

        var command = new SaveUserOnboardingCommand
        {
            PreferredUnit = "Kgs",
            BodyWeight = 80m,
            AlternatingLift = "PendlayRow",
            SquatStartingWeight = 60m,
            BenchPressStartingWeight = 40m,
            OverheadPressStartingWeight = 30m,
            DeadliftStartingWeight = 70m,
            AlternatingLiftStartingWeight = 50m
        };

        await _handler.Handle(command, CancellationToken.None);

        _identityService.Verify(s => s.SaveUserOnboardingAsync(
            userId,
            It.Is<UserOnboardingDto>(dto =>
                dto.PreferredUnit == "Kgs" &&
                dto.BodyWeight == 80m &&
                dto.AlternatingLift == "PendlayRow" &&
                dto.SquatStartingWeight == 60m &&
                dto.BenchPressStartingWeight == 40m &&
                dto.OverheadPressStartingWeight == 30m &&
                dto.DeadliftStartingWeight == 70m &&
                dto.AlternatingLiftStartingWeight == 50m)),
            Times.Once);
    }

    [Test]
    public async Task ShouldReturnSuccess_WhenIdentityServiceSucceeds()
    {
        _currentUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _identityService
            .Setup(s => s.SaveUserOnboardingAsync(It.IsAny<string>(), It.IsAny<UserOnboardingDto>()))
            .ReturnsAsync(Result.Success());

        var result = await _handler.Handle(new SaveUserOnboardingCommand
        {
            BodyWeight = 180m,
            SquatStartingWeight = 135m,
            BenchPressStartingWeight = 95m,
            OverheadPressStartingWeight = 65m,
            DeadliftStartingWeight = 155m,
            AlternatingLiftStartingWeight = 95m
        }, CancellationToken.None);

        result.Succeeded.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnFailure_WhenIdentityServiceFails()
    {
        _currentUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _identityService
            .Setup(s => s.SaveUserOnboardingAsync(It.IsAny<string>(), It.IsAny<UserOnboardingDto>()))
            .ReturnsAsync(Result.Failure(["User not found."]));

        var result = await _handler.Handle(new SaveUserOnboardingCommand
        {
            BodyWeight = 180m,
            SquatStartingWeight = 135m,
            BenchPressStartingWeight = 95m,
            OverheadPressStartingWeight = 65m,
            DeadliftStartingWeight = 155m,
            AlternatingLiftStartingWeight = 95m
        }, CancellationToken.None);

        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldContain("User not found.");
    }
}
