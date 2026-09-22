using Ardalis.Result;

namespace LiftAndShift.UnitTests.Core.Behaviors;

public class TransactionBehaviorTests
{
  private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
  private readonly TransactionBehavior<CreateFamilyMemberCommand, Result<FamilyMemberId>> _behavior;

  public TransactionBehaviorTests()
  {
    _behavior = new TransactionBehavior<CreateFamilyMemberCommand, Result<FamilyMemberId>>(_unitOfWork);
  }

  private static CreateFamilyMemberCommand TestCommand() =>
    new(FamilyMemberName.From("Ada"), Pin.From("1234"));

  private static Result<FamilyMemberId> Succeed() => Result.Success(FamilyMemberId.From(1));
  private static Result<FamilyMemberId> Fail() => Result.Invalid(new ValidationError { ErrorMessage = "nope" });

  [Fact]
  public async Task CommitsWhenTheHandlerSucceeds()
  {
    var response = await _behavior.Handle(
      TestCommand(), (_, _) => ValueTask.FromResult(Succeed()), CancellationToken.None);

    response.Value.ShouldBe(FamilyMemberId.From(1));
    await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
    await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
    await _unitOfWork.DidNotReceive().RollbackTransactionAsync(Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task RollsBackWhenTheHandlerReturnsAFailureResult()
  {
    var response = await _behavior.Handle(
      TestCommand(), (_, _) => ValueTask.FromResult(Fail()), CancellationToken.None);

    response.IsSuccess.ShouldBeFalse();
    await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
    await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task RollsBackAndRethrowsWhenTheHandlerThrows()
  {
    await Should.ThrowAsync<InvalidOperationException>(() =>
      _behavior.Handle(TestCommand(), (_, _) => throw new InvalidOperationException("boom"), CancellationToken.None).AsTask());

    await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
    await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
  }
}
