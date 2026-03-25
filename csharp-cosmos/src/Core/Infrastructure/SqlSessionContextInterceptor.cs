using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Options;
using Todo.Core.Authentication;
using Todo.Core.Configuration;

namespace Todo.Core.Infrastructure;

/// <summary>
/// Sets SQL Server SESSION_CONTEXT for row-level security (TenantId) before each command.
/// </summary>
public sealed class SqlSessionContextInterceptor : DbCommandInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOptions<AzureSqlSettings> _sqlSettings;

    public SqlSessionContextInterceptor(
        IHttpContextAccessor httpContextAccessor,
        IOptions<AzureSqlSettings> sqlSettings)
    {
        _httpContextAccessor = httpContextAccessor;
        _sqlSettings = sqlSettings;
    }

    public override InterceptionResult<object> ScalarExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result)
    {
        SetSessionContext(command);
        return base.ScalarExecuting(command, eventData, result);
    }

    public override InterceptionResult<int> NonQueryExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result)
    {
        SetSessionContext(command);
        return base.NonQueryExecuting(command, eventData, result);
    }

    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        SetSessionContext(command);
        return base.ReaderExecuting(command, eventData, result);
    }

    public override async ValueTask<InterceptionResult<object>> ScalarExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<object> result,
        CancellationToken cancellationToken = default)
    {
        await SetSessionContextAsync(command, cancellationToken).ConfigureAwait(false);
        return await base.ScalarExecutingAsync(command, eventData, result, cancellationToken).ConfigureAwait(false);
    }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await SetSessionContextAsync(command, cancellationToken).ConfigureAwait(false);
        return await base.NonQueryExecutingAsync(command, eventData, result, cancellationToken).ConfigureAwait(false);
    }

    public override async ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        await SetSessionContextAsync(command, cancellationToken).ConfigureAwait(false);
        return await base.ReaderExecutingAsync(command, eventData, result, cancellationToken).ConfigureAwait(false);
    }

    private string? ResolveTenantId()
    {
        var user = _httpContextAccessor.HttpContext?.RequestServices.GetService(typeof(ICurrentUser)) as ICurrentUser;
        if (!string.IsNullOrEmpty(user?.TenantId))
            return user.TenantId;
        var fallback = _sqlSettings.Value.DefaultTenantId;
        return string.IsNullOrEmpty(fallback) ? null : fallback;
    }

    private void SetSessionContext(DbCommand command)
    {
        var tenantId = ResolveTenantId();
        if (string.IsNullOrEmpty(tenantId) || command.Connection is not SqlConnection conn)
            return;
        if (conn.State != ConnectionState.Open)
            return;
        using var ctx = conn.CreateCommand();
        ctx.Transaction = command.Transaction as SqlTransaction;
        ctx.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@tenant, @read_only=1;";
        var p = ctx.CreateParameter();
        p.ParameterName = "@tenant";
        p.Value = tenantId;
        p.SqlDbType = SqlDbType.NVarChar;
        p.Size = 128;
        ctx.Parameters.Add(p);
        ctx.ExecuteNonQuery();
    }

    private async Task SetSessionContextAsync(DbCommand command, CancellationToken cancellationToken)
    {
        var tenantId = ResolveTenantId();
        if (string.IsNullOrEmpty(tenantId) || command.Connection is not SqlConnection conn)
            return;
        if (conn.State != ConnectionState.Open)
            return;
        await using var ctx = conn.CreateCommand();
        ctx.Transaction = command.Transaction as SqlTransaction;
        ctx.CommandText = "EXEC sp_set_session_context @key=N'TenantId', @value=@tenant, @read_only=1;";
        var p = ctx.CreateParameter();
        p.ParameterName = "@tenant";
        p.Value = tenantId;
        p.SqlDbType = SqlDbType.NVarChar;
        p.Size = 128;
        ctx.Parameters.Add(p);
        await ctx.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
