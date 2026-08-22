using LiftAndShift.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace LiftAndShift.Application.Common.Behaviours;

public class LoggingBehaviour<TRequest, TResponse> : MessagePreProcessor<TRequest, TResponse>
    where TRequest : notnull, IMessage
{
    private readonly ILogger _logger;
    private readonly IUser _user;
    private readonly IIdentityService _identityService;

    public LoggingBehaviour(ILogger<TRequest> logger, IUser user, IIdentityService identityService)
    {
        _logger = logger;
        _user = user;
        _identityService = identityService;
    }

    protected override async ValueTask Handle(TRequest request, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _user.Id ?? string.Empty;
        string? userName = string.Empty;

        if (!string.IsNullOrEmpty(userId))
        {
            userName = await _identityService.GetUserNameAsync(userId);
        }

        _logger.LogInformation("LiftAndShift Request: {Name} {@UserId} {@UserName} {@Request}",
            requestName, userId, userName, request);
    }
}
