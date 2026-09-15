namespace LiftAndShift.UnitTests.UseCases.FamilyMembers;

public class CreateFamilyMemberHandlerHandleTests
{
  private readonly FamilyMemberName _testName = FamilyMemberName.From("Ada");
  private readonly Pin _testPin = Pin.From("1234");
  private readonly IRepository<FamilyMember> _familyMemberRepository = Substitute.For<IRepository<FamilyMember>>();
  private readonly IRepository<Programme> _programmeRepository = Substitute.For<IRepository<Programme>>();
  private readonly CreateFamilyMemberHandler _handler;

  public CreateFamilyMemberHandlerHandleTests()
  {
    _handler = new CreateFamilyMemberHandler(_familyMemberRepository, _programmeRepository);
  }

  private FamilyMember CreateFamilyMember()
  {
    return new FamilyMember(_testName, _testPin);
  }

  [Fact]
  public async Task ReturnsSuccessGivenValidNameAndPin()
  {
    _familyMemberRepository.AddAsync(Arg.Any<FamilyMember>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(CreateFamilyMember()));
    var result = await _handler.Handle(new CreateFamilyMemberCommand(_testName, _testPin), CancellationToken.None);

    result.IsSuccess.ShouldBeTrue();
  }

  [Fact]
  public async Task CreatesProgrammeAtTrainingPhase1()
  {
    var createdFamilyMember = CreateFamilyMember();
    _familyMemberRepository.AddAsync(Arg.Any<FamilyMember>(), Arg.Any<CancellationToken>())
      .Returns(Task.FromResult(createdFamilyMember));

    Programme? capturedProgramme = null;
    _ = _programmeRepository.AddAsync(Arg.Do<Programme>(p => capturedProgramme = p), Arg.Any<CancellationToken>());
    _programmeRepository.ClearReceivedCalls();

    await _handler.Handle(new CreateFamilyMemberCommand(_testName, _testPin), CancellationToken.None);

    await _programmeRepository.Received(1).AddAsync(Arg.Any<Programme>(), Arg.Any<CancellationToken>());
    capturedProgramme.ShouldNotBeNull();
    capturedProgramme!.TrainingPhase.ShouldBe(TrainingPhase.From(1));
  }
}
