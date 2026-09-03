using LiftAndShift.Application.Common.Behaviours;
using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Exercises.Commands.CreateExercise;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace LiftAndShift.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<CreateExerciseCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<CreateExerciseCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<CreateExerciseCommand, int>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Handle(new CreateExerciseCommand { Name = "Squat" }, (_, _) => ValueTask.FromResult(1), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateExerciseCommand, int>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Handle(new CreateExerciseCommand { Name = "Squat" }, (_, _) => ValueTask.FromResult(1), new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
