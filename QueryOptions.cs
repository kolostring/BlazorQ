using System.Runtime.CompilerServices;

namespace BlazorQ;

public static class QueryOptionsFactory
{
    public static QueryOptions<TKey, TRes> Create<TKey, TRes>(
        TKey key,
        Func<QueryHandlerExecutionContext<TKey>, Task<QueryResult<TRes>>> handler,
        Func<TRes, Task>? onSuccess = null,
        Func<QueryError, Task>? onError = null,
        Func<QueryResult<TRes>, Task>? onSettled = null,
        bool enabled = true,
        FetchOptions? fetchOptions = null,
        CacheOptions? cacheOptions = null) where TKey : ITuple
        => new(key, handler, onSuccess, onError, onSettled, enabled);
}

public sealed record QueryOptions<TKey, TRes>(
    TKey Key,
    Func<QueryHandlerExecutionContext<TKey>, Task<QueryResult<TRes>>> Handler,
    Func<TRes, Task>? OnSuccess = null,
    Func<QueryError, Task>? OnError = null,
    Func<QueryResult<TRes>, Task>? OnSettled = null,
    bool Enabled = true,
    FetchOptions? FetchOptions = null,
    CacheOptions? CacheOptions = null
) where TKey : ITuple;