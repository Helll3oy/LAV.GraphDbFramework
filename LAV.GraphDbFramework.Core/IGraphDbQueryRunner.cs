namespace LAV.GraphDbFramework.Core;

public interface IGraphDbQueryRunner : IAsyncDisposable
{
    ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters);
    ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<IGraphDbRecord, T> mapper);
}