using LiftAndShift.Core.ContributorAggregate;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.ProgrammeAggregate;
using Vogen;

namespace LiftAndShift.Infrastructure.Data.Config;

[EfCoreConverter<ContributorId>]
[EfCoreConverter<ContributorName>]
[EfCoreConverter<FamilyMemberId>]
[EfCoreConverter<FamilyMemberName>]
[EfCoreConverter<Pin>]
[EfCoreConverter<ProgrammeId>]
[EfCoreConverter<TrainingPhase>]
internal partial class VogenEfCoreConverters;
