using System.Net;
using Todo.Core.Common;

namespace Todo.Core.Exceptions;

/// <summary>
/// Thrown when request validation fails. Maps to HTTP 400 with field-specific errors.
/// </summary>
public sealed class ValidationException : ApplicationExceptionBase
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors, string message = "One or more validation errors occurred.")
        : base(ErrorCodes.ValidationError, HttpStatusCode.BadRequest, message)
    {
        Errors = errors;
    }
}
