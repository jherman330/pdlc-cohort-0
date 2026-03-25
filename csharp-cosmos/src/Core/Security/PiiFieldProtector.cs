using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Todo.Core.Configuration;

namespace Todo.Core.Security;

/// <summary>Encrypts and decrypts PII string fields for Cosmos DB using AES-256-GCM.</summary>
public interface IPiiFieldProtector
{
    string Protect(string plaintext);
    string Unprotect(string ciphertext);
}

/// <inheritdoc />
public sealed class PiiFieldProtector : IPiiFieldProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key;

    public PiiFieldProtector(IOptions<PiiEncryptionSettings> options)
    {
        var b64 = options.Value.EncryptionKeyBase64;
        if (string.IsNullOrWhiteSpace(b64))
            throw new InvalidOperationException("PiiEncryption:EncryptionKeyBase64 is required when IPiiFieldProtector is registered.");
        _key = Convert.FromBase64String(b64);
        if (_key.Length != 32)
            throw new InvalidOperationException("PiiEncryption:EncryptionKeyBase64 must decode to exactly 32 bytes (256 bits).");
    }

    /// <inheritdoc />
    public string Protect(string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return plaintext;
        var nonce = new byte[NonceSize];
        RandomNumberGenerator.Fill(nonce);
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[TagSize];
        using (var aes = new AesGcm(_key, TagSize))
        {
            aes.Encrypt(nonce, plainBytes, cipher, tag);
        }

        // nonce | tag | cipher — all base64 for JSON storage
        var combined = new byte[nonce.Length + tag.Length + cipher.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipher, 0, combined, nonce.Length + tag.Length, cipher.Length);
        return Convert.ToBase64String(combined);
    }

    /// <inheritdoc />
    public string Unprotect(string ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext))
            return ciphertext;
        var combined = Convert.FromBase64String(ciphertext);
        if (combined.Length < NonceSize + TagSize)
            throw new CryptographicException("Invalid PII ciphertext length.");
        var nonceLen = NonceSize;
        var tagLen = TagSize;
        var nonce = combined.AsSpan(0, nonceLen);
        var tag = combined.AsSpan(nonceLen, tagLen);
        var cipher = combined.AsSpan(nonceLen + tagLen);
        var plain = new byte[cipher.Length];
        using (var aes = new AesGcm(_key, TagSize))
        {
            aes.Decrypt(nonce, cipher, tag, plain);
        }

        return Encoding.UTF8.GetString(plain);
    }
}
