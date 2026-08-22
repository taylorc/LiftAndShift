using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;
using LiftAndShift.Domain.Entities;

namespace LiftAndShift.Application.BodyMetrics.Commands.LogBodyMetric;

[Authorize]
public record LogBodyMetricCommand : IRequest<int>
{
    public DateTimeOffset Date { get; init; } = DateTimeOffset.UtcNow;
    public decimal WeightKg { get; init; }
    public string? Notes { get; init; }
}

public class LogBodyMetricCommandHandler : IRequestHandler<LogBodyMetricCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public LogBodyMetricCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<int> Handle(LogBodyMetricCommand request, CancellationToken cancellationToken)
    {
        var metric = new BodyMetric
        {
            UserId = _currentUser.Id!,
            Date = request.Date,
            WeightKg = request.WeightKg,
            Notes = request.Notes
        };

        _context.BodyMetrics.Add(metric);
        await _context.SaveChangesAsync(cancellationToken);

        return metric.Id;
    }
}

public class LogBodyMetricCommandValidator : AbstractValidator<LogBodyMetricCommand>
{
    public LogBodyMetricCommandValidator()
    {
        RuleFor(x => x.WeightKg)
            .GreaterThan(0).WithMessage("Weight must be greater than zero.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");
    }
}
