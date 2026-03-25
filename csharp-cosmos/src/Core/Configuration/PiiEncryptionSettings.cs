namespace Todo.Core.Configuration;

/// <summary>
/// AES-256-GCM field-level encryption for PII stored in Cosmos documents. Key material must come from Key Vault or a secure secret store in production.
/// </summary>
public sealed class PiiEncryptionSettings
{
    public const string SectionName = "PiiEncryption";

    /// <summary>32-byte key as Base64 (256-bit AES key).</summary>
    public string EncryptionKeyBase64 { get; set; } = string.Empty;
}
