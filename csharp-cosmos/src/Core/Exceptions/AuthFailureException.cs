using System.Net;
using Todo.Core.Common;

namespace Todo.Core.Exceptions;

/// <summary>Maps to HTTP 401 with a structured error code (invalid login, refresh token, etc.).</summary>
public sealed class AuthFailureException : ApplicationExceptionBase
{
    public AuthFailureException(string errorCode, string message)
        : base(errorCode, HttpStatusCode.Unauthorized, message)
    {
    }
}
