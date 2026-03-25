namespace Todo.Core.Authentication;

/// <summary>
/// Permission claim values (fine-grained RBAC). Policies use the same string.
/// </summary>
public static class Permissions
{
    public const string Ping = "ping:access";
    public const string AssetsRead = "assets:read";
    public const string AssetsWrite = "assets:write";
    public const string LicensesRead = "licenses:read";
    public const string LicensesWrite = "licenses:write";
    public const string ProcurementRead = "procurement:read";
    public const string ProcurementWrite = "procurement:write";
    public const string ReportsRead = "reports:read";
    public const string RolesAssign = "roles:assign";
}
