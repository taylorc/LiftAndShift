using System.ComponentModel.DataAnnotations;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.UseCases.FamilyMembers.Create;
using LiftAndShift.Web.Extensions;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace LiftAndShift.Web.FamilyMembers;

public class Create(IMediator mediator)
  : Endpoint<CreateFamilyMemberRequest,
          Results<Created<CreateFamilyMemberResponse>,
                          ValidationProblem,
                          ProblemHttpResult>>
{
  private readonly IMediator _mediator = mediator;

  public override void Configure()
  {
    Post(CreateFamilyMemberRequest.Route);
    AllowAnonymous();
    Summary(s =>
    {
      s.Summary = "Create a new family member";
      s.Description = "Creates a new family member with the provided name and PIN. The PIN is a 4-digit code used to select this family member's identity on a shared device; it is not authentication.";
      s.ExampleRequest = new CreateFamilyMemberRequest { Name = "Ada", Pin = "1234" };
      s.ResponseExamples[201] = new CreateFamilyMemberResponse(1, "Ada");

      s.Responses[201] = "Family member created successfully";
      s.Responses[400] = "Invalid input data - validation errors";
      s.Responses[500] = "Internal server error";
    });

    Tags("FamilyMembers");

    Description(builder => builder
      .Accepts<CreateFamilyMemberRequest>("application/json")
      .Produces<CreateFamilyMemberResponse>(201, "application/json")
      .ProducesProblem(400)
      .ProducesProblem(500));
  }

  public override async Task<Results<Created<CreateFamilyMemberResponse>, ValidationProblem, ProblemHttpResult>>
    ExecuteAsync(CreateFamilyMemberRequest request, CancellationToken cancellationToken)
  {
    var result = await _mediator.Send(
      new CreateFamilyMemberCommand(FamilyMemberName.From(request.Name!), Pin.From(request.Pin!)),
      cancellationToken);

    return result.ToCreatedResult(
      id => $"/FamilyMembers/{id}",
      id => new CreateFamilyMemberResponse(id.Value, request.Name!));
  }
}

public class CreateFamilyMemberRequest
{
  public const string Route = "/FamilyMembers";

  [Required]
  public string Name { get; set; } = string.Empty;
  [Required]
  public string Pin { get; set; } = string.Empty;
}

public class CreateFamilyMemberValidator : Validator<CreateFamilyMemberRequest>
{
  public CreateFamilyMemberValidator()
  {
    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("Name is required.")
      .MinimumLength(2)
      .MaximumLength(FamilyMemberName.MaxLength);

    RuleFor(x => x.Pin)
      .NotEmpty()
      .WithMessage("Pin is required.")
      .Matches("^[0-9]{4}$")
      .WithMessage("Pin must be exactly 4 digits.");
  }
}

public class CreateFamilyMemberResponse(int id, string name)
{
  public int Id { get; set; } = id;
  public string Name { get; set; } = name;
}
