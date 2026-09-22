using Ardalis.Specification;

namespace LiftAndShift.UnitTests.UseCases.PersonalRecords;

public class GetPersonalRecordsHandlerHandleTests
{
  private readonly FamilyMemberId _testFamilyMemberId = FamilyMemberId.From(1);
  private readonly IReadRepository<FamilyMember> _familyMemberRepository = Substitute.For<IReadRepository<FamilyMember>>();
  private readonly IPersonalRecordsQueryService _query = Substitute.For<IPersonalRecordsQueryService>();
  private readonly GetPersonalRecordsHandler _handler;

  public GetPersonalRecordsHandlerHandleTests()
  {
    _handler = new GetPersonalRecordsHandler(_familyMemberRepository, _query);
  }

  [Fact]
  public async Task ReturnsNotFoundGivenNonexistentFamilyMember()
  {
    var result = await _handler.Handle(new GetPersonalRecordsQuery(_testFamilyMemberId), CancellationToken.None);

    result.Status.ShouldBe(Ardalis.Result.ResultStatus.NotFound);
  }

  [Fact]
  public async Task ReturnsEmptyListGivenFamilyMemberWithNoRecords()
  {
    _familyMemberRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<FamilyMember>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<FamilyMember?>(new FamilyMember(FamilyMemberName.From("Ada"), Pin.From("1234"))));
    _query.GetAsync(_testFamilyMemberId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<IReadOnlyList<PersonalRecordDto>>([]));

    var result = await _handler.Handle(new GetPersonalRecordsQuery(_testFamilyMemberId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldBeEmpty();
  }

  [Fact]
  public async Task ReturnsRecordsFromTheQueryService()
  {
    _familyMemberRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<FamilyMember>>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<FamilyMember?>(new FamilyMember(FamilyMemberName.From("Ada"), Pin.From("1234"))));

    var record = new PersonalRecordDto(Lift.Squat, WeightKg.From(60), new DateOnly(2026, 1, 1), WorkoutSessionId.From(1));
    _query.GetAsync(_testFamilyMemberId, Arg.Any<CancellationToken>())
      .Returns(Task.FromResult<IReadOnlyList<PersonalRecordDto>>([record]));

    var result = await _handler.Handle(new GetPersonalRecordsQuery(_testFamilyMemberId), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
    result.Value.ShouldContain(r => r.Lift == Lift.Squat && r.WeightKg == WeightKg.From(60));
  }
}
