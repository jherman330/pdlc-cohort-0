namespace Todo.Core.Authentication;

/// <summary>
/// Maps each <see cref="UserRole"/> to granted permissions (WO-2 RBAC).
/// </summary>
public static class RolePermissionRegistry
{
    /// <summary>All permission values used for authorization policy registration.</summary>
    public static IReadOnlyList<string> AllPermissionValues { get; } =
        new[]
        {
            Permissions.Ping,
            Permissions.AssetsRead,
            Permissions.AssetsWrite,
            Permissions.LicensesRead,
            Permissions.LicensesWrite,
            Permissions.ProcurementRead,
            Permissions.ProcurementWrite,
            Permissions.ReportsRead,
            Permissions.RolesAssign,
        };

    private static readonly IReadOnlyDictionary<UserRole, IReadOnlySet<string>> Map = new Dictionary<UserRole, IReadOnlySet<string>>
    {
        [UserRole.ItManager] = new HashSet<string>(AllPermissionValues, StringComparer.Ordinal),
        [UserRole.LicenseAdministrator] =
            ToSet(Permissions.Ping, Permissions.AssetsRead, Permissions.LicensesRead, Permissions.LicensesWrite, Permissions.ReportsRead),
        [UserRole.ProcurementSpecialist] =
            ToSet(Permissions.Ping, Permissions.AssetsRead, Permissions.ProcurementRead, Permissions.ProcurementWrite, Permissions.ReportsRead),
        [UserRole.Executive] =
            ToSet(Permissions.Ping, Permissions.AssetsRead, Permissions.ReportsRead),
    };

    public static IReadOnlySet<string> GetPermissions(UserRole role) =>
        Map.TryGetValue(role, out var perms) ? perms : new HashSet<string>(StringComparer.Ordinal);

    private static HashSet<string> ToSet(params string[] items) => new(items, StringComparer.Ordinal);
}
