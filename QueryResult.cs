using System.Diagnostics.CodeAnalysis;

namespace BlazorQ;

public record QueryError(string Code, string Message)
{
    public static readonly QueryError None = new(string.Empty, string.Empty);
    public static readonly QueryError NullValue = new("Error.NullValue", "Null Value");
}

public class QueryResult
{
    protected QueryResult(bool isSuccess, QueryError error)
    {
        switch (isSuccess)
        {
            case true when error != QueryError.None:
                throw new InvalidOperationException();

            case false when error == QueryError.None:
                throw new InvalidOperationException();

            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public QueryError Error { get; }

    public static QueryResult Success() => new(true, QueryError.None);
    public static QueryResult Failure(QueryError error) => new(false, error);

    public static QueryResult<T> Success<T>(T value) => new(value, true, QueryError.None);
    public static QueryResult<T> Failure<T>(QueryError error) => new(default, false, error);

    public static QueryResult<T> Create<T>(T? value) =>
        value is not null ? Success(value) : Failure<T>(QueryError.NullValue);
}

public class QueryResult<T> : QueryResult
{
    private readonly T? _value;

    protected internal QueryResult(T? value, bool isSuccess, QueryError error) : base(isSuccess, error)
        => _value = value;

    [NotNull] public T Value => _value! ?? throw new InvalidOperationException("Result has no value");

    public static implicit operator QueryResult<T>(T? value) => Create(value);
}