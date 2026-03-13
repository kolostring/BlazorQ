namespace BlazorQ;

public sealed record FetchOptions(TimeSpan? RefetchInterval = null, TimeSpan? StaleTime = null);
