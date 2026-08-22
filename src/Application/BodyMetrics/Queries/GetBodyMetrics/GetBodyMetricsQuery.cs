using LiftAndShift.Application.Common.Interfaces;
using LiftAndShift.Application.Common.Security;

namespace LiftAndShift.Application.BodyMetrics.Queries.GetBodyMetrics;

public record BodyMetricDto
{
    public int Id { get; init; }
    public DateTimeOffset Date { get; init; }
    public decimal WeightKg { get; init; }
    public string? Notes { get; init; }
}

[Authorize]
public record GetBodyMetricsQuery : IRequest<List<BodyMetricDto>>;

public class GetBodyMetricsQueryHandler : IRequestHandler<GetBodyMetricsQuery, List<BodyMetricDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetBodyMetricsQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async ValueTask<List<BodyMetricDto>> Handle(GetBodyMetricsQuery request, CancellationToken cancellationToken)
    {
        return await _context.BodyMetrics
            .Where(m => m.UserId == _currentUser.Id)
            .OrderBy(m => m.Date)
            .Select(m => new BodyMetricDto
            {
                Id = m.Id,
                Date = m.Date,
                WeightKg = m.WeightKg,
                Notes = m.Notes
            })
            .ToListAsync(cancellationToken);
    }
}
