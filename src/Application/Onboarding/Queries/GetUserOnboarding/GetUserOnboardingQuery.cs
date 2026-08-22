using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Models;
using LiftAndShift.Application.Common.Security;

namespace LiftAndShift.Application.Onboarding.Queries.GetUserOnboarding;

[Authorize]
public record GetUserOnboardingQuery : IRequest<UserOnboardingDto>;

public class GetUserOnboardingQueryHandler : IRequestHandler<GetUserOnboardingQuery, UserOnboardingDto>
{
    private readonly IIdentityService _identityService;
    private readonly IUser _currentUser;

    public GetUserOnboardingQueryHandler(IIdentityService identityService, IUser currentUser)
    {
        _identityService = identityService;
        _currentUser = currentUser;
    }

    public async ValueTask<UserOnboardingDto> Handle(GetUserOnboardingQuery request, CancellationToken cancellationToken)
    {
        return await _identityService.GetUserOnboardingAsync(_currentUser.Id!)
            ?? new UserOnboardingDto();
    }
}
