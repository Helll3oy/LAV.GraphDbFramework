using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAV.GraphDbFramework.Core.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source) where T : class
    {
        return source.Where(item => item is not null);
    }

    public static async IAsyncEnumerable<T> WhereNotNullAsync<T>(this IAsyncEnumerable<T> source) where T : class
    {
        await foreach (var item in source.ConfigureAwait(false))
        {
            if (item is not null)
                yield return item;
        }
    }
}