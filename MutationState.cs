namespace BlazorQ;

public class MutationState<TParams, TResponse>(
    Func<MutationHandlerExecutionContext<TParams>, Task<QueryResult<TResponse>>> handler,
    IServiceProvider serviceProvider
) 
{
    private QueryResult<TResponse>? _result;
    private int _runningMutationsQuantity = 0;

    public event Action? OnChanged;

    public TResponse? Data => _result == null ? default : _result.Value;
    public QueryError? Error => _result?.Error;

    public bool IsIdle => _runningMutationsQuantity == 0;
    public bool IsFetching => _runningMutationsQuantity > 0;
    public bool IsError => IsIdle && _result?.IsFailure == true;
    public bool IsSuccess => IsIdle && _result?.IsSuccess == true;

    public async Task<QueryResult<TResponse>> ExecuteAsync(TParams variables, CancellationToken ct = default)
    {
        _runningMutationsQuantity++;
        NotifyChanged();

        try
        {
            var ctx = new MutationHandlerExecutionContext<TParams>
            {
                Params = variables,
                ServiceProvider = serviceProvider,
                CancellationToken = ct
            };

            _result = await handler(ctx);
        }
        catch (Exception ex)
        {
            _result = QueryResult.Failure<TResponse>(new QueryError("Mutation.Exception", ex.Message));
        }
        finally
        {
            _runningMutationsQuantity--;
            NotifyChanged();
        }

        return _result;
    }

    private void NotifyChanged() => OnChanged?.Invoke();

    public void Reset()
    {
        _result = null;
        NotifyChanged();
    }
}