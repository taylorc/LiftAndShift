using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.BodyMetrics;
using LiftAndShift.UseCases.BodyMetrics.List;
using LiftAndShift.Web.Extensions;
using FluentValidation;

namespace LiftAndShift.Web.BodyMetrics;

public class List(IMediator mediator) : Endpoint<ListBodyMetricsRequest, BodyMetricListResponse, ListBodyMetricsMapper>
{
  public override void Configure()
  {
    Get(ListBodyMetricsRequest.Route);
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "List a family member's bodyweight entries";
      s.Description = "Retrieves a paginated list of the family member's bodyweight entries, most-recent-first. Supports GitHub-style pagination with 1-based page indexing and configurable page size.";
      s.ExampleRequest = new ListBodyMetricsRequest { FamilyMemberId = 1, Page = 1, PerPage = 10 };

      s.Params["page"] = "1-based page index (default 1)";
      s.Params["per_page"] = $"Page size 1–{UseCases.Constants.MAX_PAGE_SIZE} (default {UseCases.Constants.DEFAULT_PAGE_SIZE})";

      s.Responses[200] = "Paginated list of bodyweight entries returned successfully";
      s.Responses[400] = "Invalid pagination parameters";
    });

    Tags("BodyMetrics");

    Description(builder => builder
      .Accepts<ListBodyMetricsRequest>()
      .Produces<BodyMetricListResponse>(200, "application/json")
      .ProducesProblem(400));
  }

  public override async Task HandleAsync(ListBodyMetricsRequest request, CancellationToken cancellationToken)
  {
    // ListBodyMetricsHandler always succeeds - pagination bounds are already enforced by
    // ListBodyMetricsValidator before this endpoint runs, so there's no failure branch to handle here.
    var result = await mediator.Send(
      new ListBodyMetricsQuery(FamilyMemberId.From(request.FamilyMemberId), request.Page, request.PerPage),
      cancellationToken);

    var pagedResult = result.Value;
    AddLinkHeader(request.FamilyMemberId, pagedResult.Page, pagedResult.PerPage, pagedResult.TotalPages);

    var response = Map.FromEntity(pagedResult);
    await Send.OkAsync(response, cancellationToken);
  }

  private void AddLinkHeader(int familyMemberId, int page, int perPage, int totalPages)
  {
    var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}";
    string Link(string rel, int p) => $"<{baseUrl}?page={p}&per_page={perPage}>; rel=\"{rel}\"";

    var parts = new List<string>();
    if (page > 1)
    {
      parts.Add(Link("first", 1));
      parts.Add(Link("prev", page - 1));
    }
    if (page < totalPages)
    {
      parts.Add(Link("next", page + 1));
      parts.Add(Link("last", totalPages));
    }

    if (parts.Count > 0)
      HttpContext.Response.Headers["Link"] = string.Join(", ", parts);
  }
}

public sealed class ListBodyMetricsRequest
{
  public const string Route = "/FamilyMembers/{FamilyMemberId:int}/BodyMetrics";
  public static string BuildRoute(int familyMemberId) => Route.BuildRoute(familyMemberId);

  public int FamilyMemberId { get; set; }

  [BindFrom("page")]
  public int Page { get; init; } = 1;

  [BindFrom("per_page")]
  public int PerPage { get; init; } = UseCases.Constants.DEFAULT_PAGE_SIZE;
}

public record BodyMetricListResponse : UseCases.PagedResult<BodyMetricRecord>
{
  public BodyMetricListResponse(IReadOnlyList<BodyMetricRecord> Items, int Page, int PerPage, int TotalCount, int TotalPages)
    : base(Items, Page, PerPage, TotalCount, TotalPages)
  {
  }
}

public sealed class ListBodyMetricsValidator : Validator<ListBodyMetricsRequest>
{
  public ListBodyMetricsValidator()
  {
    RuleFor(x => x.FamilyMemberId)
      .GreaterThan(0);

    RuleFor(x => x.Page)
      .GreaterThanOrEqualTo(1)
      .WithMessage("page must be >= 1");

    RuleFor(x => x.PerPage)
      .InclusiveBetween(1, UseCases.Constants.MAX_PAGE_SIZE)
      .WithMessage($"per_page must be between 1 and {UseCases.Constants.MAX_PAGE_SIZE}");
  }
}

public sealed class ListBodyMetricsMapper
  : Mapper<ListBodyMetricsRequest, BodyMetricListResponse, UseCases.PagedResult<BodyMetricDto>>
{
  public override BodyMetricListResponse FromEntity(UseCases.PagedResult<BodyMetricDto> e)
  {
    var items = e.Items
      .Select(s => BodyMetricRecord.FromDto(s))
      .ToList();

    return new BodyMetricListResponse(items, e.Page, e.PerPage, e.TotalCount, e.TotalPages);
  }
}
