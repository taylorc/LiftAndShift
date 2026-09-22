using LiftAndShift.Core.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace LiftAndShift.Infrastructure.Data;

public class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
  private IDbContextTransaction? _transaction;

  public async Task BeginTransactionAsync(CancellationToken cancellationToken)
  {
    _transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
  }

  public async Task CommitTransactionAsync(CancellationToken cancellationToken)
  {
    if (_transaction == null) return;

    try
    {
      await _transaction.CommitAsync(cancellationToken);
    }
    finally
    {
      await _transaction.DisposeAsync();
      _transaction = null;
    }
  }

  public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
  {
    if (_transaction == null) return;

    try
    {
      await _transaction.RollbackAsync(cancellationToken);
    }
    finally
    {
      await _transaction.DisposeAsync();
      _transaction = null;
    }
  }
}
