using System.Security.Cryptography;
using System.Text.Json;
using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Payments.Application;
using InvoiceFlow.Payments.Domain;

namespace InvoiceFlow.Payments.Infrastructure.Services;

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
