namespace BlazorQ;

public class MutationHandlerExecutionContext<TParams>
{
    public TParams Params { get; set; } = default!;
    public IServiceProvider ServiceProvider { get; set; } = default!;
    public CancellationToken CancellationToken { get; set; }
}