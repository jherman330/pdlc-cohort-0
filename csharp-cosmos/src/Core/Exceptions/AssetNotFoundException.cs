using System.Net;
using Todo.Core.Common;

namespace Todo.Core.Exceptions;

/// <summary>
/// Thrown when an asset is not found. Maps to HTTP 404.
/// </summary>
public sealed class AssetNotFoundException : ApplicationExceptionBase
{
    public AssetNotFoundException(string message = "Asset not found.", Exception? innerException = null)
        : base(ErrorCodes.AssetNotFound, HttpStatusCode.NotFound, message, null, innerException) { }
}
