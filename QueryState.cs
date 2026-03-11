using System.Runtime.CompilerServices;

namespace BlazorQ;

public enum QueryStatus
{
    Idle,
    Fetching
}

public class QueryState<TKey, TResponse>(
    TKey key,
    Func<QueryHandlerExecutionContext<TKey>, Task<QueryResult<TResponse>>> handler,
    CacheOptions cacheOptions,
    IServiceProvider serviceProvider
)
    : IQueryState<TResponse> where TKey : ITuple
{
    public TKey Key { get; } = key;
    private QueryResult<TResponse>? _result;
    private QueryStatus _status = QueryStatus.Idle;

    private int _observersCount = 0;
    private DateTimeOffset _lastUpdatedAt = DateTimeOffset.MinValue;
    public CacheOptions _cacheOptions = cacheOptions;

    public event Action? OnChanged;
    public event Action? OnInvalidated;
    public event Action<TKey, CacheOptions>? OnLastSubscriberRemoved;
    public event Action<TKey>? OnFirstSubscriberAdded;

    public TResponse? Data => _result == null ? default : _result.Value;
    public QueryError? Error => _result?.Error;

    public QueryResult<TResponse>? Res
    {
        get => _result;
    }

    public void SetData(TResponse data)
    {
        _result = QueryResult.Success(data);
        _status = QueryStatus.Idle;
        _lastUpdatedAt = DateTimeOffset.UtcNow;
        NotifyChanged();
    }

    public void SetCacheOptions(CacheOptions cacheOptions)
    {
        if (cacheOptions.GcTime > _cacheOptions.GcTime)
        {
            _cacheOptions = cacheOptions;
        }
    }

    public bool IsIdle => _status == QueryStatus.Idle;
    public bool IsFetching => _status == QueryStatus.Fetching;
    public bool IsLoading => IsFetching && _result == null;
    public bool IsPending => _result == null;
    public bool IsError => IsIdle && _result?.IsFailure == true;
    public bool IsSuccess => IsIdle && _result?.IsSuccess == true;
    public bool CanFetch => IsIdle && _observersCount > 0;

    public DateTimeOffset LastUpdatedAt => _lastUpdatedAt;

    public async Task Run(CancellationToken ct)
    {
        if (IsFetching)
        {
            return;
        }

        _status = QueryStatus.Fetching;
        NotifyChanged();

        try
        {
            var ctx = new QueryHandlerExecutionContext<TKey>()
            {
                Key = Key,
                ServiceProvider = serviceProvider,
                CancellationToken = ct
            };
            _result = await handler(ctx);
        }
        finally
        {
            _lastUpdatedAt = DateTimeOffset.UtcNow;
            _status = QueryStatus.Idle;
        }

        NotifyChanged();
    }

    private void NotifyChanged() => OnChanged?.Invoke();

    public void NotifyInvalidated()
    {
        _lastUpdatedAt = DateTimeOffset.MinValue;
        OnInvalidated?.Invoke();
    }

    public void IncrementObservers()
    {
        if (_observersCount == 0)
        {
            OnFirstSubscriberAdded?.Invoke(Key);
        }

        _observersCount++;
    }

    public void DecrementObservers()
    {
        _observersCount--;
        if (_observersCount == 0)
        {
            OnLastSubscriberRemoved?.Invoke(Key, _cacheOptions);
        }

        if (_observersCount < 0)
        {
            throw new InvalidOperationException(
                $"Query state with key {Key} has invalid observer count: {_observersCount}.");
        }
    }
}