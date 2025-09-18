using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LAV.GraphDbFramework.Core.Mapping;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using LAV.GraphDbFramework.Core.Transactions;
using LAV.GraphDbFramework.Core;
using LAV.GraphDbFramework.Core.Exceptions;

namespace LAV.GraphDbFramework.Memgraph;

internal class MemgraphTransaction : IGraphDbTransaction, IGraphDbQueryRunner
{
	private readonly IAsyncTransaction _transaction;
	private readonly IAsyncSession _session;
	private readonly ILogger<MemgraphTransaction> _logger;
	private GraphDbTransactionStatus _status = GraphDbTransactionStatus.Active;
	private readonly List<string> _savepoints = new List<string>();

	public GraphDbTransactionStatus Status => _status;
	public Guid TransactionId { get; } = Guid.NewGuid();
	public DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;
	public TimeSpan Timeout { get; }

	public event EventHandler<GraphDbTransactionEventArgs> Committed;
	public event EventHandler<GraphDbTransactionEventArgs> RolledBack;
	public event EventHandler<GraphDbTransactionFailedEventArgs> Failed;

	public MemgraphTransaction(IAsyncSession session, IAsyncTransaction transaction,
		TimeSpan timeout, ILogger<MemgraphTransaction> logger)
	{
		_session = session;
		_transaction = transaction;
		Timeout = timeout;
		_logger = logger;

		_logger?.LogInformation("Transaction {TransactionId} started", TransactionId);
	}

	public async Task CommitAsync()
	{
		try
		{
			await _transaction.CommitAsync();
			_status = GraphDbTransactionStatus.Committed;

			_logger?.LogInformation("Transaction {TransactionId} committed", TransactionId);
			Committed?.Invoke(this, new GraphDbTransactionEventArgs(TransactionId));
		}
		catch (Exception ex)
		{
			_status = GraphDbTransactionStatus.Failed;
			_logger?.LogError(ex, "Failed to commit transaction {TransactionId}", TransactionId);
			Failed?.Invoke(this, new GraphDbTransactionFailedEventArgs(TransactionId, ex));
			throw new GraphDbTransactionException("Commit", ex);
		}
	}

	public async Task RollbackAsync()
	{
		try
		{
			await _transaction.RollbackAsync();
			_status = GraphDbTransactionStatus.RolledBack;

			_logger?.LogInformation("Transaction {TransactionId} rolled back", TransactionId);
			RolledBack?.Invoke(this, new GraphDbTransactionEventArgs(TransactionId));
		}
		catch (Exception ex)
		{
			_status = GraphDbTransactionStatus.Failed;
			_logger?.LogError(ex, "Failed to rollback transaction {TransactionId}", TransactionId);
			Failed?.Invoke(this, new GraphDbTransactionFailedEventArgs(TransactionId, ex));
			throw new GraphDbTransactionException("Rollback", ex);
		}
	}

	public async Task SavepointAsync(string name)
	{
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException("Savepoint name cannot be empty", nameof(name));

		try
		{
			await _transaction.RunAsync($"SAVEPOINT {name}");
			_savepoints.Add(name);

			_logger?.LogDebug("Savepoint {SavepointName} created in transaction {TransactionId}",
				name, TransactionId);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Failed to create savepoint {SavepointName} in transaction {TransactionId}",
				name, TransactionId);
			throw new GraphDbTransactionException("Savepoint", ex);
		}
	}

	public async Task RollbackToSavepointAsync(string name)
	{
		if (string.IsNullOrEmpty(name))
			throw new ArgumentException("Savepoint name cannot be empty", nameof(name));

		if (!_savepoints.Contains(name))
			throw new InvalidOperationException($"Savepoint {name} does not exist");

		try
		{
			await _transaction.RunAsync($"ROLLBACK TO SAVEPOINT {name}");

			_logger?.LogDebug("Rolled back to savepoint {SavepointName} in transaction {TransactionId}",
				name, TransactionId);
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Failed to rollback to savepoint {SavepointName} in transaction {TransactionId}",
				name, TransactionId);
			throw new GraphDbTransactionException("RollbackToSavepoint", ex);
		}
	}

	public async ValueTask DisposeAsync()
	{
		if (_status == GraphDbTransactionStatus.Active)
		{
			_logger?.LogWarning("Transaction {TransactionId} was not explicitly committed or rolled back", TransactionId);
			await RollbackAsync();
		}

		await _transaction.DisposeAsync();
		await _session.DisposeAsync();
	}

	// Реализация IQueryRunner для совместимости
	public async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters)
	{
		try
		{
			var result = await _transaction.RunAsync(query, parameters);
			var records = await result.ToListAsync();

			return records.Select(record => MapperCache<T>.MapFromRecord(new MemgraphRecord(record))).ToList();
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Query failed in transaction {TransactionId}: {Query}", TransactionId, query);
			throw new GraphDbQueryException(query, parameters ?? new object(), ex);
		}
	}

	public async ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<IGraphDbRecord, T> mapper)
	{
		try
		{
			var result = await _transaction.RunAsync(query, parameters);
			var records = await result.ToListAsync();

			return records.Select(record => mapper(new MemgraphRecord(record))).ToList();
		}
		catch (Exception ex)
		{
			_logger?.LogError(ex, "Query failed in transaction {TransactionId}: {Query}", TransactionId, query);
			throw new GraphDbQueryException(query, parameters ?? new object(), ex);
		}
	}
}