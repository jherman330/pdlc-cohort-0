using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Todo.Core.Common;

namespace Todo.Core.Middleware;

/// <summary>
/// Middleware placeholder for pipeline order. Model state validation runs in ValidationFilter (after model binding).
/// </summary>
public class ValidationMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context) => _next(context);
}

/// <summary>
/// Validates model state before controller actions. Returns standardized 400 with field-specific errors per Backend blueprint.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(ms => ms.Value?.Errors.Count > 0)
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value!.Errors.Select(e => e.ErrorMessage).ToArray());
            var apiErrors = errors.SelectMany(kv => kv.Value.Select(msg => new ApiError
            {
                Code = ErrorCodes.ValidationError,
                Message = msg,
                Field = kv.Key
            })).ToArray();
            context.Result = new BadRequestObjectResult(new ApiErrorResponse { Errors = apiErrors });
            return;
        }

        await next();
    }
}
