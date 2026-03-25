using Microsoft.Extensions.Options;
using Todo.Core.Configuration;
using Todo.Core.Security;
using Xunit;

namespace Todo.Core.Tests;

public sealed class PiiFieldProtectorTests
{
    [Fact]
    public void Protect_round_trips_plaintext()
    {
        var keyB64 = Convert.ToBase64String(new byte[32]);
        var settings = Options.Create(new PiiEncryptionSettings { EncryptionKeyBase64 = keyB64 });
        var sut = new PiiFieldProtector(settings);

        const string plain = "user@example.com";
        var cipher = sut.Protect(plain);
        Assert.NotEqual(plain, cipher);

        var roundTrip = sut.Unprotect(cipher);
        Assert.Equal(plain, roundTrip);
    }

    [Fact]
    public void Protect_empty_returns_empty()
    {
        var keyB64 = Convert.ToBase64String(new byte[32]);
        var settings = Options.Create(new PiiEncryptionSettings { EncryptionKeyBase64 = keyB64 });
        var sut = new PiiFieldProtector(settings);

        Assert.Equal(string.Empty, sut.Protect(string.Empty));
        Assert.Equal(string.Empty, sut.Unprotect(string.Empty));
    }
}
