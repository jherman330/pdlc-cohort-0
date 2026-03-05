using System.Net;
using Todo.Core.Common;

namespace Todo.Core.Exceptions;

/// <summary>
/// Thrown when a data access operation fails (Cosmos, SQL, etc.). Maps to the underlying HTTP/database status.
/// </summary>
public sealed class DataAccessException : ApplicationExceptionBase
{
    public DataAccessException(string operation, string? id, string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, Exception? innerException = null)
        : base(ErrorCodes.InternalError, statusCode, $"{operation} failed{(id != null ? $" for id '{id}'" : "")}: {message}", null, innerException) { }
}
