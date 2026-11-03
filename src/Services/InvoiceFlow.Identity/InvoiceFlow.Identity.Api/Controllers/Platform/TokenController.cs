using InvoiceFlow.BuildingBlocks.Application;
using InvoiceFlow.Identity.Application;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceFlow.Identity.Api.Controllers.Platform;

[ApiController]
[Route("connect")]
[ResponseCache(
    NoStore = true,
    Location = ResponseCacheLocation.None)]
public sealed class TokenController(
    IMediator mediator,
    ISystemDateTimeProvider dateTimeProvider)
    : ApiController
{
    [AllowAnonymous]
    [HttpPost("token")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> Token(
        [FromForm] ClientCredentialsTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (request.GrantType != "client_credentials")
            return OAuthError(
                "unsupported_grant_type",
                StatusCodes.Status400BadRequest);

        if (string.IsNullOrWhiteSpace(request.ClientId) ||
            string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            return OAuthError(
                "invalid_client",
                StatusCodes.Status401Unauthorized);
        }

        if (request.TenantId == Guid.Empty)
        {
            return OAuthError(
                "invalid_request",
                StatusCodes.Status400BadRequest);
        }

        var result = await mediator.Send(
            new GenerateServiceClientTokenCommand(
                request.TenantId,
                request.ClientId,
                request.ClientSecret),
            cancellationToken);

        if (result.IsFailure)
            return MapOAuthError(result.Error!);

        if (result.Value is null)
            return OAuthError(
                "server_error",
                StatusCodes.Status500InternalServerError);

        var token = result.Value;

        var expiresIn = Math.Max(
            0,
            (int)Math.Ceiling(
                (token.AccessTokenExpiresAt -
                 dateTimeProvider.GetUtcNow()).TotalSeconds));

        return Ok(new
        {
            access_token = token.AccessToken,
            token_type = token.TokenType,
            expires_in = expiresIn
        });
    }

    private IActionResult MapOAuthError(ApplicationError error)
    {
        return error.Code switch
        {
            var code when code == ApplicationErrors.InvalidServiceClientCredentials.Code =>
                OAuthError("invalid_client", StatusCodes.Status401Unauthorized),

            var code when code == ApplicationErrors.ServiceClientAccessDenied.Code =>
                OAuthError("unauthorized_client", StatusCodes.Status400BadRequest),

            _ => OAuthError("server_error", StatusCodes.Status500InternalServerError)
        };
    }

    private IActionResult OAuthError(string error, int statusCode)
    {
        return StatusCode(statusCode, new
        {
            error
        });
    }
}
