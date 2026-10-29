using System.Security.Cryptography;
using System.Text.Json;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Invoices.Application;
using InvoiceFlow.Invoices.Domain;

namespace InvoiceFlow.Invoices.Infrastructure.Services;

internal sealed class RequestHasher : IRequestHasher
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public Result<RequestHash> Hash<TRequest>(TRequest request)
    {
        if (request is null)
            return Result<RequestHash>.Failure(ApplicationErrors.RequestHashInputRequired);

        var requestBytes = JsonSerializer.SerializeToUtf8Bytes(
            request,
            JsonOptions);

        var hashBytes = SHA256.HashData(requestBytes);
        var hash = Convert.ToHexString(hashBytes);

        return Result<RequestHash>.Success(RequestHash.Create(hash));
    }
}
