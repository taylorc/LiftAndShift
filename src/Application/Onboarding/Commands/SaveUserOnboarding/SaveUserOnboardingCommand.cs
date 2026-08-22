using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Models;
using LiftAndShift.Application.Common.Security;

namespace LiftAndShift.Application.Onboarding.Commands.SaveUserOnboarding;

[Authorize]
public record SaveUserOnboardingCommand : IRequest<Result>
{
    public string PreferredUnit { get; init; } = "Lbs";
    public decimal BodyWeight { get; init; }
    public string AlternatingLift { get; init; } = "PowerClean";
    public decimal SquatStartingWeight { get; init; }
    public decimal BenchPressStartingWeight { get; init; }
    public decimal OverheadPressStartingWeight { get; init; }
    public decimal DeadliftStartingWeight { get; init; }
    public decimal AlternatingLiftStartingWeight { get; init; }
}

public class SaveUserOnboardingCommandHandler : IRequestHandler<SaveUserOnboardingCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public SaveUserOnboardingCommandHandler(IIdentityService identityService, IUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async ValueTask<Result> Handle(SaveUserOnboardingCommand request, CancellationToken cancellationToken)
    {
        var dto = new UserOnboardingDto
        {
            PreferredUnit = request.PreferredUnit,
            BodyWeight = request.BodyWeight,
            AlternatingLift = request.AlternatingLift,
            SquatStartingWeight = request.SquatStartingWeight,
            BenchPressStartingWeight = request.BenchPressStartingWeight,
            OverheadPressStartingWeight = request.OverheadPressStartingWeight,
            DeadliftStartingWeight = request.DeadliftStartingWeight,
            AlternatingLiftStartingWeight = request.AlternatingLiftStartingWeight
        };

        return await _identityService.SaveUserOnboardingAsync(_currentUser.Id!, dto);
    }
}
