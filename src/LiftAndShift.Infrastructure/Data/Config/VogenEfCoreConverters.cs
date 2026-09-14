using LiftAndShift.Core.ContributorAggregate;
using Vogen;

namespace LiftAndShift.Infrastructure.Data.Config;

[EfCoreConverter<ContributorId>]
[EfCoreConverter<ContributorName>]
internal partial class VogenEfCoreConverters;
