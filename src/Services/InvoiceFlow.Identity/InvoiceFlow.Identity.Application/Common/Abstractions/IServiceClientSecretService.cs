using InvoiceFlow.Identity.Domain;

namespace InvoiceFlow.Identity.Application;

public interface IServiceClientSecretService
{
    (string secret, ServiceClientSecretHash hash) GenerateSecretAndHash();

    bool Verify(
        string secret,
        ServiceClientSecretHash expectedHash);
}
