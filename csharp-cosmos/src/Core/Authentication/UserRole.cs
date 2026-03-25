namespace Todo.Core.Authentication;

/// <summary>
/// Platform roles per WO-2. Serialized as PascalCase strings in configuration.
/// </summary>
public enum UserRole
{
    ItManager,
    LicenseAdministrator,
    ProcurementSpecialist,
    Executive,
}
