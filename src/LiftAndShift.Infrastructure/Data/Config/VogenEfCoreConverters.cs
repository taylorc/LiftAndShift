using LiftAndShift.Core.ContributorAggregate;
using LiftAndShift.Core.FamilyMemberAggregate;
using LiftAndShift.Core.ProgrammeAggregate;
using LiftAndShift.Core.WorkoutSessionAggregate;
using LiftAndShift.Core.WorkWeightAggregate;
using Vogen;

namespace LiftAndShift.Infrastructure.Data.Config;

[EfCoreConverter<ContributorId>]
[EfCoreConverter<ContributorName>]
[EfCoreConverter<FamilyMemberId>]
[EfCoreConverter<FamilyMemberName>]
[EfCoreConverter<Pin>]
[EfCoreConverter<ProgrammeId>]
[EfCoreConverter<TrainingPhase>]
[EfCoreConverter<WorkWeightId>]
[EfCoreConverter<WeightKg>]
[EfCoreConverter<WorkoutSessionId>]
[EfCoreConverter<LoggedSetId>]
internal partial class VogenEfCoreConverters;
