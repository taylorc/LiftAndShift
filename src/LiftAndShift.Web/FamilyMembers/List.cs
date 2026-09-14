using LiftAndShift.UseCases.FamilyMembers;
using LiftAndShift.UseCases.FamilyMembers.List;
using FluentValidation;

namespace LiftAndShift.Web.FamilyMembers;

public class List(IMediator mediator) : Endpoint<ListFamilyMembersRequest, FamilyMemberListResponse, ListFamilyMembersMapper>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Get("/FamilyMembers");
    AllowAnonymous();

    Summary(s =>
    {
      s.Summary = "List family members with pagination";
      s.Description = "Retrieves a paginated list of all family members. Supports GitHub-style pagination with 1-based page indexing and configurable page size.";
      s.ExampleRequest = new ListFamilyMembersRequest { Page = 1, PerPage = 10 };
      s.ResponseExamples[200] = new FamilyMemberListResponse(
        new List<FamilyMemberRecord>
        {
          new(1, "Ada", "1234"),
          new(2, "Grace", "5678")
        },
        1, 10, 2, 1);

      s.Params["page"] = "1-based page index (default 1)";
      s.Params["per_page"] = $"Page size 1–{UseCases.Constants.MAX_PAGE_SIZE} (default {UseCases.Constants.DEFAULT_PAGE_SIZE})";

      s.Responses[200] = "Paginated list of family members returned successfully";
      s.Responses[400] = "Invalid pagination parameters";
    });

    Tags("FamilyMembers");

    Description(builder => builder
      .Accepts<ListFamilyMembersRequest>()
      .Produces<FamilyMemberListResponse>(200, "application/json")
      .ProducesProblem(400));
  }

  public override async Task HandleAsync(ListFamilyMembersRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(new ListFamilyMembersQuery(request.Page, request.PerPage));
    if (!result.IsSuccess)
    {
      await Send.ErrorsAsync(statusCode: 400, cancellationToken);
      return;
    }

    var pagedResult = result.Value;
    AddLinkHeader(pagedResult.Page, pagedResult.PerPage, pagedResult.TotalPages);

    var response = Map.FromEntity(pagedResult);
    await Send.OkAsync(response, cancellationToken);
  }

  private void AddLinkHeader(int page, int perPage, int totalPages)
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

public sealed class ListFamilyMembersRequest
{
  [BindFrom("page")]
  public int Page { get; init; } = 1;

  [BindFrom("per_page")]
  public int PerPage { get; init; } = UseCases.Constants.DEFAULT_PAGE_SIZE;
}

public record FamilyMemberListResponse : UseCases.PagedResult<FamilyMemberRecord>
{
  public FamilyMemberListResponse(IReadOnlyList<FamilyMemberRecord> Items, int Page, int PerPage, int TotalCount, int TotalPages)
    : base(Items, Page, PerPage, TotalCount, TotalPages)
  {
  }
}

public sealed class ListFamilyMembersValidator : Validator<ListFamilyMembersRequest>
{
  public ListFamilyMembersValidator()
  {
    RuleFor(x => x.Page)
      .GreaterThanOrEqualTo(1)
      .WithMessage("page must be >= 1");

    RuleFor(x => x.PerPage)
      .InclusiveBetween(1, UseCases.Constants.MAX_PAGE_SIZE)
      .WithMessage($"per_page must be between 1 and {UseCases.Constants.MAX_PAGE_SIZE}");
  }
}

public sealed class ListFamilyMembersMapper
  : Mapper<ListFamilyMembersRequest, FamilyMemberListResponse, UseCases.PagedResult<FamilyMemberDto>>
{
  public override FamilyMemberListResponse FromEntity(UseCases.PagedResult<FamilyMemberDto> e)
  {
    var items = e.Items
      .Select(f => new FamilyMemberRecord(f.Id.Value, f.Name.Value, f.Pin.Value))
      .ToList();

    return new FamilyMemberListResponse(items, e.Page, e.PerPage, e.TotalCount, e.TotalPages);
  }
}
