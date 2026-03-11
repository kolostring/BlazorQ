namespace BlazorQ;

public record FetchOptions(TimeSpan? RefetchInterval = null, TimeSpan? StaleTime = null);
