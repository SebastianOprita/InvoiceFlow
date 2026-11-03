using System.Security.Cryptography;
using System.Text;
using InvoiceFlow.Identity.Application;
using InvoiceFlow.Identity.Domain;
using Microsoft.AspNetCore.WebUtilities;

namespace InvoiceFlow.Identity.Infrastructure;

internal sealed class ServiceClientSecretService : IServiceClientSecretService
{
    public (string secret, ServiceClientSecretHash hash) GenerateSecretAndHash()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        var secret = WebEncoders.Base64UrlEncode(bytes);

        var hash = Hash(secret);

        return (secret, hash);
    }

    private ServiceClientSecretHash Hash(string secret)
    {
        var bytes = Encoding.UTF8.GetBytes(secret);
        var hash = SHA256.HashData(bytes);

        return ServiceClientSecretHash.Create(
            Convert.ToHexString(hash));
    }

    public bool Verify(
        string? secret,
        ServiceClientSecretHash? expectedHash)
    {
        if (string.IsNullOrEmpty(secret) || expectedHash is null)
            return false;

        var actualHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(secret));

        var expectedBytes = Convert.FromHexString(
            expectedHash.Value);

        return CryptographicOperations.FixedTimeEquals(
            actualHash,
            expectedBytes);
    }
}
