namespace LiftAndShift.Core.Interfaces;

/// <summary>
/// Wraps a handler's repository writes in a single database transaction, so a sequence of
/// AddAsync/UpdateAsync calls against more than one aggregate either all persist or none do.
/// Implemented in Infrastructure over the EF Core DbContext; consumed here only through this
/// interface, per Clean Architecture layering.
/// </summary>
public interface IUnitOfWork
{
  Task BeginTransactionAsync(CancellationToken cancellationToken);
  Task CommitTransactionAsync(CancellationToken cancellationToken);
  Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
