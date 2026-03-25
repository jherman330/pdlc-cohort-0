using System.Net;
using Todo.Core.Common;

namespace Todo.Core.Exceptions;

/// <summary>HTTP 403 with application error code.</summary>
public sealed class ForbiddenOperationException : ApplicationExceptionBase
{
    public ForbiddenOperationException(string message)
        : base(ErrorCodes.Forbidden, HttpStatusCode.Forbidden, message)
    {
    }
}
