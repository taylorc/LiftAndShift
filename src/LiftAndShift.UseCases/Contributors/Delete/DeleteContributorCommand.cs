using LiftAndShift.Core.ContributorAggregate;

namespace LiftAndShift.UseCases.Contributors.Delete;

public record DeleteContributorCommand(ContributorId ContributorId) : ICommand<Result>;
