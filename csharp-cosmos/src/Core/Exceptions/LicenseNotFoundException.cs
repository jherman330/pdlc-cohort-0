using System.Net;
using Todo.Core.Common;

namespace Todo.Core.Exceptions;

/// <summary>
/// Thrown when a license is not found. Maps to HTTP 404.
/// </summary>
public sealed class LicenseNotFoundException : ApplicationExceptionBase
{
    public LicenseNotFoundException(string message = "License not found.", Exception? innerException = null)
        : base(ErrorCodes.LicenseNotFound, HttpStatusCode.NotFound, message, null, innerException) { }
}
