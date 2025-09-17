namespace LAV.GraphDbFramework.Core;

public interface IGraphDbQueryRunner : IGraphDbQueryRunner<IGraphDbRecord>
{

}

public interface IGraphDbQueryRunner<TGraphDbRecord> : IAsyncDisposable
    where TGraphDbRecord : IGraphDbRecord
{
    ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters);
    ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<TGraphDbRecord, T> mapper);
}