using System.Net;
using Todo.Core.Common;

namespace Todo.Core.Exceptions;

/// <summary>
/// Thrown when an entity is not found by id. Maps to HTTP 404. Used by generic repositories.
/// </summary>
public sealed class EntityNotFoundException : ApplicationExceptionBase
{
    public EntityNotFoundException(string entityName, string id, Exception? innerException = null)
        : base(ErrorCodes.NotFound, HttpStatusCode.NotFound, $"{entityName} with id '{id}' was not found.", null, innerException) { }
}
