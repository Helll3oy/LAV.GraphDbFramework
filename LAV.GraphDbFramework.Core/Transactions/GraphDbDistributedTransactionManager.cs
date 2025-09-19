using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core.Exceptions;
using Microsoft.Extensions.Logging;

namespace LAV.GraphDbFramework.Core.Transactions;

public class GraphDbDistributedTransactionManager : IAsyncDisposable
{
	private readonly List<IGraphDbTransaction> _transactions = new List<IGraphDbTransaction>();
	private readonly ILogger<GraphDbDistributedTransactionManager> _logger;
	private bool _disposed;

	public GraphDbDistributedTransactionManager(ILogger<GraphDbDistributedTransactionManager> logger = null)
	{
		_logger = logger;
	}

	public void EnlistTransaction(IGraphDbTransaction transaction)
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(GraphDbDistributedTransactionManager));

		_transactions.Add(transaction);
		_logger?.LogDebug("Transaction {TransactionId} enlisted in distributed transaction", transaction.TransactionId);
	}

	public async Task CommitAllAsync()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(GraphDbDistributedTransactionManager));

		var committedTransactions = new List<IGraphDbTransaction>();

		try
		{
			// Фаза подготовки (Two-Phase Commit)
			foreach (var transaction in _transactions)
			{
				if (transaction.Status == GraphDbTransactionStatus.Active)
				{
					// В Neo4j нет явной фазы подготовки, поэтому сразу коммитим
					await transaction.CommitAsync();
					committedTransactions.Add(transaction);
				}
			}

			_logger?.LogInformation("All transactions committed successfully");
		}
		catch (Exception ex)
		{
			// Откатываем уже закоммиченные транзакции (компенсирующие действия)
			await RollbackCommittedTransactionsAsync(committedTransactions);

			_logger?.LogError(ex, "Failed to commit distributed transaction");
			throw new GraphDbTransactionException("Distributed commit", ex);
		}
	}

	public async Task RollbackAllAsync()
	{
		if (_disposed)
			throw new ObjectDisposedException(nameof(GraphDbDistributedTransactionManager));

		foreach (var transaction in _transactions)
		{
			if (transaction.Status == GraphDbTransactionStatus.Active)
			{
				try
				{
					await transaction.RollbackAsync();
				}
				catch (Exception ex)
				{
					_logger?.LogError(ex, "Failed to rollback transaction {TransactionId}", transaction.TransactionId);
					// Продолжаем откатывать остальные транзакции
				}
			}
		}
	}

	private async Task RollbackCommittedTransactionsAsync(List<IGraphDbTransaction> committedTransactions)
	{
		// В графовых БД компенсирующие действия обычно требуют специальной логики,
		// так как откат уже закоммиченной транзакции невозможен
		foreach (var transaction in committedTransactions)
		{
			_logger?.LogWarning("Cannot rollback committed transaction {TransactionId}, " +
							   "compensating actions may be required", transaction.TransactionId);
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_disposed) return;

		_disposed = true;

		// При dispose откатываем все активные транзакции
		await RollbackAllAsync();
		_transactions.Clear();
	}
}