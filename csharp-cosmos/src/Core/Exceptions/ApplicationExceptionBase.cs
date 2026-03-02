using System.Net;

namespace Todo.Core.Exceptions;

/// <summary>
/// Base type for application exceptions that map to HTTP status codes and structured error responses.
/// </summary>
public abstract class ApplicationExceptionBase : Exception
{
    public string ErrorCode { get; }
    public HttpStatusCode StatusCode { get; }
    public string? Field { get; }

    protected ApplicationExceptionBase(string errorCode, HttpStatusCode statusCode, string message, string? field = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Field = field;
    }
}
