namespace LAV.GraphDbFramework.Core;

public interface IQueryRunner
{
    ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters);
    ValueTask<IReadOnlyList<T>> RunAsync<T>(string query, object? parameters, Func<IRecord, T>? mapper);
}