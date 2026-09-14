using LiftAndShift.Core.ContributorAggregate;

namespace LiftAndShift.UseCases.Contributors;
public record ContributorDto(ContributorId Id, ContributorName Name, PhoneNumber PhoneNumber);
