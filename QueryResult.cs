namespace BlazorQ;

public abstract record QueryResult<T>
{
  public sealed record Success(T Value) : QueryResult<T>;
  public sealed record Failure(QueryError Error) : QueryResult<T>;
}

public sealed record QueryError(string Code, string Message)
{
  public static readonly QueryError None = new(string.Empty, string.Empty);
  public static readonly QueryError NullValue = new("Error.NullValue", "Null Value");
}