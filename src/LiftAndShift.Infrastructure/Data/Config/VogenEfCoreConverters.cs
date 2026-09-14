using LiftAndShift.Core.ContributorAggregate;
using LiftAndShift.Core.FamilyMemberAggregate;
using Vogen;

namespace LiftAndShift.Infrastructure.Data.Config;

[EfCoreConverter<ContributorId>]
[EfCoreConverter<ContributorName>]
[EfCoreConverter<FamilyMemberId>]
[EfCoreConverter<FamilyMemberName>]
[EfCoreConverter<Pin>]
internal partial class VogenEfCoreConverters;
