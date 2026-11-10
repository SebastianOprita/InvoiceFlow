using System.Net;

namespace InvoiceFlow.BuildingBlocks.ServiceClient.Exceptions;

public sealed class ServiceClientAuthenticationException : Exception
{
    public HttpStatusCode? StatusCode { get; }
    public string? OAuthError { get; }
    public ServiceClientAuthenticationException(string message, HttpStatusCode? statusCode = null, string? oauthError = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        OAuthError = oauthError;
    }
}
