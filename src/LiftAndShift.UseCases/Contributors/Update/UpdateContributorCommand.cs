using LiftAndShift.Core.ContributorAggregate;

namespace LiftAndShift.UseCases.Contributors.Update;

public record UpdateContributorCommand(ContributorId ContributorId, ContributorName NewName) : ICommand<Result<ContributorDto>>;
