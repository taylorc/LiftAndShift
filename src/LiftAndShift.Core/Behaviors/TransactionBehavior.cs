using LiftAndShift.Core.Interfaces;

namespace LiftAndShift.Core.Behaviors;

/// <summary>
/// Wraps every ICommand in a database transaction via IUnitOfWork, so a handler that writes to more
/// than one aggregate (e.g. CreateFamilyMemberHandler adding a FamilyMember then a Programme) either
/// persists all of its writes or none of them - instead of each AddAsync/UpdateAsync committing
/// independently and leaving a partial state behind if a later write in the same handler fails.
/// Applies only to ICommand&lt;TResponse&gt;, not IQuery&lt;TResponse&gt; - reads never need this.
/// </summary>
public class TransactionBehavior<TCommand, TResponse>(IUnitOfWork unitOfWork)
  : IPipelineBehavior<TCommand, TResponse>
  where TCommand : ICommand<TResponse>
{
  public async ValueTask<TResponse> Handle(
    TCommand message,
    MessageHandlerDelegate<TCommand, TResponse> next,
    CancellationToken cancellationToken)
  {
    await unitOfWork.BeginTransactionAsync(cancellationToken);

    try
    {
      var response = await next(message, cancellationToken);

      if (IsSuccess(response))
      {
        await unitOfWork.CommitTransactionAsync(cancellationToken);
      }
      else
      {
        await unitOfWork.RollbackTransactionAsync(cancellationToken);
      }

      return response;
    }
    catch
    {
      await unitOfWork.RollbackTransactionAsync(cancellationToken);
      throw;
    }
  }

  private static bool IsSuccess(TResponse response) =>
    response is not IResult result || result.Status == ResultStatus.Ok;
}
