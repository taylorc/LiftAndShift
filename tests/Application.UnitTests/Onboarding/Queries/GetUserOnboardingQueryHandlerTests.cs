using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Models;
using LiftAndShift.Application.Onboarding.Queries.GetUserOnboarding;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace LiftAndShift.Application.UnitTests.Onboarding.Queries;

public class GetUserOnboardingQueryHandlerTests
{
    private Mock<IIdentityService> _identityService = null!;
    private Mock<IUser> _currentUser = null!;
    private GetUserOnboardingQueryHandler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        _identityService = new Mock<IIdentityService>();
        _currentUser = new Mock<IUser>();
        _handler = new GetUserOnboardingQueryHandler(_identityService.Object, _currentUser.Object);
    }

    [Test]
    public async Task ShouldReturnDto_WhenUserExists()
    {
        var userId = Guid.NewGuid().ToString();
        _currentUser.Setup(u => u.Id).Returns(userId);

        var expected = new UserOnboardingDto
        {
            IsOnboarded = true,
            PreferredUnit = "Kgs",
            BodyWeight = 80m,
            AlternatingLift = "PendlayRow",
            SquatStartingWeight = 60m,
            BenchPressStartingWeight = 40m,
            OverheadPressStartingWeight = 30m,
            DeadliftStartingWeight = 70m,
            AlternatingLiftStartingWeight = 50m
        };

        _identityService
            .Setup(s => s.GetUserOnboardingAsync(userId))
            .ReturnsAsync(expected);

        var result = await _handler.Handle(new GetUserOnboardingQuery(), CancellationToken.None);

        result.ShouldBe(expected);
    }

    [Test]
    public async Task ShouldReturnDefaultDto_WhenUserNotFound()
    {
        _currentUser.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());
        _identityService
            .Setup(s => s.GetUserOnboardingAsync(It.IsAny<string>()))
            .ReturnsAsync((UserOnboardingDto?)null);

        var result = await _handler.Handle(new GetUserOnboardingQuery(), CancellationToken.None);

        result.ShouldNotBeNull();
        result.IsOnboarded.ShouldBeFalse();
        result.PreferredUnit.ShouldBe("Lbs");
        result.AlternatingLift.ShouldBe("PowerClean");
        result.BodyWeight.ShouldBeNull();
        result.SquatStartingWeight.ShouldBeNull();
    }

    [Test]
    public async Task ShouldCallGetUserOnboardingAsync_WithCurrentUserId()
    {
        var userId = Guid.NewGuid().ToString();
        _currentUser.Setup(u => u.Id).Returns(userId);
        _identityService
            .Setup(s => s.GetUserOnboardingAsync(userId))
            .ReturnsAsync(new UserOnboardingDto());

        await _handler.Handle(new GetUserOnboardingQuery(), CancellationToken.None);

        _identityService.Verify(s => s.GetUserOnboardingAsync(userId), Times.Once);
    }
}
