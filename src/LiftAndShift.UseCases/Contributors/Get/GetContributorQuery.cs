using LiftAndShift.Core.ContributorAggregate;

namespace LiftAndShift.UseCases.Contributors.Get;

public record GetContributorQuery(ContributorId ContributorId) : IQuery<Result<ContributorDto>>;
